using Cardapio_Inteligente.Api.Configuracao;
using Cardapio_Inteligente.Api.Dados;
using Cardapio_Inteligente.Api.Servicos;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Text;
using Microsoft.OpenApi.Models; // Necessário para a segurança JWT no Swagger

// --------------------------------------------------------------------------
// 🔧 Inicialização do builder
// --------------------------------------------------------------------------
var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------------------------------
// 1️⃣ Serviços base
// --------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// --------------------------------------------------------------------------
// 2️⃣ Swagger com suporte a JWT
// --------------------------------------------------------------------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cardápio Inteligente API",
        Version = "v1",
        Description = "API para autenticação, IA e gerenciamento de usuários."
    });

    // Adiciona o suporte a Bearer Token (JWT) no Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira seu Token JWT no formato: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// --------------------------------------------------------------------------
// 3️⃣ Configuração do Banco de Dados
// --------------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString,
        ServerVersion.AutoDetect(connectionString),
        mySqlOptions =>
        {
            mySqlOptions.SchemaBehavior(MySqlSchemaBehavior.Ignore);
            mySqlOptions.EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        });
});

// --------------------------------------------------------------------------
// 4️⃣ Configuração do LLamaSharp e Injeção de Dependência
// --------------------------------------------------------------------------
// ✅ Víncula as configurações da seção "LlamaSettings" do appsettings.json
builder.Services.Configure<LlamaSettings>(builder.Configuration.GetSection("LlamaSettings"));

// ✅ Registra o serviço de IA como Singleton (o modelo é grande e só deve ser carregado uma vez)
builder.Services.AddSingleton<ILlamaService, LlamaService>();

// --------------------------------------------------------------------------
// 5️⃣ Configuração do JWT
// --------------------------------------------------------------------------
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JwtSettings:Secret não configurado.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

// --------------------------------------------------------------------------
// 6️⃣ Configuração do CORS (para o app MAUI)
// --------------------------------------------------------------------------
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMauiClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// --------------------------------------------------------------------------
// 7️⃣ Inicialização e Verificação de Serviços
// --------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    // 🗄️ Verifica o banco de dados
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    Console.WriteLine("🗄️ Banco de dados verificado/criado com sucesso.");

    // 🤖 Inicialização da IA no startup (carregamento do modelo)
    try
    {
        var llama = scope.ServiceProvider.GetRequiredService<ILlamaService>();
        Console.WriteLine("🤖 Serviço de IA inicializado com sucesso.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Erro ao inicializar IA: {ex.Message}");
    }
}

// --------------------------------------------------------------------------
// 8️⃣ Configuração do pipeline HTTP
// --------------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ Ordem correta: CORS → Auth → Controllers
app.UseCors("AllowMauiClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// --------------------------------------------------------------------------
// ✅ Log final de inicialização
// --------------------------------------------------------------------------
Console.WriteLine("🚀 API Cardápio Inteligente iniciada com sucesso!");
Console.WriteLine($"🌐 CORS liberado para: {string.Join(", ", allowedOrigins)}");

app.Run();