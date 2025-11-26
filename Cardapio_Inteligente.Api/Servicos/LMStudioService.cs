using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cardapio_Inteligente.Api.Servicos
{
    /// <summary>
    /// Serviço para integração com LM Studio rodando Phi-3-mini
    /// Usa endpoints OpenAI-compatíveis do LM Studio
    /// </summary>
    public class LMStudioService : ILlamaService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private const string SYSTEM_PROMPT = @"Você é um assistente virtual especializado em nutrição e cardápios para pessoas com intolerância à lactose.
Responda de forma clara, objetiva e amigável.
Suas respostas devem ser em português brasileiro.
Foque em dar informações úteis sobre pratos, ingredientes e alternativas sem lactose.
Seja breve e direto nas respostas.";

        public LMStudioService(IConfiguration configuration)
        {
            // Obtém URL do LM Studio do appsettings.json
            _baseUrl = configuration["LMStudio:BaseUrl"] ?? "http://192.168.56.1:5000";
            
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60)
            };

            Console.WriteLine($"🔗 LMStudioService configurado para: {_baseUrl}");
        }

        public async Task<string> GerarRespostaAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return "Prompt vazio. Informe uma pergunta válida.";

            try
            {
                Console.WriteLine($"🧠 Enviando pergunta para LM Studio: {prompt}");

                // Monta o payload no formato OpenAI Chat Completions
                var requestBody = new
                {
                    model = "phi-3-mini-4k-instruct", // Nome do modelo no LM Studio
                    messages = new[]
                    {
                        new { role = "system", content = SYSTEM_PROMPT },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 300,
                    top_p = 0.9
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Faz a requisição para o endpoint /v1/chat/completions
                var response = await _httpClient.PostAsync(
                    $"{_baseUrl}/v1/chat/completions",
                    httpContent
                );

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"❌ Erro na resposta do LM Studio: {response.StatusCode} - {errorContent}");
                    return $"Erro ao conectar com LM Studio: {response.StatusCode}";
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📥 Resposta bruta do LM Studio: {responseContent}");

                // Parse da resposta JSON
                using var doc = JsonDocument.Parse(responseContent);
                var root = doc.RootElement;

                // Extrai a resposta do formato OpenAI
                if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message))
                    {
                        if (message.TryGetProperty("content", out var content))
                        {
                            var resposta = content.GetString()?.Trim() ?? "Sem resposta.";
                            Console.WriteLine($"✅ Resposta da IA: {resposta}");
                            return resposta;
                        }
                    }
                }

                return "Erro ao processar resposta da IA.";
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Erro de conexão com LM Studio: {ex.Message}");
                return $"Não foi possível conectar ao LM Studio em {_baseUrl}. Verifique se o servidor está rodando.";
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("⏱️ Timeout na conexão com LM Studio");
                return "Timeout: LM Studio demorou muito para responder.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro em LMStudioService: {ex.Message}");
                return $"Erro ao gerar resposta da IA: {ex.Message}";
            }
        }
    }
}
