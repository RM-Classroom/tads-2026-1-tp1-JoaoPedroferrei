using Microsoft.EntityFrameworkCore;
using Locadora_veiculos.Models;
using static Locadora_veiculos.Models.Aluguel;

namespace Locadora_veiculos.Data
{
    public class LocadoraDbContext : DbContext
    {
        public LocadoraDbContext(DbContextOptions<LocadoraDbContext> options) : base(options)  
        {
        }

        // Tabelas 
        public DbSet<Fabricante> Fabricantes { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Aluguel> Alugueis { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Fabricante 
            modelBuilder.Entity<Fabricante>(entity =>
            {
                entity.ToTable("Fabricantes");
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Nome).IsRequired().HasMaxLength(100);
                entity.Property(f => f.PaisOrigem).HasMaxLength(100);
            });

            // Categoria
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("Categorias");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nome).IsRequired().HasMaxLength(80);
                entity.Property(c => c.Descricao).HasMaxLength(255);
            });

            // Veículo
            modelBuilder.Entity<Veiculo>(entity =>
            {
                entity.ToTable("Veiculos");
                entity.HasKey(v => v.Id);
                entity.Property(v => v.Modelo).IsRequired().HasMaxLength(100);
                entity.Property(v => v.AnoFabricacao).IsRequired();
                entity.Property(v => v.Quilometragem).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(v => v.ValorDiaria).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(v => v.Disponivel).HasDefaultValue(true).ValueGeneratedNever();

                // Fabricante -> Veículos (N:1)
                entity.HasOne(v => v.Fabricante)
                      .WithMany(f => f.Veiculos)
                      .HasForeignKey(v => v.FabricanteId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Categoria -> Veículos (N:1)
                entity.HasOne(v => v.Categoria)
                      .WithMany(c => c.Veiculos)
                      .HasForeignKey(v => v.CategoriaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Clientes");
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Nome).IsRequired().HasMaxLength(150);
                entity.Property(c => c.CPF).IsRequired().HasMaxLength(14);
                entity.Property(c => c.Email).IsRequired().HasMaxLength(150);
                entity.Property(c => c.Telefone).HasMaxLength(20);

                // Índices únicos
                entity.HasIndex(c => c.CPF).IsUnique();
                entity.HasIndex(c => c.Email).IsUnique(); 
            });

            // Aluguel 
            modelBuilder.Entity<Aluguel>(entity =>
            {
                entity.ToTable("Alugueis");
                entity.HasKey(a => a.Id);
                entity.Property(a => a.DataInicio).IsRequired();
                entity.Property(a => a.DataFim).IsRequired();
                entity.Property(a => a.KmInicial).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(a => a.KmFinal).HasColumnType("decimal(10,2)");
                entity.Property(a => a.ValorDiaria).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(a => a.ValorTotal).HasColumnType("decimal(10,2)");
                entity.Property(a => a.Status).IsRequired().HasMaxLength(20).HasDefaultValue(StatusAluguel.Aberto);

                // Cliente -> Aluguéis (N:1)
                entity.HasOne(a => a.Cliente)
                      .WithMany(c => c.Alugueis)
                      .HasForeignKey(a => a.ClienteId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Veículo -> Aluguéis (N:1)
                entity.HasOne(a => a.Veiculo)
                      .WithMany(v => v.Alugueis)
                      .HasForeignKey(a => a.VeiculoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
