using Cardapio_Inteligente.Api.Dados;
using Cardapio_Inteligente.Api.Modelos;
using Cardapio_Inteligente.Api.Servicos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Cardapio_Inteligente.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PratosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILlamaService _llamaService;

        public PratosController(AppDbContext context, ILlamaService llamaService)
        {
            _context = context;
            _llamaService = llamaService;
        }

        // ----------------------------------------------------------------------
        // ✅ ENDPOINT: Lista de pratos
        // ----------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prato>>> GetPratos(string? alergias)
        {
            var query = _context.Pratos.AsQueryable();

            if (!string.IsNullOrEmpty(alergias))
            {
                string alergiaLower = alergias.ToLower();
                if (alergiaLower.Contains("lactose"))
                    query = query.Where(p => p.TemLactose == "Não");
            }

            var pratos = await query.ToListAsync();
            return Ok(pratos);
        }

        // ----------------------------------------------------------------------
        // ✅ ENDPOINT: Assistente de IA - COM LIMPEZA INTELIGENTE AUTOMÁTICA
        // ----------------------------------------------------------------------
        public class PromptRequest
        {
            public string Prompt { get; set; } = string.Empty;
        }

        [HttpPost("assistente-chat")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GerarRespostaIA([FromBody] PromptRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
                return BadRequest(new { sucesso = false, mensagem = "O campo 'prompt' é obrigatório." });

            try
            {
                // 1. Busca todos os pratos
                var pratos = await _context.Pratos.AsNoTracking().ToListAsync();
                if (pratos.Count == 0)
                    return Ok(new { sucesso = true, mensagem = "O cardápio está vazio no momento." });

                // 2. DETECÇÃO DE TIPO DE PERGUNTA
                string promptLower = request.Prompt.ToLower();
                IEnumerable<Prato> pratosRelevantes = pratos;

                // 🔹 PERGUNTAS CONCEITUAIS SOBRE LACTOSE
                string[] perguntasConceituais = {
                    "o que é lactose", "o que significa lactose", "defina lactose",
                    "o que quer dizer lactose", "explica lactose", "conceito de lactose",
                    "significado de lactose", "definição de lactose"
                };

                bool perguntaConceitual = perguntasConceituais.Any(t => promptLower.Contains(t));

                // 🔹 PERGUNTAS SOBRE PRATOS SEM LACTOSE
                string[] termosSemLactose = {
                    "sem lactose", "livre de lactose", "não contém lactose",
                    "sem leite", "intolerância à lactose", "pratos sem lactose",
                    "alergia a leite", "sugira", "recomende", "quais pratos",
                    "opções", "cardápio sem lactose"
                };

                bool pedeSemLactose = termosSemLactose.Any(t => promptLower.Contains(t));

                // 3. CONSTRUÇÃO DO PROMPT BASEADO NO TIPO DE PERGUNTA
                string promptCompleto;
                bool ehPerguntaConceitual = perguntaConceitual && !pedeSemLactose;

                if (ehPerguntaConceitual)
                {
                    // 🔹 PROMPT PARA PERGUNTAS CONCEITUAIS
                    promptCompleto = $@"
Você é o assistente do restaurante Cardápio Inteligente, especializado em ajudar clientes com intolerância à lactose.

O cliente perguntou: ""{request.Prompt}""

Por favor, explique de forma clara e educada o que é lactose, focando em:
- Definição simples de lactose
- Onde é encontrada (alimentos com lactose)
- Problemas relacionados (intolerância)
- Como isso se relaciona com as escolhas alimentares

Seja simpático e direto. Não liste pratos do cardápio a menos que o cliente peça.

RESPOSTA:";
                }
                else if (pedeSemLactose)
                {
                    // 🔹 PROMPT PARA PRATOS SEM LACTOSE
                    pratosRelevantes = pratos.Where(p =>
                        p.TemLactose != null &&
                        p.TemLactose.Equals("Não", StringComparison.OrdinalIgnoreCase));

                    if (!pratosRelevantes.Any())
                        return Ok(new { sucesso = true, mensagem = "Lamentamos, mas não há pratos sem lactose disponíveis no cardápio no momento." });

                    var nomesPratos = string.Join(", ", pratosRelevantes.Select(p => p.ItemMenu));

                    promptCompleto = $@"
CLIENTE PERGUNTOU: {request.Prompt}

CARDÁPIO SEM LACTOSE DISPONÍVEL: {nomesPratos}

INSTRUÇÃO: Liste TODOS os pratos sem lactose acima em sua resposta. Seja simpático e direto.

RESPOSTA:";
                }
                else
                {
                    // 🔹 PROMPT PARA OUTRAS PERGUNTAS (fallback)
                    promptCompleto = $@"
CLIENTE PERGUNTOU: {request.Prompt}

INSTRUÇÃO: Você é o assistente do restaurante Cardápio Inteligente. Responda de forma simpática e útil, focando em ajudar com dúvidas sobre o cardápio e intolerâncias alimentares, especialmente lactose.

RESPOSTA:";
                }

                var resposta = await _llamaService.GerarRespostaAsync(promptCompleto);

                // 4. LIMPEZA INTELIGENTE - DETECTA AUTOMATICAMENTE O FIM DA RESPOSTA PRINCIPAL
                var respostaLimpa = LimparRespostaInteligentemente(resposta, request.Prompt);

                // 5. FALLBACK DIRETO - Apenas para perguntas sobre pratos
                if (!ehPerguntaConceitual && pedeSemLactose &&
                    (string.IsNullOrWhiteSpace(respostaLimpa) ||
                     respostaLimpa.Split(',').Length < 2 || // Menos de 2 pratos mencionados
                     !ContemMultiplosPratos(respostaLimpa, pratosRelevantes.Select(p => p.ItemMenu))))
                {
                    // Fallback: Monta resposta manual com TODOS os pratos
                    if (pratosRelevantes.Any())
                    {
                        var todosNomes = string.Join(", ", pratosRelevantes.Select(p => p.ItemMenu));
                        respostaLimpa = $"Temos estes pratos sem lactose: {todosNomes}. São todas opções deliciosas!";
                    }
                    else
                    {
                        respostaLimpa = "Desculpe, não consegui entender sua solicitação. Posso ajudar com informações sobre pratos sem lactose?";
                    }
                }

                // Capitaliza a primeira letra se necessário
                if (!string.IsNullOrEmpty(respostaLimpa) && char.IsLower(respostaLimpa[0]))
                {
                    respostaLimpa = char.ToUpper(respostaLimpa[0]) + respostaLimpa.Substring(1);
                }

                // Garante pontuação final
                if (!string.IsNullOrEmpty(respostaLimpa) &&
                    !respostaLimpa.EndsWith(".") &&
                    !respostaLimpa.EndsWith("!") &&
                    !respostaLimpa.EndsWith("?"))
                {
                    respostaLimpa += ".";
                }

                return Ok(new { sucesso = true, mensagem = respostaLimpa });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO IA CONTEXTUAL] {ex}");
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro interno ao gerar resposta da IA: {ex.Message}"
                });
            }
        }

        // 🔹 MÉTODO INTELIGENTE PARA LIMPEZA DE RESPOSTAS - DETECTA FIM NATURAL
        private string LimparRespostaInteligentemente(string resposta, string perguntaOriginal)
        {
            if (string.IsNullOrWhiteSpace(resposta))
                return string.Empty;

            var respostaLimpa = resposta.Trim();
            Console.WriteLine($"🔍 Analisando resposta de {respostaLimpa.Length} caracteres...");

            // 🔹 ETAPA 1: Análise por linhas para detectar estrutura
            var linhas = respostaLimpa.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            if (linhas.Count == 0)
                return string.Empty;

            // 🔹 ETAPA 2: Detecta padrões de reinício explícitos
            var padroesReinicio = new[]
            {
                "====", "---", "### Response:", "- Response:", "Response:",
                "Assistant:", "Resposta:", "Olá amigo!", "Hello", "Hi,",
                "Explanation:", "teacher:", "Solution=", "Pergunta do cliente:", "Question:",
                "```", "\"\"\"", "'''", "___"
            };

            foreach (var padrao in padroesReinicio)
            {
                for (int i = 0; i < linhas.Count; i++)
                {
                    if (i > 0 && linhas[i].StartsWith(padrao, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"🎯 Detectado reinício no padrão: {padrao}");
                        respostaLimpa = string.Join(" ", linhas.Take(i)).Trim();
                        linhas = respostaLimpa.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(l => l.Trim())
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToList();
                        break;
                    }
                }
            }

            // 🔹 ETAPA 3: Análise de contexto - detecta onde a resposta termina naturalmente
            if (linhas.Count > 1)
            {
                // Padrões que indicam CONCLUSÃO natural da resposta
                var padroesConclusao = new[]
                {
                    "espero que", "bom apetite", "boa refeição", "aproveite",
                    "qualquer dúvida", "estamos à disposição", "volte sempre",
                    "obrigado", "obrigada", "atenciosamente", "cumprimentos",
                    "bon appétit", "tenha um ótimo", "desejo uma excelente"
                };

                // Padrões que indicam NOVO INÍCIO (duplicação)
                var padroesNovoInicio = new[]
                {
                    "1.", "2.", "3.", "4.", "5.", "6.", "7.", "8.", "9.", "10.",
                    "- ", "• ", "* ", "→ ", "⇒ ", "📌 ", "🔥 ", "⭐ ", "💡 "
                };

                // Procura pelo ponto natural de conclusão
                for (int i = 1; i < linhas.Count; i++)
                {
                    var linhaAtual = linhas[i];
                    var linhaAnterior = linhas[i - 1];

                    // Verifica se a linha anterior parece ser uma conclusão
                    bool linhaAnteriorEhConclusao = padroesConclusao.Any(p =>
                        linhaAnterior.Contains(p, StringComparison.OrdinalIgnoreCase)) ||
                        linhaAnterior.EndsWith("!") ||
                        linhaAnterior.EndsWith(".") && linhaAnterior.Length > 20;

                    // Verifica se a linha atual parece ser um novo início
                    bool linhaAtualEhNovoInicio = padroesNovoInicio.Any(p =>
                        linhaAtual.StartsWith(p)) ||
                        (linhaAtual.StartsWith("Olá", StringComparison.OrdinalIgnoreCase) && i > 0);

                    // Se encontrou uma conclusão seguida de um novo início, corta aqui
                    if (linhaAnteriorEhConclusao && linhaAtualEhNovoInicio)
                    {
                        Console.WriteLine($"🎯 Detectada conclusão natural seguida de novo início");
                        respostaLimpa = string.Join(" ", linhas.Take(i)).Trim();
                        break;
                    }
                }
            }

            // 🔹 ETAPA 4: Remove prefixos comuns no início
            respostaLimpa = Regex.Replace(respostaLimpa,
                @"^(Assistant:\s*|Resposta:\s*|Response:\s*|RESPOSTA:\s*)",
                "", RegexOptions.IgnoreCase).Trim();

            // 🔹 ETAPA 5: Limpeza final de linhas problemáticas
            var linhasFinais = respostaLimpa.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l) &&
                           !l.StartsWith("Explanation:", StringComparison.OrdinalIgnoreCase) &&
                           !l.StartsWith("teacher:", StringComparison.OrdinalIgnoreCase) &&
                           !l.StartsWith("Pergunta do cliente:", StringComparison.OrdinalIgnoreCase) &&
                           !l.StartsWith("Question:", StringComparison.OrdinalIgnoreCase) &&
                           !l.StartsWith("Solution=", StringComparison.OrdinalIgnoreCase))
                .ToList();

            respostaLimpa = string.Join(" ", linhasFinais).Trim();

            // Remove múltiplos espaços
            respostaLimpa = Regex.Replace(respostaLimpa, @"\s+", " ");

            Console.WriteLine($"🧹 Resposta limpa: {respostaLimpa.Length} caracteres");
            return respostaLimpa;
        }

        // Método auxiliar para verificar se a resposta contém múltiplos pratos
        private bool ContemMultiplosPratos(string resposta, IEnumerable<string> nomesPratos)
        {
            int count = 0;
            foreach (var nome in nomesPratos)
            {
                if (resposta.IndexOf(nome, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    count++;
                    if (count >= 2) return true; // Pelo menos 2 pratos mencionados
                }
            }
            return false;
        }
    }
}