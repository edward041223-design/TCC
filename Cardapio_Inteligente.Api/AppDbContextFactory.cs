using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using Cardapio_Inteligente.Api.Dados;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.Extensions.Configuration;
using System.IO;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        optionsBuilder.UseMySql(connectionString,
            ServerVersion.AutoDetect(connectionString),
            mySqlOptions => mySqlOptions.SchemaBehavior(MySqlSchemaBehavior.Ignore));

        return new AppDbContext(optionsBuilder.Options);
    }
}
