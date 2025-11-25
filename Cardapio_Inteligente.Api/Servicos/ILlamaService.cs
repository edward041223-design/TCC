using System.Threading.Tasks;

namespace Cardapio_Inteligente.Api.Servicos
{
    public interface ILlamaService
    {
        Task<string> GerarRespostaAsync(string prompt);
    }
}
