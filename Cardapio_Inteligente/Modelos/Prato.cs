// ✅ Namespace corrigido para MAUI; removidos atributos EF Core
namespace Cardapio_Inteligente.Modelos
{
    public class Prato
    {
        public int Id { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string ItemMenu { get; set; } = string.Empty;
        public string Ingredientes { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string TemLactose { get; set; } = "Desconhecido";
    }
}