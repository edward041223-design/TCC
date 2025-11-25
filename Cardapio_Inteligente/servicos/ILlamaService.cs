using System.Threading.Tasks;

namespace Cardapio_Inteligente.Servicos
{
    /// <summary>
    /// Interface para serviço de IA que pode ser implementada localmente (Android) ou via API (Windows)
    /// </summary>
    public interface ILlamaService
    {
        /// <summary>
        /// Gera uma resposta da IA baseada no prompt fornecido
        /// </summary>
        Task<string> GerarRespostaAsync(string prompt);

        /// <summary>
        /// Verifica se o serviço está pronto para uso
        /// </summary>
        Task<bool> IsReadyAsync();
    }
}