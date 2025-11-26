# 🚀 GUIA RÁPIDO: Como Aplicar as Correções

**Prazo:** 2 dias (25-27 de novembro)  
**Tempo estimado para aplicar correções:** 2-3 horas

---

## 📁 ESTRUTURA DOS ARQUIVOS CORRIGIDOS

```
/home/user/TCC_CORRIGIDO/
├── ANALISE_E_CORRECOES.md          (📄 Análise completa - LEIA PRIMEIRO)
├── COMO_APLICAR_CORRECOES.md       (📄 Este arquivo)
├── popular_banco.sql               (🗄️ Script SQL para popular banco)
├── testar_lm_studio.sh             (🧪 Script de teste do LM Studio)
│
└── Cardapio_Inteligente.Api/
    ├── Servicos/
    │   └── LMStudioService.cs      (✅ NOVO - Integração com LM Studio)
    ├── Program.cs                  (✅ CORRIGIDO)
    ├── appsettings.json            (✅ CORRIGIDO)
    └── Cardapio_Inteligente.Api.csproj  (✅ CORRIGIDO)
```

---

## ⚡ PASSO A PASSO RÁPIDO (30 minutos)

### **1. BACKUP DO PROJETO ORIGINAL (5 min)**

```bash
# No seu computador Windows, copie a pasta inteira:
# C:\Projetos\TCC → C:\Projetos\TCC_BACKUP_25NOV

# Ou use Git:
cd C:\Projetos\TCC
git add .
git commit -m "Backup antes das correções - 25/11"
```

---

### **2. APLICAR CORREÇÕES NA API (15 min)**

#### **2.1. Copiar arquivo NOVO:**

📁 **LMStudioService.cs**

```
DE: /home/user/TCC_CORRIGIDO/Cardapio_Inteligente.Api/Servicos/LMStudioService.cs
PARA: SEU_PROJETO/Cardapio_Inteligente.Api/Servicos/LMStudioService.cs
```

Ação: Criar arquivo novo com o conteúdo fornecido

---

#### **2.2. SUBSTITUIR arquivos existentes:**

📁 **Program.cs**

```
DE: /home/user/TCC_CORRIGIDO/Cardapio_Inteligente.Api/Program.cs
PARA: SEU_PROJETO/Cardapio_Inteligente.Api/Program.cs
```

⚠️ **IMPORTANTE:** Faça backup do Program.cs original antes!

---

📁 **appsettings.json**

```
DE: /home/user/TCC_CORRIGIDO/Cardapio_Inteligente.Api/appsettings.json
PARA: SEU_PROJETO/Cardapio_Inteligente.Api/appsettings.json
```

⚠️ **ATENÇÃO:** Ajuste a ConnectionString se sua senha do MySQL for diferente!

---

📁 **Cardapio_Inteligente.Api.csproj**

```
DE: /home/user/TCC_CORRIGIDO/Cardapio_Inteligente.Api/Cardapio_Inteligente.Api.csproj
PARA: SEU_PROJETO/Cardapio_Inteligente.Api/Cardapio_Inteligente.Api.csproj
```

---

### **3. REMOVER ARQUIVOS ANTIGOS (2 min)**

❌ **DELETAR (não são mais usados):**

```
SEU_PROJETO/Cardapio_Inteligente.Api/Servicos/LlamaService.cs
SEU_PROJETO/Cardapio_Inteligente.Api/Configuracao/LlamaSettings.cs (opcional manter)
SEU_PROJETO/Cardapio_Inteligente.Api/ModelosIA/Phi-3-mini-4k-instruct-q4.gguf (2GB!)
```

---

### **4. RESTAURAR PACOTES (5 min)**

Abra o terminal na pasta da API:

```bash
cd SEU_PROJETO/Cardapio_Inteligente.Api

# Limpar pacotes antigos
dotnet clean

# Restaurar pacotes novos (sem LLamaSharp)
dotnet restore

# Verificar se compilou
dotnet build
```

**Espere ver:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

### **5. CONFIGURAR MYSQL (5 min)**

#### **5.1. Verificar se MySQL está rodando:**

- Abra MySQL Workbench
- Conecte em `localhost:3306`
- Usuário: `root`
- Senha: (sua senha ou deixe em branco)

#### **5.2. Popular banco de dados:**

```sql
-- Copie o conteúdo de popular_banco.sql
-- Cole no MySQL Workbench
-- Execute (Ctrl+Shift+Enter)
```

**Verificar se funcionou:**
```sql
USE cardapio_db;
SELECT COUNT(*) FROM Pratos; -- Deve retornar 25+
```

---

### **6. TESTAR LM STUDIO (3 min)**

#### **6.1. Verificar se LM Studio está rodando:**

- Abra LM Studio
- Clique em "Local Server"
- Verifique se está em `http://192.168.56.1:5000` ou `localhost:5000`
- Modelo carregado: `phi-3-mini-4k-instruct`

#### **6.2. Testar manualmente (opcional):**

Abra Postman ou navegador e teste:

```
GET http://192.168.56.1:5000/v1/models
```

Deve retornar lista de modelos.

---

### **7. RODAR A API (2 min)**

```bash
cd SEU_PROJETO/Cardapio_Inteligente.Api
dotnet run
```

**Espere ver:**
```
✅ Banco de dados verificado/criado com sucesso.
✅ Serviço LM Studio inicializado.
🚀 API Cardápio Inteligente iniciada com sucesso!
🔗 LM Studio: http://192.168.56.1:5000
Now listening on: http://localhost:5068
```

---

### **8. TESTAR NO SWAGGER (3 min)**

1. Abra navegador: `http://localhost:5068/swagger`

2. Teste endpoint de IA:
   - Encontre `POST /api/IA/chat` (ou similar)
   - Clique em "Try it out"
   - Body: `{ "mensagem": "Olá, você funciona?" }`
   - Execute

**Esperado:** Resposta da IA em português

---

## 🔧 CORREÇÕES NO APP MAUI (Opcional - se necessário)

O App MAUI já está configurado corretamente na maioria dos casos. Apenas verifique:

### **Verificar ApiService.cs:**

```csharp
// Deve estar usando http://localhost:5068 (Windows)
// Ou http://10.0.2.2:5068 (Android Emulator)
```

Se precisar ajustar, edite:

```
SEU_PROJETO/Cardapio_Inteligente/servicos/ApiService.cs
```

Nas linhas 69-84 (GetBaseAddressesForPlatform).

---

## ✅ CHECKLIST DE VERIFICAÇÃO

Após aplicar as correções, verifique:

- [ ] `dotnet build` sem erros
- [ ] MySQL conectando (veja logs da API)
- [ ] LM Studio respondendo (teste no Swagger)
- [ ] API iniciando na porta 5068
- [ ] Pratos cadastrados no banco (SELECT * FROM Pratos)
- [ ] Swagger acessível em http://localhost:5068/swagger

---

## 🐛 PROBLEMAS COMUNS

### **Erro: "Connection string 'DefaultConnection' not found"**

**Solução:** Verifique appsettings.json:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=cardapio_db;User=root;Password=SUA_SENHA;"
  }
}
```

Substitua `SUA_SENHA` pela senha do seu MySQL (ou deixe vazio se não tiver senha).

---

### **Erro: "Unable to connect to MySQL server"**

**Soluções:**
1. Verifique se MySQL está rodando (Services → MySQL)
2. Teste conexão no MySQL Workbench
3. Verifique firewall do Windows

---

### **Erro: "Não foi possível conectar ao LM Studio"**

**Soluções:**
1. Abra LM Studio e inicie o servidor local
2. Verifique a URL em appsettings.json:
   - Se LM Studio mostra `localhost:1234`, use `http://localhost:1234`
   - Se mostra IP específico, use aquele IP
3. Teste no navegador: `http://SEU_IP:PORTA/v1/models`

---

### **Erro: "The type or namespace name 'LLamaSharp' could not be found"**

**Solução:** Você esqueceu de atualizar o .csproj. Copie novamente:

```bash
cp TCC_CORRIGIDO/.../Cardapio_Inteligente.Api.csproj SEU_PROJETO/.../
dotnet restore
```

---

## 📊 TESTES FINAIS

### **1. Teste da API isoladamente:**

```bash
# Terminal 1: Rodar API
cd Cardapio_Inteligente.Api
dotnet run

# Terminal 2: Testar endpoint
curl -X POST http://localhost:5068/api/IA/chat \
  -H "Content-Type: application/json" \
  -d '{"mensagem": "Quais pratos sem lactose você recomenda?"}'
```

---

### **2. Teste do App MAUI:**

1. Abra Visual Studio
2. Selecione projeto `Cardapio_Inteligente`
3. Target: `net8.0-android` ou `net8.0-windows`
4. F5 (Run)

**Fluxo de teste:**
- Fazer cadastro
- Fazer login
- Ir para ChatPage
- Perguntar: "Quais pratos sem lactose?"
- Verificar resposta da IA

---

## 📞 SUPORTE RÁPIDO

**Se algo não funcionar:**

1. **Verifique logs do console:**
   - API mostra todos os logs em tempo real
   - Procure por ❌ (erros) ou ⚠️ (avisos)

2. **Principais logs de sucesso:**
   ```
   ✅ Banco de dados verificado
   ✅ Serviço LM Studio inicializado
   🚀 API iniciada com sucesso
   ```

3. **Teste componentes isoladamente:**
   - MySQL: Execute `SELECT 1` no Workbench
   - LM Studio: Abra `http://IP:PORTA/v1/models` no navegador
   - API: Acesse Swagger

---

## 🎯 PRÓXIMOS PASSOS

Após tudo funcionando:

1. **Documentar no TCC:**
   - Screenshots da API rodando
   - Screenshots do App funcionando
   - Diagrama de arquitetura (já fornecido)

2. **Preparar apresentação:**
   - Demo ao vivo do app
   - Explicar integração com LM Studio
   - Mostrar código relevante

3. **Testes finais:**
   - Android: Deploy no emulador/device
   - Windows: Rodar no desktop
   - Screenshots de tudo

---

## 📝 RESUMO DAS MUDANÇAS

| Antes | Depois |
|-------|--------|
| LLamaSharp (local, pesado) | LM Studio via HTTP |
| Modelo .gguf no projeto (2GB) | Modelo externo no LM Studio |
| Configuração complexa GPU/CPU | Configuração simples (URL) |
| Build lento | Build rápido |
| Difícil de debugar | Fácil (logs HTTP) |

---

**Tempo total estimado:** 30-60 minutos  
**Dificuldade:** Média  
**Impacto:** CRÍTICO (resolve problema principal do TCC)

---

**BOA SORTE! 🍀**

Se precisar de ajuda, verifique os logs e consulte ANALISE_E_CORRECOES.md para mais detalhes.
