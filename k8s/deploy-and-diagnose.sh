#!/bin/bash

# Script para deploy e diagnóstico do RabbitMQ no Kubernetes

echo "=== Deploy do FCG Pagamentos ==="

# Aplicar deployment
echo "Aplicando deployment..."
kubectl apply -f deployment.yaml

# Aguardar pod ficar pronto
echo "Aguardando pod ficar pronto..."
kubectl wait --for=condition=ready pod -l app=fcg-pagamentos --timeout=120s

# Obter nome do pod
POD_NAME=$(kubectl get pods -l app=fcg-pagamentos -o jsonpath='{.items[0].metadata.name}')

if [ -z "$POD_NAME" ]; then
    echo "Erro: Não foi possível encontrar o pod"
    exit 1
fi

echo "Pod encontrado: $POD_NAME"

# Aguardar um pouco para o pod iniciar completamente
sleep 5

echo ""
echo "=== Testando DNS ==="
echo "Testando resolução do hostname toucan.lmq.cloudamqp.com..."

kubectl exec $POD_NAME -- nslookup toucan.lmq.cloudamqp.com || echo "AVISO: nslookup falhou, tentando getent..."
kubectl exec $POD_NAME -- getent hosts toucan.lmq.cloudamqp.com || echo "AVISO: getent hosts falhou"

echo ""
echo "=== Testando Conectividade ==="
echo "Testando conexão TCP na porta 5671..."

kubectl exec $POD_NAME -- sh -c "nc -zv -w 5 toucan.lmq.cloudamqp.com 5671 2>&1 || echo 'Falha na conexão TCP'"

echo ""
echo "=== Verificando DNS Config ==="
echo "Verificando configuração DNS do pod..."

kubectl exec $POD_NAME -- cat /etc/resolv.conf

echo ""
echo "=== Logs do Pod ==="
echo "Últimas 50 linhas dos logs:"
kubectl logs $POD_NAME --tail=50

echo ""
echo "=== Informações do Pod ==="
kubectl describe pod $POD_NAME | grep -A 10 "DNS\|Environment\|Args"

echo ""
echo "=== Para monitorar logs em tempo real, execute: ==="
echo "kubectl logs -f $POD_NAME"
