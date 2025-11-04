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
        public DbSet<Entregador> Entregadores { get; set; }
        public DbSet<Entrega> Entregas { get; set; }
        public DbSet<HistoricoEntrega> HistoricoEntregas { get; set; }
        public DbSet<ConfiguracaoDelivery> ConfiguracoesDelivery { get; set; }
        public DbSet<Conta> Contas { get; set; }
        public DbSet<Caixa> Caixas { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<MovimentacaoCaixa> MovimentacoesCaixa { get; set; }
        public DbSet<MovimentacaoConta> MovimentacaoContas { get; set; }

        // Módulo Estoque
        public DbSet<Estoque> Estoques { get; set; }
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
        public DbSet<CategoriaEstoque> CategoriasEstoque { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<ItemInventario> ItensInventario { get; set; }
        public DbSet<ProdutoIngrediente> ProdutoIngredientes { get; set; }


        public DbSet<CozinhaConfig> CozinhaConfigs { get; set; }





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



         

            // Configurações do Módulo Estoque
            modelBuilder.Entity<Estoque>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Codigo).HasMaxLength(20);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.EstoqueAtual).HasColumnType("decimal(18,3)");
                entity.Property(e => e.EstoqueMinimo).HasColumnType("decimal(18,3)");
                entity.Property(e => e.EstoqueMaximo).HasColumnType("decimal(18,3)");
                entity.Property(e => e.CustoUnitario).HasColumnType("decimal(18,2)");

                // Relacionamentos
                entity.HasOne(e => e.CategoriaEstoque)
                      .WithMany(c => c.Estoques)
                      .HasForeignKey(e => e.CategoriaEstoqueId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Fornecedor)
                      .WithMany()
                      .HasForeignKey(e => e.FornecedorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MovimentacaoEstoque>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantidade).HasColumnType("decimal(18,3)");
                entity.Property(e => e.CustoUnitario).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CustoTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Documento).HasMaxLength(50);
                entity.Property(e => e.Observacao).HasMaxLength(500);
                entity.Property(e => e.Usuario).HasMaxLength(100);

                // Relacionamentos
                entity.HasOne(e => e.Estoque)
                      .WithMany(e => e.Movimentacoes)
                      .HasForeignKey(e => e.EstoqueId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Fornecedor)
                      .WithMany()
                      .HasForeignKey(e => e.FornecedorId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Pedido)
                      .WithMany()
                      .HasForeignKey(e => e.PedidoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuração para garantir que só tenha uma linha na tabela
            modelBuilder.Entity<CozinhaConfig>()
                .HasData(new CozinhaConfig
                {
                    Id = 1,
                    TempoAlertaPreparoMinutos = 30,
                    TempoAlertaProntoMinutos = 10,
                    IntervaloAtualizacaoSegundos = 30
                });
        }
    }
}