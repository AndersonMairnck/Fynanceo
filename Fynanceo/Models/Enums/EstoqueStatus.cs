using System.ComponentModel.DataAnnotations;

namespace Fynanceo.Models.Enums;
// Enums para Estoque
public enum UnidadeMedida
{
    [Display(Name = "Unidade")]
    Unidade,
    [Display(Name = "Quilograma")]
    Kg,
    [Display(Name = "Grama")]
    g,
    [Display(Name = "Litro")]
    Litro,
    [Display(Name = "Mililitro")]
    ml,
    [Display(Name = "Metro")]
    Metro,
    [Display(Name = "Centímetro")]
    cm,
    [Display(Name = "Pacote")]
    Pacote,
    [Display(Name = "Caixa")]
    Caixa
}

public enum TipoMovimentacaoEstoque
{
    [Display(Name = "Entrada")]
    Entrada,
    [Display(Name = "Saída")]
    Saida,
    [Display(Name = "Ajuste")]
    Ajuste,
    [Display(Name = "Perda")]
    Perda,
    [Display(Name = "Transferência")]
    Transferencia
}

public enum StatusEstoque
{
    // [Display(Name = "Ativo")]
    // Ativo,
    // [Display(Name = "Inativo")]
    // Inativo,
    // [Display(Name = "Bloqueado")]
    // Bloqueado
    Ativo = 1,
    Inativo = 2,
    Bloqueado = 3,

}

public enum StatusInventario
{
    [Display(Name = "Aberto")]
    Aberto,
    [Display(Name = "Em Andamento")]
    EmAndamento,
    [Display(Name = "Concluído")]
    Concluido,
    [Display(Name = "Cancelado")]
    Cancelado
}