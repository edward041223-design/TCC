namespace Cardapio_Inteligente.Api.Modelos.Enums
{
    /// <summary>
    /// Enum que representa se um prato contém lactose.
    /// Mantém compatibilidade com o tipo ENUM do MySQL ('Sim', 'Não', 'Desconhecido').
    /// </summary>
    public enum TemLactoseEnum
    {
        Desconhecido = 0,
        Sim = 1,
        Nao = 2
    }
}
