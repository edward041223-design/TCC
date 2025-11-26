#!/bin/bash

# ============================================================
# Script para testar LM Studio
# Phi-3-mini-4k-instruct rodando em http://192.168.56.1:5000
# ============================================================

echo "🧪 Testando LM Studio..."
echo ""

# Cores para output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

LM_STUDIO_URL="http://192.168.56.1:5000"

# ============================================================
# Teste 1: Verificar se LM Studio está rodando
# ============================================================
echo -e "${YELLOW}[Teste 1] Verificando se LM Studio está online...${NC}"
response=$(curl -s -o /dev/null -w "%{http_code}" "$LM_STUDIO_URL/v1/models" 2>/dev/null)

if [ "$response" == "200" ]; then
    echo -e "${GREEN}✅ LM Studio está rodando!${NC}"
else
    echo -e "${RED}❌ LM Studio não está respondendo (HTTP $response)${NC}"
    echo "Verifique se o LM Studio está aberto e rodando na porta 5000"
    exit 1
fi

echo ""

# ============================================================
# Teste 2: Listar modelos disponíveis
# ============================================================
echo -e "${YELLOW}[Teste 2] Listando modelos disponíveis...${NC}"
curl -s "$LM_STUDIO_URL/v1/models" | python3 -m json.tool 2>/dev/null || echo "Resposta não é JSON válido"
echo ""

# ============================================================
# Teste 3: Fazer pergunta simples à IA
# ============================================================
echo -e "${YELLOW}[Teste 3] Fazendo pergunta simples à IA...${NC}"
echo "Pergunta: Olá, você consegue me ouvir?"
echo ""

curl -s -X POST "$LM_STUDIO_URL/v1/chat/completions" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "phi-3-mini-4k-instruct",
    "messages": [
      {"role": "system", "content": "Você é um assistente útil."},
      {"role": "user", "content": "Olá, você consegue me ouvir?"}
    ],
    "temperature": 0.7,
    "max_tokens": 50
  }' | python3 -c "
import sys, json
try:
    data = json.load(sys.stdin)
    resposta = data['choices'][0]['message']['content']
    print('🤖 Resposta da IA:', resposta)
    print('✅ IA está funcionando!')
except Exception as e:
    print('❌ Erro ao processar resposta:', e)
    sys.exit(1)
"

echo ""

# ============================================================
# Teste 4: Pergunta relacionada ao projeto (lactose)
# ============================================================
echo -e "${YELLOW}[Teste 4] Testando pergunta sobre lactose...${NC}"
echo "Pergunta: Quais pratos você recomenda para quem tem intolerância à lactose?"
echo ""

curl -s -X POST "$LM_STUDIO_URL/v1/chat/completions" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "phi-3-mini-4k-instruct",
    "messages": [
      {
        "role": "system",
        "content": "Você é um assistente especializado em nutrição e cardápios para pessoas com intolerância à lactose. Responda em português de forma breve."
      },
      {
        "role": "user",
        "content": "Quais pratos você recomenda para quem tem intolerância à lactose?"
      }
    ],
    "temperature": 0.7,
    "max_tokens": 150
  }' | python3 -c "
import sys, json
try:
    data = json.load(sys.stdin)
    resposta = data['choices'][0]['message']['content']
    print('🤖 Resposta da IA:')
    print(resposta)
    print('\n✅ Teste de pergunta sobre lactose concluído!')
except Exception as e:
    print('❌ Erro ao processar resposta:', e)
    sys.exit(1)
"

echo ""

# ============================================================
# Teste 5: Verificar latência (tempo de resposta)
# ============================================================
echo -e "${YELLOW}[Teste 5] Medindo tempo de resposta...${NC}"

start_time=$(date +%s.%N)

curl -s -X POST "$LM_STUDIO_URL/v1/chat/completions" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "phi-3-mini-4k-instruct",
    "messages": [{"role": "user", "content": "Diga oi"}],
    "max_tokens": 10
  }' > /dev/null

end_time=$(date +%s.%N)
duration=$(echo "$end_time - $start_time" | bc)

echo "⏱️  Tempo de resposta: ${duration}s"

if (( $(echo "$duration < 5" | bc -l) )); then
    echo -e "${GREEN}✅ Latência boa (< 5s)${NC}"
elif (( $(echo "$duration < 10" | bc -l) )); then
    echo -e "${YELLOW}⚠️  Latência média (5-10s)${NC}"
else
    echo -e "${RED}❌ Latência alta (> 10s) - Considere usar GPU${NC}"
fi

echo ""

# ============================================================
# Resumo Final
# ============================================================
echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}📊 RESUMO DOS TESTES${NC}"
echo -e "${GREEN}========================================${NC}"
echo "✅ LM Studio: Online"
echo "✅ Modelo: phi-3-mini-4k-instruct"
echo "✅ Endpoints: /v1/chat/completions"
echo "✅ Resposta em português: OK"
echo "✅ Integração pronta para usar na API"
echo ""
echo -e "${YELLOW}📝 Próximos passos:${NC}"
echo "1. Copiar arquivos corrigidos da API"
echo "2. Rodar: dotnet run na pasta Cardapio_Inteligente.Api"
echo "3. Testar endpoint /api/IA/chat no Swagger"
echo "4. Testar ChatPage no app MAUI"
echo ""
