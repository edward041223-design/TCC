#if ANDROID && LLAMA_AVAILABLE
using System;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using LLama;
using LLama.Common;
using LLama.Sampling;
using Microsoft.Maui.Storage;

namespace Cardapio_Inteligente.Servicos
{
 /// <summary>
 /// Implementação Android que usa LLamaSharp para inferência local.
 /// Compilada apenas quando ANDROID e LLAMA_AVAILABLE estiverem definidas.
 /// </summary>
 public class LlamaServiceAndroid : ILlamaService
 {
 private LLamaWeights? _model;
 private LLamaContext? _context;
 private InteractiveExecutor? _executor;
 private bool _isInitialized = false;
 private readonly SemaphoreSlim _initLock = new(1,1);

 private const string SYSTEM_PROMPT = @"Você é um assistente virtual especializado em nutrição e cardápios para pessoas com intolerância à lactose.
Responda de forma clara, objetiva e amigável.
Suas respostas devem ser em português brasileiro.
Foque em dar informações úteis sobre pratos, ingredientes e alternativas sem lactose.";

 public async Task<bool> IsReadyAsync()
 {
 if (_isInitialized)
 return true;

 await InitializeAsync();
 return _isInitialized;
 }

 private async Task InitializeAsync()
 {
 if (_isInitialized)
 return;

 await _initLock.WaitAsync();
 try
 {
 if (_isInitialized)
 return;

 Console.WriteLine("?? Inicializando LLamaSharp no Android...");

 // Garante que o modelo exista (tenta copiar de assets se necessário)
 var modelFile = "Phi-3-mini-4k-instruct-q4.gguf";
 var copied = await LlamaModelInstaller.EnsureModelInstalledAsync(modelFile);
 if (!copied)
 {
 throw new FileNotFoundException("Modelo .gguf não disponível no dispositivo. Instalação falhou.");
 }

 var modelPath = Path.Combine(FileSystem.Current.AppDataDirectory, "ModelosIA", modelFile);

 if (string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath))
 {
 throw new FileNotFoundException($"Modelo de IA não encontrado em: {modelPath}");
 }

 var parameters = new ModelParams(modelPath)
 {
 ContextSize =2048,
 GpuLayerCount =0,
 Threads = Environment.ProcessorCount >4 ?4 : Environment.ProcessorCount
 };

 _model = LLamaWeights.LoadFromFile(parameters);
 _context = _model.CreateContext(parameters);
 _executor = new InteractiveExecutor(_context);

 _isInitialized = true;
 Console.WriteLine("? LLamaSharp inicializado com sucesso no Android!");
 }
 catch (Exception ex)
 {
 Console.WriteLine($"? Erro ao inicializar LLamaSharp: {ex.Message}");
 throw;
 }
 finally
 {
 _initLock.Release();
 }
 }

 private string GetModelPath()
 {
 var assetsPath = Path.Combine(FileSystem.Current.AppDataDirectory, "ModelosIA", "Phi-3-mini-4k-instruct-q4.gguf");
 if (!File.Exists(assetsPath))
 {
 Console.WriteLine($"Modelo não encontrado em {assetsPath}");
 }
 return assetsPath;
 }

 public async Task<string> GerarRespostaAsync(string prompt)
 {
 if (string.IsNullOrWhiteSpace(prompt))
 return "Prompt vazio. Informe uma pergunta válida.";

 if (!_isInitialized)
 {
 await InitializeAsync();
 }

 if (_executor == null)
 throw new InvalidOperationException("LLamaSharp não foi inicializado corretamente.");

 try
 {
 var fullPrompt = $"{SYSTEM_PROMPT}\n\nUsuário: {prompt}\n\nAssistente:";

 var infParams = new InferenceParams
 {
 MaxTokens =256,
 AntiPrompts = new[] { "Usuário:", "\n\n" }
 };

 var sb = new StringBuilder();
 await foreach (var token in _executor.InferAsync(fullPrompt, infParams))
 {
 sb.Append(token);
 }

 var result = sb.ToString().Trim();
 return result;
 }
 catch (Exception ex)
 {
 Console.WriteLine($"? Erro ao gerar resposta LLamaSharp: {ex.Message}");
 throw;
 }
 }
 }
}
#endif