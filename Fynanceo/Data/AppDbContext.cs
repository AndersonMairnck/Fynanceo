using Fynanceo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Fynanceo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<EnderecoCliente> EnderecosClientes { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<IngredienteProduto> IngredientesProdutos { get; set; }
        public DbSet<Mesa> Mesas { get; set; }
        public DbSet<Funcionario> Funcionarios { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<ItemPedido> ItensPedido { get; set; }
     public DbSet<HistoricoPedido> HistoricoPedido { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Converter DateTime para UTC ao salvar e ao ler
            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(), // salvar em UTC
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc) // ler como UTC
            );
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType == typeof(DateTime));

                foreach (var property in properties)
                {
                    modelBuilder
                        .Entity(entityType.ClrType)
                        .Property(property.Name)
                        .HasConversion(dateTimeConverter);
                }
            }

            modelBuilder.Entity<Cliente>()
           .Property(c => c.DataNascimento)
               .HasConversion(new ValueConverter<DateTime?, DateTime?>(
                  v => v.HasValue ? v.Value.ToUniversalTime() : null,
                 v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null
                  ));

            // Configurações para Cliente
            modelBuilder.Entity<Cliente>()
                .HasMany(c => c.Enderecos)
                .WithOne(e => e.Cliente)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurações para Produto
            modelBuilder.Entity<Produto>()
                .HasMany(p => p.Ingredientes)
                .WithOne(i => i.Produto)
                .HasForeignKey(i => i.ProdutoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para melhor performance
            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.CpfCnpj)
                .IsUnique();

            modelBuilder.Entity<Produto>()
                .HasIndex(p => p.Codigo)
                .IsUnique();

            modelBuilder.Entity<Mesa>()
                .HasIndex(m => m.Numero)
                .IsUnique();

            modelBuilder.Entity<Funcionario>()
                .HasIndex(f => f.CPF)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}