using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cardapio_Inteligente.Api.Modelos
{
    [Table("pratos")]
    public class Prato
    {
        [Key]
        public int Id { get; set; }

        [Column("Categoria")]
        [Required]
        public string Categoria { get; set; } = string.Empty;

        [Column("Item_Menu")]
        [Required]
        public string ItemMenu { get; set; } = string.Empty;

        [Column("Ingredientes")]
        public string Ingredientes { get; set; } = string.Empty;

        [Column("Preco", TypeName = "decimal(18,2)")]
        [Required]
        public decimal Preco { get; set; }

        [Column("Tem_Lactose")]
        public string TemLactose { get; set; } = "Desconhecido";
    }
}
