using System.ComponentModel.DataAnnotations;

namespace Fynanceo.Models.Enums;
// Enums para Estoque
public enum StatusUnidadeMedida
{
    [Display(Name = "Unidade")]
    Unidade = 1,
    [Display(Name = "Quilograma")]
    Kg = 2,
    [Display(Name = "Grama")]
    g = 3,
    [Display(Name = "Litro")]
    Litro = 4,
    [Display(Name = "Mililitro")]
    ml =5,
    [Display(Name = "Metro")]
    Metro = 6,
    [Display(Name = "Centímetro")]
    cm =7,
    [Display(Name = "Pacote")]
    Pacote = 8,
    [Display(Name = "Caixa")]
    Caixa = 9

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