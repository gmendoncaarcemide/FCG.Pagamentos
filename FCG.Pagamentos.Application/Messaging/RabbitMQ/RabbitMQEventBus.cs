using FCG.Pagamentos.Application.Messaging.Configuration;
using FCG.Pagamentos.Application.Messaging.Events;
using FCG.Pagamentos.Application.Messaging.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Text;

namespace FCG.Pagamentos.Application.Messaging.RabbitMQ;

public class RabbitMQEventBus : IEventBus, IDisposable
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly Dictionary<string, Type> _eventTypes = new();
    private readonly Dictionary<string, Type> _handlerTypes = new();
    private readonly object _lock = new object();
    private bool _isInitialized = false;
    private const string ExchangeName = "fcg_events";

    public RabbitMQEventBus(
        IOptions<RabbitMQSettings> settings,
        ILogger<RabbitMQEventBus> logger,
        IServiceProvider serviceProvider)
    {
        _settings = settings.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
        // Não inicializa no construtor para não bloquear startup
        _logger.LogInformation("RabbitMQEventBus created. Connection will be established on first use.");
    }

    private void EnsureInitialized()
    {
        if (_isInitialized && _connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
        {
            return;
        }

        lock (_lock)
        {
            if (_isInitialized && _connection != null && _connection.IsOpen && _channel != null && _channel.IsOpen)
            {
                return;
            }

            InitializeRabbitMQWithRetry();
        }
    }

    private void InitializeRabbitMQWithRetry()
    {
        var maxRetries = 5;
        var delaySeconds = 2;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation(
                    "Attempting to connect to RabbitMQ at {HostName}:{Port} with SSL={UseSsl} (attempt {Attempt}/{MaxRetries})",
                    _settings.HostName, _settings.Port, _settings.UseSsl, attempt, maxRetries);

                var factory = new ConnectionFactory
                {
                    HostName = _settings.HostName,
                    Port = _settings.Port,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    VirtualHost = _settings.VirtualHost,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(10)
                };

                // Configure SSL if enabled
                if (_settings.UseSsl)
                {
                    factory.Ssl = new SslOption
                    {
                        Enabled = true,
                        ServerName = _settings.SslServerName ?? _settings.HostName,
                        CertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
                        {
                            if (sslPolicyErrors == SslPolicyErrors.None)
                            {
                                return true;
                            }

                            _logger.LogWarning(
                                "SSL certificate validation errors: {Errors}. Accepting certificate anyway.",
                                sslPolicyErrors);
                            return true;
                        }
                    };
                }

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.ExchangeDeclare(
                    exchange: ExchangeName,
                    type: ExchangeType.Topic,
                    durable: true,
                    autoDelete: false);

                _isInitialized = true;

                _logger.LogInformation(
                    "RabbitMQ connection established successfully at {HostName}:{Port}",
                    _settings.HostName, _settings.Port);
                
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, 
                    "Failed to initialize RabbitMQ connection to {HostName}:{Port} (attempt {Attempt}/{MaxRetries}). " +
                    "Will retry in {DelaySeconds} seconds. Error: {ErrorMessage}",
                    _settings.HostName, _settings.Port, attempt, maxRetries, delaySeconds, ex.Message);

                // Cleanup failed connection
                try
                {
                    _channel?.Close();
                    _channel?.Dispose();
                    _connection?.Close();
                    _connection?.Dispose();
                }
                catch { }

                _channel = null;
                _connection = null;
                _isInitialized = false;

                if (attempt < maxRetries)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
                    delaySeconds *= 2; // Exponential backoff
                }
                else
                {
                    _logger.LogError(
                        "Failed to connect to RabbitMQ after {MaxRetries} attempts. " +
                        "The application will continue to run but events cannot be published. " +
                        "Check DNS resolution, network connectivity, firewall rules, and SSL configuration. " +
                        "Hostname: {HostName}, Port: {Port}",
                        maxRetries, _settings.HostName, _settings.Port);
                    // Não lança exceção - permite que o app continue funcionando
                }
            }
        }
    }

    private void InitializeRabbitMQ()
    {
        EnsureInitialized();
        
        if (!_isInitialized)
        {
            throw new InvalidOperationException(
                $"Cannot establish RabbitMQ connection to {_settings.HostName}:{_settings.Port}. " +
                "Please check DNS resolution, network connectivity, and firewall rules.");
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event, string? routingKey = null) where TEvent : IntegrationEvent
    {
        try
        {
            EnsureInitialized();

            if (!_isInitialized || _channel == null || !_channel.IsOpen)
            {
                _logger.LogWarning(
                    "RabbitMQ is not available. Event {EventName} with ID {EventId} will not be published.",
                    typeof(TEvent).Name, @event.Id);
                return;
            }

            var eventName = @event.GetType().Name;
            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            var properties = _channel!.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Type = eventName;
            properties.MessageId = @event.Id.ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            var finalRoutingKey = routingKey ?? eventName;

            for (int retry = 0; retry <= _settings.RetryCount; retry++)
            {
                try
                {
                    _channel.BasicPublish(
                        exchange: ExchangeName,
                        routingKey: finalRoutingKey,
                        basicProperties: properties,
                        body: body);

                    _logger.LogInformation(
                        "Published event {EventName} with ID {EventId} to routing key {RoutingKey}",
                        eventName, @event.Id, finalRoutingKey);

                    await Task.CompletedTask;
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, 
                        "Error publishing event {EventName} (attempt {Retry}/{MaxRetries})",
                        eventName, retry + 1, _settings.RetryCount + 1);

                    if (retry == _settings.RetryCount)
                        throw;

                    await Task.Delay(_settings.RetryDelayMilliseconds);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Unexpected error publishing event {EventName} with ID {EventId}",
                typeof(TEvent).Name, @event.Id);
            throw;
        }
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IEventHandler<TEvent>
    {
        try
        {
            var eventName = typeof(TEvent).Name;
            var queueName = $"{eventName}_queue";

            _eventTypes[eventName] = typeof(TEvent);
            _handlerTypes[eventName] = typeof(THandler);

            EnsureInitialized();

            if (!_isInitialized || _channel == null || !_channel.IsOpen)
            {
                _logger.LogWarning(
                    "RabbitMQ is not available. Cannot subscribe to event {EventName}. " +
                    "Subscription will be retried when connection is available.",
                    eventName);
                return;
            }

        _channel!.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(
            queue: queueName,
            exchange: ExchangeName,
            routingKey: eventName);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var eventName = ea.RoutingKey;
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                await ProcessEvent(eventName, message);
                _channel.BasicAck(ea.DeliveryTag, false);
                
                _logger.LogInformation(
                    "Successfully processed event {EventName} with delivery tag {DeliveryTag}",
                    eventName, ea.DeliveryTag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error processing event {EventName}. Message: {Message}",
                    eventName, message);

                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(
            queue: queueName,
            autoAck: false,
            consumer: consumer);

            _logger.LogInformation(
                "Subscribed to event {EventName} with handler {HandlerName}",
                eventName, typeof(THandler).Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error subscribing to event {EventName} with handler {HandlerName}",
                typeof(TEvent).Name, typeof(THandler).Name);
            // Não lança exceção para não impedir que o app inicie
        }
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        if (!_eventTypes.TryGetValue(eventName, out var eventType))
        {
            _logger.LogWarning("No event type registered for {EventName}", eventName);
            return;
        }

        if (!_handlerTypes.TryGetValue(eventName, out var handlerType))
        {
            _logger.LogWarning("No handler registered for {EventName}", eventName);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var @event = JsonConvert.DeserializeObject(message, eventType);
        var handler = scope.ServiceProvider.GetService(handlerType);

        if (handler == null)
        {
            _logger.LogError("Could not resolve handler {HandlerType}", handlerType.Name);
            return;
        }

        var handleMethod = handlerType.GetMethod("HandleAsync");
        if (handleMethod != null)
        {
            await (Task)handleMethod.Invoke(handler, new[] { @event })!;
        }
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        
        _logger.LogInformation("RabbitMQ connection disposed");
    }
}
