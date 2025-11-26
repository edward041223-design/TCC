using Cardapio_Inteligente.Api.Dados;
using Cardapio_Inteligente.Api.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration; // Necessário para acessar IConfiguration

namespace Cardapio_Inteligente.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public UsuariosController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // -----------------------------
        // Cadastrar novo usuário
        // -----------------------------
        [HttpPost("Cadastrar")]
        [ProducesResponseType(201)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> Cadastrar([FromBody] Usuario usuario)
        {
            if (usuario == null)
                return BadRequest(new { Mensagem = "Dados inválidos." });

            if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
                return Conflict(new { Mensagem = "Este e-mail já está em uso." });

            // ⚠️ NOTA: A senha é armazenada em texto simples conforme a estrutura atual do projeto.
            usuario.DataCadastro = DateTime.UtcNow;
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Retorna o objeto do usuário no corpo da resposta (correção aplicada aqui)
            return CreatedAtAction(nameof(Login), new { id = usuario.Id }, new
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Telefone = usuario.Telefone,
                // ✅ CORREÇÃO 1/2: Usando o novo nome da propriedade
                IngredientesNaoGosta = usuario.IngredientesNaoGosta,
                Alergias = usuario.Alergias,
                DataCadastro = usuario.DataCadastro
            });
        }

        // -----------------------------
        // Login de usuário
        // -----------------------------
        [HttpPost("Login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var usuario = await _context.Usuarios
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Senha == loginDto.Senha);

            if (usuario == null)
                return Unauthorized(new { sucesso = false, mensagem = "E-mail ou Senha inválidos." });

            var token = GerarToken(usuario);

            return Ok(new LoginResponse
            {
                Token = token,
                Usuario = usuario // Inclui o objeto Usuário (sem a senha, devido ao [JsonIgnore] no modelo)
            });
        }

        // -----------------------------
        // Obter detalhes do usuário (exemplo de endpoint protegido)
        // -----------------------------
        [HttpGet("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize] // Requer JWT
        public async Task<IActionResult> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(new { mensagem = "Usuário não encontrado." });
            }

            return Ok(new
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Telefone = usuario.Telefone,
                // ✅ CORREÇÃO 2/2: Usando o novo nome da propriedade
                IngredientesNaoGosta = usuario.IngredientesNaoGosta,
                Alergias = usuario.Alergias,
                DataCadastro = usuario.DataCadastro
            });
        }


        // -----------------------------
        // Geração do Token JWT
        // -----------------------------
        private string GerarToken(Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var keyString = _configuration["JwtSettings:Secret"];
            if (string.IsNullOrEmpty(keyString))
                throw new InvalidOperationException("JwtSettings:Secret não configurado no appsettings.json.");

            var key = Encoding.UTF8.GetBytes(keyString);

            // Adiciona as Claims (informações) no token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Email, usuario.Email),
                // ✅ Adiciona a Alergia como uma Claim (útil para futuros filtros automáticos no MAUI)
                new Claim("Alergias", usuario.Alergias)
            };

            // ✅ lê TokenLifetimeHours do appsettings.json (padrão = 2h)
            int tokenHours = 2;
            var tokenLifetimeSetting = _configuration["JwtSettings:TokenLifetimeHours"];
            if (!string.IsNullOrWhiteSpace(tokenLifetimeSetting) && int.TryParse(tokenLifetimeSetting, out var th))
            {
                tokenHours = th;
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(tokenHours),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}