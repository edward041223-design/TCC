using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Cardapio_Inteligente.Api.Modelos
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email é obrigatório")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Senha { get; set; } = string.Empty;

        public string Telefone { get; set; } = string.Empty;

        
        [Column("IngredientesNaoGosta")]
        public string IngredientesNaoGosta { get; set; } = string.Empty;

        public string Alergias { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; }

        [NotMapped]
        [JsonIgnore]
        public string? Token { get; set; }
    }
}