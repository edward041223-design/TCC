using Cardapio_Inteligente.Api.Dados;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cardapio_Inteligente.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IngredientesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public IngredientesController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retorna uma lista única de todos os ingredientes disponíveis nos pratos cadastrados.
        /// Este endpoint é usado para popular os checkboxes de seleção de ingredientes.
        /// </summary>
        /// <returns>Lista única de ingredientes ordenada alfabeticamente</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<string>), 200)]
        public async Task<ActionResult<List<string>>> GetIngredientes()
        {
            try
            {
                // Busca todos os pratos com ingredientes
                var pratos = await _context.Pratos
                    .Where(p => !string.IsNullOrEmpty(p.Ingredientes))
                    .Select(p => p.Ingredientes)
                    .ToListAsync();

                var ingredientesUnicos = new HashSet<string>();

                foreach (var ingredientesStr in pratos)
                {
                    try
                    {
                        // Tenta parsear como JSON array
                        // Formato esperado: ['Tomates', 'Manjericão', 'Alho']
                        var ingredientes = JsonSerializer.Deserialize<List<string>>(ingredientesStr);
                        
                        if (ingredientes != null)
                        {
                            foreach (var ing in ingredientes)
                            {
                                var ingredienteLimpo = ing.Trim();
                                if (!string.IsNullOrWhiteSpace(ingredienteLimpo) && 
                                    !ingredienteLimpo.Equals("confidencial", StringComparison.OrdinalIgnoreCase))
                                {
                                    ingredientesUnicos.Add(ingredienteLimpo);
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Se não for JSON válido, tenta split por vírgula
                        var ingredientes = ingredientesStr
                            .Replace("[", "")
                            .Replace("]", "")
                            .Replace("'", "")
                            .Replace("\"", "")
                            .Split(',')
                            .Select(i => i.Trim())
                            .Where(i => !string.IsNullOrWhiteSpace(i) && 
                                       !i.Equals("confidencial", StringComparison.OrdinalIgnoreCase));

                        foreach (var ing in ingredientes)
                        {
                            ingredientesUnicos.Add(ing);
                        }
                    }
                }

                // Retorna lista ordenada alfabeticamente
                var listaOrdenada = ingredientesUnicos
                    .OrderBy(i => i)
                    .ToList();

                return Ok(listaOrdenada);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    sucesso = false, 
                    mensagem = $"Erro ao buscar ingredientes: {ex.Message}" 
                });
            }
        }
    }
}
