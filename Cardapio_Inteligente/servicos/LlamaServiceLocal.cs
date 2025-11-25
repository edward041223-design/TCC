using System;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace Cardapio_Inteligente.Servicos
{
    /// <summary>
    /// Implementação LOCAL (stub) da interface ILlamaService para plataformas que não são Android.
    /// Em Android a implementação real deve ser registrada (LlamaServiceAndroid) para usar LLamaSharp.
    /// </summary>
    public class LlamaServiceLocal : ILlamaService
    {
        private bool _isInitialized = false;
        private readonly SemaphoreSlim _initLock = new(1, 1);

        private const string SYSTEM_PROMPT = @"Você é um assistente virtual especializado em nutrição e cardápios para pessoas com intolerância à lactose.
Responda de forma clara, objetiva e amigável.
Suas respostas devem ser em português brasileiro.
Foque em dar informações úteis sobre pratos, ingredientes e alternativas sem lactose.";

        public async Task<bool> IsReadyAsync()
        {
            if (_isInitialized)
                return true;

            await _initLock.WaitAsync();
            try
            {
                _isInitialized = true;
            }
            finally
            {
                _initLock.Release();
            }

            return _isInitialized;
        }

        public Task<string> GerarRespostaAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                return Task.FromResult("Prompt vazio. Informe uma pergunta válida.");

            var sb = new StringBuilder();
            sb.AppendLine("[Resposta gerada pelo stub de IA local]");
            sb.AppendLine();
            sb.AppendLine("Prompt recebido:");
            sb.AppendLine(prompt);
            sb.AppendLine();
            sb.AppendLine("Observação: Este é um stub sem capacidades de inferência. Em Android, registre a implementação que usa LLamaSharp.");

            return Task.FromResult(sb.ToString());
        }
    }
}
