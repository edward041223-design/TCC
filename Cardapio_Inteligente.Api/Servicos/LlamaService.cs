using Cardapio_Inteligente.Api.Configuracao;
using LLama;
using LLama.Abstractions;
using LLama.Common;
using LLama.Sampling;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Cardapio_Inteligente.Api.Servicos
{
    public class LlamaService : ILlamaService
    {
        private readonly StatelessExecutor _executor;
        private readonly LlamaSettings _settings;

        public LlamaService(IOptions<LlamaSettings> settings)
        {
            _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));

            // 🔹 Caminho absoluto do modelo (garante que ele seja encontrado)
            var modelPath = Path.Combine(AppContext.BaseDirectory, _settings.ModelPath);

            if (!File.Exists(modelPath))
            {
                Console.WriteLine($"❌ Modelo não encontrado: {modelPath}");
                // Garante que o aplicativo não inicie se o modelo não estiver no local
                throw new FileNotFoundException("Modelo .gguf não encontrado no caminho ModelosIA/", modelPath);
            }

            // ✅ CORREÇÃO CS0266: Adiciona (uint) para converter o int do LlamaSettings.ContextSize para o tipo uint esperado.
            var modelParams = new ModelParams(modelPath)
            {
                ContextSize = (uint)_settings.ContextSize, // ✅ CORRIGIDO: Conversão explícita para uint
                BatchSize = 512,
                Threads = Math.Max(2, _settings.NumThreads),
                GpuLayerCount = _settings.GpuLayerCount
            };

            try
            {
                Console.WriteLine($"🔄 Carregando modelo Phi-3-mini de: {modelPath}");
                var weights = LLamaWeights.LoadFromFile(modelParams);
                _executor = new StatelessExecutor(weights, modelParams);
                Console.WriteLine($"✅ Modelo carregado com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Falha ao carregar modelo: {ex.Message}");
                throw new InvalidOperationException($"Erro ao carregar modelo: {ex.Message}", ex);
            }
        }

        public async Task<string> GerarRespostaAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return "Prompt vazio. Informe uma pergunta válida.";

            try
            {
                // 🔹 Configuração de sampling natural e estável
                var samplingPipeline = new DefaultSamplingPipeline
                {
                    Temperature = Math.Clamp((float)_settings.Temperature, 0.7f, 1.1f),
                    TopP = Math.Clamp((float)_settings.TopP, 0.85f, 0.95f),
                    RepeatPenalty = 1.1f
                };

                // 🔹 Parâmetros de inferência
                var infParams = new InferenceParams
                {
                    MaxTokens = Math.Min(_settings.MaxTokens, 512),
                    SamplingPipeline = samplingPipeline,
                    // ✅ AntiPrompts aprimorados do passo anterior
                    AntiPrompts = new[] { "Explanation>", "Pergunta:", "Resposta:", "Cliente:", "<|assistant|>", "<|im_end|>" }
                };

                Console.WriteLine($"🧠 Gerando resposta ({prompt.Length} caracteres)...");

                var resultBuilder = new StringBuilder();

                // 🔹 Execução segura e assíncrona
                await foreach (var token in _executor.InferAsync(prompt, infParams))
                {
                    resultBuilder.Append(token);
                }

                var resposta = resultBuilder.ToString().Trim();

                // ✅ Limpeza robusta de ruídos de tokens e formatação Markdown do passo anterior
                resposta = resposta
                    .Replace("Explanation>", "")
                    .Replace("Pergunta:", "")
                    .Replace("Resposta:", "")
                    .Replace("Cliente:", "")
                    .Replace("<|assistant|>", "")
                    .Replace("<|im_end|>", "")
                    .Replace("**", "") // Remove negrito Markdown
                    .Replace("...", "") // Remove elipses
                    .Replace("\n\n\n", "\n\n")
                    .Trim();

                Console.WriteLine($"🤖 Resposta: {resposta}");
                return resposta;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro em LlamaService: {ex.Message}");
                return $"Erro ao gerar resposta da IA: {ex.Message}";
            }
        }
    }
}