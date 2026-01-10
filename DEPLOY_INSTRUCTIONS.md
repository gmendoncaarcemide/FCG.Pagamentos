# Instruções para Deploy e Correção do Problema RabbitMQ

## Problema Identificado

O pod Kubernetes não consegue resolver o DNS do RabbitMQ externo (`toucan.lmq.cloudamqp.com`), causando falha na inicialização da aplicação.

## Correções Aplicadas

### 1. Código C# - Inicialização Lazy com Retry
- ✅ `RabbitMQEventBus` agora usa inicialização lazy (não bloqueia startup)
- ✅ Retry automático com backoff exponencial (5 tentativas)
- ✅ App continua funcionando mesmo se RabbitMQ não estiver disponível inicialmente
- ✅ Logs detalhados para diagnóstico

### 2. Kubernetes Deployment - Configuração DNS
- ✅ DNS customizado usando Google DNS (8.8.8.8, 8.8.4.4)
- ✅ Configuração SSL correta para CloudAMQP
- ✅ VirtualHost configurado corretamente

## Passos para Deploy

### 1. Rebuild da Imagem Docker

```bash
# Build da nova imagem
docker build -t fabriciorosanet/fcg-pagamentos-api:latest .

# Push para o registry
docker push fabriciorosanet/fcg-pagamentos-api:latest
```

**⚠️ IMPORTANTE**: Certifique-se de que a imagem foi rebuild com o código atualizado que inclui:
- Inicialização lazy do RabbitMQ
- Suporte a SSL
- Retry automático

### 2. Aplicar Deployment no Kubernetes

```bash
# Aplicar deployment atualizado
kubectl apply -f k8s/deployment.yaml

# Verificar se o pod foi criado/atualizado
kubectl get pods -l app=fcg-pagamentos
```

### 3. Usar Script de Diagnóstico

```bash
# Tornar script executável (Linux/Mac)
chmod +x k8s/deploy-and-diagnose.sh

# Executar diagnóstico
./k8s/deploy-and-diagnose.sh
```

### 4. Testar DNS Manualmente

Se o script não funcionar, teste manualmente:

```bash
# Obter nome do pod
POD_NAME=$(kubectl get pods -l app=fcg-pagamentos -o jsonpath='{.items[0].metadata.name}')

# Testar DNS
kubectl exec $POD_NAME -- nslookup toucan.lmq.cloudamqp.com

# Testar conectividade
kubectl exec $POD_NAME -- nc -zv toucan.lmq.cloudamqp.com 5671

# Ver configuração DNS do pod
kubectl exec $POD_NAME -- cat /etc/resolv.conf
```

### 5. Verificar Logs

```bash
# Ver logs em tempo real
kubectl logs -f deployment/fcg-pagamentos

# Ou se o pod já existe
kubectl logs -f <pod-name>
```

## Verificações Esperadas

Após o deploy, você deve ver nos logs:

✅ **Sucesso**: 
```
[INFO] Attempting to connect to RabbitMQ at toucan.lmq.cloudamqp.com:5671 with SSL=True
[INFO] RabbitMQ connection established successfully at toucan.lmq.cloudamqp.com:5671
```

⚠️ **Retry (normal na primeira tentativa)**:
```
[WARN] Failed to initialize RabbitMQ connection (attempt 1/5). Will retry in 2 seconds.
[WARN] Failed to initialize RabbitMQ connection (attempt 2/5). Will retry in 4 seconds.
[INFO] RabbitMQ connection established successfully...
```

❌ **Problema DNS (se ainda ocorrer)**:
```
[WARN] Failed to initialize RabbitMQ connection... Error: Name or service not known
```

## Soluções Alternativas se DNS Ainda Falhar

### Opção 1: Usar IP Direto (Temporário)

Se o DNS continuar falhando, você pode obter o IP e usar diretamente:

```bash
# Obter IP do hostname
nslookup toucan.lmq.cloudamqp.com
# ou
dig +short toucan.lmq.cloudamqp.com
```

Depois atualizar o deployment:

```yaml
- "--RabbitMQ__HostName=<IP_DO_RABBITMQ>"
```

**⚠️ AVISO**: IPs podem mudar, use apenas como solução temporária.

### Opção 2: Verificar CoreDNS do Cluster

```bash
# Verificar se CoreDNS está rodando
kubectl get pods -n kube-system | grep coredns

# Ver logs do CoreDNS
kubectl logs -n kube-system <coredns-pod-name>
```

### Opção 3: Configurar ExternalName Service

Criar um Service do tipo ExternalName no Kubernetes:

```yaml
apiVersion: v1
kind: Service
metadata:
  name: rabbitmq-external
spec:
  type: ExternalName
  externalName: toucan.lmq.cloudamqp.com
```

E usar no deployment:
```yaml
- "--RabbitMQ__HostName=rabbitmq-external"
```

## Troubleshooting

### O app não inicia
- Verificar se a imagem foi rebuild corretamente
- Verificar logs do pod: `kubectl logs <pod-name>`
- Verificar eventos: `kubectl describe pod <pod-name>`

### DNS ainda falha
- Verificar se o deployment foi aplicado: `kubectl get deployment fcg-pagamentos -o yaml | grep -A 10 dnsConfig`
- Verificar se há Network Policies bloqueando: `kubectl get networkpolicies --all-namespaces`
- Verificar conectividade de internet do cluster

### Conexão SSL falha
- Verificar se `UseSsl=true` está configurado
- Verificar se porta 5671 está acessível (não bloqueada por firewall)
- Verificar logs SSL nos logs do pod

## Próximos Passos

1. ✅ Rebuild e push da imagem Docker
2. ✅ Aplicar deployment atualizado
3. ✅ Monitorar logs para verificar conexão
4. ⚠️ Se DNS ainda falhar, usar IP direto ou configurar ExternalName Service
5. ✅ Verificar se eventos estão sendo publicados corretamente
