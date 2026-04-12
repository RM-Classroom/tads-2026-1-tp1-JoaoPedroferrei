using System.ComponentModel.DataAnnotations;
using static Locadora_veiculos.Models.Aluguel;

namespace Locadora_veiculos.DTOs
{
    // ──────────────────────── FABRICANTE ────────────────────────
    public class FabricanteCreateDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(100)]
        public string Nome { get; set; }

        [MaxLength(100)]
        public string PaisOrigem { get; set; }
    }

    public class FabricanteResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string PaisOrigem { get; set; }
        public int TotalVeiculos { get; set; }
    }

    // ──────────────────────── CATEGORIA ────────────────────────
    public class CategoriaCreateDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(80)]
        public string Nome { get; set; }

        [MaxLength(255)]
        public string Descricao { get; set; }
    }

    public class CategoriaResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public int TotalVeiculos { get; set; }
    }

    // ──────────────────────── VEÍCULO ────────────────────────
    public class VeiculoCreateDto
    {
        [Required(ErrorMessage = "FabricanteId é obrigatório")]
        public int FabricanteId { get; set; }

        [Required(ErrorMessage = "CategoriaId é obrigatório")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "Modelo é obrigatório")]
        [MaxLength(100)]
        public string Modelo { get; set; }

        [Required]
        [Range(1900, 2100, ErrorMessage = "Ano inválido")]
        public int AnoFabricacao { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Quilometragem deve ser positiva")]
        public decimal Quilometragem { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor da diária deve ser positivo")]
        public decimal ValorDiaria { get; set; }

        public bool Disponivel { get; set; } = true;
    }

    public class VeiculoUpdateDto : VeiculoCreateDto { }

    public class VeiculoResponseDto
    {
        public int Id { get; set; }
        public string Modelo { get; set; }
        public int AnoFabricacao { get; set; }
        public decimal Quilometragem { get; set; }
        public decimal ValorDiaria { get; set; }
        public bool Disponivel { get; set; }
        public string NomeFabricante { get; set; }
        public string NomeCategoria { get; set; }
    }

    // ──────────────────────── CLIENTE ────────────────────────
    public class ClienteCreateDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(150)]
        public string Nome { get; set; }

        [Required(ErrorMessage = "CPF é obrigatório")]
        [MaxLength(14)]
        [RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$|^\d{11}$",
            ErrorMessage = "CPF inválido. Use formato 000.000.000-00 ou 00000000000")]
        public string CPF { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        [MaxLength(150)]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Telefone { get; set; }

        public DateTime? DataNascimento { get; set; }
    }

    public class ClienteUpdateDto : ClienteCreateDto { }

    public class ClienteResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string CPF { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public DateTime? DataNascimento { get; set; }
        public int TotalAlugueis { get; set; }
    }

    // ──────────────────────── ALUGUEL ────────────────────────
    public class AluguelCreateDto
    {
        [Required(ErrorMessage = "ClienteId é obrigatório")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "VeiculoId é obrigatório")]
        public int VeiculoId { get; set; }

        [Required(ErrorMessage = "Data de início é obrigatória")]
        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "Data de fim é obrigatória")]
        public DateTime DataFim { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "KM inicial deve ser positivo")]
        public decimal KmInicial { get; set; }
    }

    public class AluguelDevolucaoDto
    {
        [Required(ErrorMessage = "KM final é obrigatório")]
        [Range(0, double.MaxValue, ErrorMessage = "KM final deve ser positivo")]
        public decimal KmFinal { get; set; }

        [Required(ErrorMessage = "Data de devolução é obrigatória")]
        public DateTime DataDevolucao { get; set; }
    }

    public class AluguelResponseDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string NomeCliente { get; set; }
        public int VeiculoId { get; set; }
        public string ModeloVeiculo { get; set; }
        public string FabricanteVeiculo { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public DateTime? DataDevolucao { get; set; }
        public decimal KmInicial { get; set; }
        public decimal? KmFinal { get; set; }
        public decimal ValorDiaria { get; set; }
        public decimal? ValorTotal { get; set; }
        public string Status { get; set; }
    }

    // ──────────────────────── FILTROS ────────────────────────
    public class VeiculoComCategoriaFabricanteDto
    {
        public int Id { get; set; }
        public string Modelo { get; set; }
        public int AnoFabricacao { get; set; }
        public decimal ValorDiaria { get; set; }
        public bool Disponivel { get; set; }
        public string Fabricante { get; set; }
        public string PaisOrigem { get; set; }
        public string Categoria { get; set; }
        public string DescricaoCategoria { get; set; }
    }

    public class AluguelDetalhadoDto
    {
        public int AluguelId { get; set; }
        public string NomeCliente { get; set; }
        public string CPFCliente { get; set; }
        public string ModeloVeiculo { get; set; }
        public string Fabricante { get; set; }
        public string Categoria { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public decimal ValorDiaria { get; set; }
        public decimal? ValorTotal { get; set; }
        public string Status { get; set; }
    }

    public class HistoricoClienteDto
    {
        public string NomeCliente { get; set; }
        public string Email { get; set; }
        public int TotalAlugueis { get; set; }
        public decimal? TotalGasto { get; set; }
        public DateTime? UltimoAluguel { get; set; }
    }

    public class VeiculoDisponivelFiltroDto
    {
        public int Id { get; set; }
        public string Modelo { get; set; }
        public string Fabricante { get; set; }
        public string Categoria { get; set; }
        public int AnoFabricacao { get; set; }
        public decimal ValorDiaria { get; set; }
        public decimal Quilometragem { get; set; }
    }

    public class RelatorioAluguelPorCategoriaDto
    {
        public string Categoria { get; set; }
        public int TotalAlugueis { get; set; }
        public decimal? ReceitaTotal { get; set; }
        public decimal ValorDiariaMedia { get; set; }
    }
}