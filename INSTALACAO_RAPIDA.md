# ⚡ GUIA RÁPIDO DE INSTALAÇÃO - 15 MINUTOS

**Prazo:** 2 dias restantes  
**Tempo:** 15 minutos para deixar tudo funcionando

---

## 📋 PRÉ-REQUISITOS

✅ MySQL instalado e rodando  
✅ LM Studio aberto com Phi-3-mini carregado em http://192.168.56.1:5000  
✅ Visual Studio 2022 com .NET 8.0  
✅ Projeto TCC original com backup feito

---

## 🚀 INSTALAÇÃO RÁPIDA

### **PASSO 1: Restaurar Banco de Dados (3 min)**

```bash
# Abra MySQL Workbench ou terminal MySQL
mysql -u root -p

# No console MySQL:
source /caminho/para/dump20251124.sql

# Ou copie e cole o conteúdo no MySQL Workbench e execute
```

**Verificar:**
```sql
USE cardapio_db;
SELECT COUNT(*) FROM pratos;    -- Deve retornar 16
SELECT COUNT(*) FROM usuarios;  -- Deve retornar ~19
```

✅ **Banco possui:**
- 16 pratos cadastrados (alguns com lactose, outros sem)
- 19 usuários de teste
- Tabelas: pratos, usuarios, __efmigrationshistory

---

### **PASSO 2: Atualizar Arquivos da API (5 min)**

**Copiar 4 arquivos:**

```
1. TCC_CORRIGIDO/Cardapio_Inteligente.Api/Servicos/LMStudioService.cs
   → SEU_PROJETO/Cardapio_Inteligente.Api/Servicos/LMStudioService.cs (CRIAR)

2. TCC_CORRIGIDO/Cardapio_Inteligente.Api/Program.cs
   → SEU_PROJETO/Cardapio_Inteligente.Api/Program.cs (SUBSTITUIR)

3. TCC_CORRIGIDO/Cardapio_Inteligente.Api/appsettings.json
   → SEU_PROJETO/Cardapio_Inteligente.Api/appsettings.json (SUBSTITUIR)

4. TCC_CORRIGIDO/Cardapio_Inteligente.Api/Cardapio_Inteligente.Api.csproj
   → SEU_PROJETO/Cardapio_Inteligente.Api/Cardapio_Inteligente.Api.csproj (SUBSTITUIR)
```

⚠️ **Ajuste appsettings.json se necessário:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=cardapio_db;User=root;Password=SUA_SENHA_MYSQL;"
  },
  "LMStudio": {
    "BaseUrl": "http://192.168.56.1:5000"
  }
}
```

---

### **PASSO 3: Deletar Arquivo Antigo (1 min)**

❌ **Remover (não é mais usado):**

```
SEU_PROJETO/Cardapio_Inteligente.Api/Servicos/LlamaService.cs
```

**Motivo:** Substituído por LMStudioService.cs que usa HTTP ao invés de LLamaSharp

---

### **PASSO 4: Restaurar Pacotes (3 min)**

```bash
cd SEU_PROJETO/Cardapio_Inteligente.Api

dotnet clean
dotnet restore
dotnet build
```

**Espere ver:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

### **PASSO 5: Testar API (2 min)**

```bash
dotnet run
```

**Logs esperados:**
```
✅ Banco de dados verificado/criado com sucesso.
✅ Serviço LM Studio inicializado.
🚀 API Cardápio Inteligente iniciada com sucesso!
🔗 LM Studio: http://192.168.56.1:5000
Now listening on: http://localhost:5068
```

---

### **PASSO 6: Testar no Swagger (1 min)**

1. Abrir: http://localhost:5068/swagger
2. Procurar endpoint: `POST /api/IA/chat`
3. Testar com: `{"mensagem": "Olá!"}`

**Resposta esperada:** IA responde em português

---

## ✅ CHECKLIST FINAL

- [ ] MySQL rodando (porta 3306)
- [ ] Banco cardapio_db com 16 pratos
- [ ] LM Studio rodando (porta 5000)
- [ ] API compilando sem erros
- [ ] API iniciando na porta 5068
- [ ] Swagger respondendo
- [ ] IA respondendo via Swagger

---

## 🎯 PRÓXIMO PASSO: TESTAR APP MAUI

1. Abrir projeto Cardapio_Inteligente no Visual Studio
2. Selecionar target: net8.0-android ou net8.0-windows
3. F5 (Run)
4. Fazer login com usuário do banco
5. Testar ChatPage

---

## 🐛 PROBLEMA COMUM

**Erro: "Não foi possível conectar ao LM Studio"**

**Solução:**
1. Verificar se LM Studio está rodando
2. Verificar IP em appsettings.json (pode ser `localhost:1234` ao invés de `192.168.56.1:5000`)
3. Testar no navegador: http://SEU_IP:PORTA/v1/models

---

## 📊 ESTRUTURA DO BANCO (Já Populado)

**Tabela: pratos**
- 16 pratos cadastrados
- Colunas: Id, Categoria, Nome, Ingredientes, Preco, TemLactose
- Exemplo: "Frango Alfredo" (Sim/lactose), "Bruschetta" (Não/sem lactose)

**Tabela: usuarios**
- 19 usuários de teste
- Colunas: Id, Nome, Email, SenhaHash, Telefone, Restricoes, Intolerancia, DataCriacao
- Exemplo: pedro.teste@gmail.com (senha: Senha12345)

⚠️ **Senhas no banco NÃO estão em BCrypt!** São senhas em texto plano. Para TCC está OK, mas em produção deveria usar BCrypt.

---

## 📝 RESUMO DAS MUDANÇAS

**O que foi corrigido:**
1. ✅ Substituído LLamaSharp por LMStudioService (HTTP)
2. ✅ Removido modelo .gguf do projeto (2GB)
3. ✅ Simplificado .csproj (sem pacotes pesados)
4. ✅ Configurado para usar LM Studio externo
5. ✅ Banco já populado com dados reais

**O que NÃO mudou:**
- ✅ App MAUI continua igual
- ✅ Controllers da API continuam iguais
- ✅ Models continuam iguais
- ✅ Banco de dados usa o dump existente

---

**Tempo total:** 15 minutos  
**Arquivos alterados:** 4  
**Arquivos removidos:** 1  
**Impacto:** Resolve o problema crítico da IA

🎯 **Após aplicar: teste imediatamente e documente screenshots!**
