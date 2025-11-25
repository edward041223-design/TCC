using Microsoft.EntityFrameworkCore;
using Cardapio_Inteligente.Api.Modelos;

namespace Cardapio_Inteligente.Api.Dados
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; } = default!;
        public DbSet<Prato> Pratos { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuarios");
                entity.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<Prato>(entity =>
            {
                entity.ToTable("pratos");
                entity.Property(p => p.Preco)
                      .HasColumnName("Preco")
                      .HasColumnType("decimal(18,2)");
                entity.Property(p => p.ItemMenu).HasColumnName("Item_Menu");
                entity.Property(p => p.TemLactose).HasColumnName("Tem_Lactose");
            });
        }
    }
}
