# Solução de Problemas - Conexão RabbitMQ Externo

## Problema
O pod Kubernetes não consegue resolver o hostname `toucan.lmq.cloudamqp.com`, resultando no erro:
```
System.Net.Sockets.SocketException: Name or service not known
```

## Soluções

### 1. Verificar DNS do Cluster (Recomendado)

Teste a resolução DNS dentro do pod:

```bash
kubectl exec -it deployment/fcg-pagamentos -- nslookup toucan.lmq.cloudamqp.com
```

Se falhar, verifique o CoreDNS:

```bash
kubectl get pods -n kube-system | grep coredns
kubectl logs -n kube-system <coredns-pod-name>
```

### 2. Configurar DNS Personalizado no Deployment

Adicione configuração de DNS no deployment.yaml:

```yaml
spec:
  template:
    spec:
      dnsPolicy: "None"
      dnsConfig:
        nameservers:
          - "8.8.8.8"
          - "8.8.4.4"
        searches:
          - "default.svc.cluster.local"
          - "svc.cluster.local"
          - "cluster.local"
```

### 3. Usar IP Direto (Temporário)

Se o DNS continuar falhando, você pode obter o IP do hostname e usar diretamente:

```bash
# Obter IP do hostname
nslookup toucan.lmq.cloudamqp.com

# Atualizar deployment com IP ao invés de hostname
--RabbitMQ__HostName=<IP_DO_RABBITMQ>
```

**⚠️ AVISO**: IPs podem mudar, então isso é apenas uma solução temporária.

### 4. Verificar Network Policies

Certifique-se de que não há Network Policies bloqueando conexões de saída:

```bash
kubectl get networkpolicies --all-namespaces
```

### 5. Verificar Firewall/Security Groups

- Certifique-se de que o cluster tem acesso à internet
- Verifique se a porta 5671 (AMQPS) não está bloqueada
- Verifique se há regras de firewall bloqueando conexões de saída

### 6. Testar Conectividade de Rede

Execute dentro do pod:

```bash
kubectl exec -it deployment/fcg-pagamentos -- sh
# Dentro do pod:
telnet toucan.lmq.cloudamqp.com 5671
# ou
nc -zv toucan.lmq.cloudamqp.com 5671
```

### 7. Configurações de SSL Corretas

O código agora suporta SSL. Certifique-se de que o deployment tem:

```yaml
- "--RabbitMQ__UseSsl=true"
- "--RabbitMQ__SslServerName=toucan.lmq.cloudamqp.com"
- "--RabbitMQ__VirtualHost=evqzezjc"  # Geralmente o mesmo que UserName no CloudAMQP
```

## Verificação Rápida

1. ✅ SSL configurado (UseSsl=true, Port=5671)
2. ✅ VirtualHost correto (geralmente o mesmo que UserName no CloudAMQP)
3. ✅ Credenciais corretas
4. ⚠️ DNS funcionando (teste com nslookup)
5. ⚠️ Conectividade de rede (teste com telnet/nc)
6. ⚠️ Sem bloqueios de firewall
