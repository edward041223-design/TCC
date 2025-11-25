using System.ComponentModel.DataAnnotations;

namespace Cardapio_Inteligente.Modelos
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Senha { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        // ✅ CORREÇÃO: Renomeado de "Preferencias" para "IngredientesNaoGosta" para coincidir com a API
        public string IngredientesNaoGosta { get; set; } = string.Empty;
        
        // Mantém "Preferencias" como propriedade auxiliar para compatibilidade com código legado
        [System.Text.Json.Serialization.JsonIgnore]
        public string Preferencias
        {
            get => IngredientesNaoGosta;
            set => IngredientesNaoGosta = value;
        }

        public string Alergias { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }

        public string? Token { get; set; }
    }
}
