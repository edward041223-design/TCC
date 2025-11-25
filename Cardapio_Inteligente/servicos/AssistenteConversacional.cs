using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardapio_Inteligente.Servicos;

public class AssistenteConversacional
{
    private readonly ApiService _apiService;

    public AssistenteConversacional(ApiService apiService)
    {
        _apiService = apiService;
    }

    readonly (string chave, string texto)[] perguntas = new[]
    {
        ("nome", "Qual seu nome completo?"),
        ("email", "Qual seu e-mail?"),
        ("senha", "Crie uma senha para seu acesso:"),
        ("telefone", "Qual seu telefone? (opcional)")
    };

    public (string chave, string texto) ObterPrimeiraPergunta() => perguntas[0];

    public (string chave, string texto)? ObterProximaPergunta(Dictionary<string, string> respostas)
    {
        foreach (var p in perguntas)
            if (!respostas.ContainsKey(p.chave))
                return p;
        return null;
    }

    public string? ObterChavePerguntaAtual(string texto)
    {
        var p = perguntas.FirstOrDefault(x => x.texto == texto);
        return string.IsNullOrEmpty(p.chave) ? null : p.chave;
    }

    public async Task<string> ProcessarMensagemMenuAsync(string mensagem)
    {
        if (string.IsNullOrWhiteSpace(mensagem))
            return "Por favor, digite sua pergunta.";

        try
        {
            string resposta = await _apiService.GerarRespostaIAAsync(mensagem);
            return resposta;
        }
        catch (System.Exception ex)
        {
            return $"Desculpe, n�o consegui processar sua pergunta: Falha na comunica��o com o assistente inteligente ({ex.Message})";
        }
    }
}
