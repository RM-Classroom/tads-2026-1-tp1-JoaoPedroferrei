using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Locadora_veiculos.Models
{
    public class Aluguel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int VeiculoId { get; set; }

        [Required]
        public DateTime DataInicio { get; set; }

        [Required]
        public DateTime DataFim { get; set; }

        public DateTime? DataDevolucao { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, double.MaxValue)]
        public decimal KmInicial { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(0, double.MaxValue)]
        public decimal? KmFinal { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, double.MaxValue)]
        public decimal ValorDiaria { get; set; }

        [Column(TypeName = $"decimal(10,2)")]
        [Range(0, double.MaxValue)]
        public decimal? ValorTotal { get; set; }

        // "ex: StatusAluguel.Aberto"
        public enum StatusAluguel
        {
            Aberto,
            Concluido,
            Cancelado
        }
        [Required]
        public StatusAluguel Status { get; set; } = StatusAluguel.Aberto; 
        

        // Navegação
        [ForeignKey("ClienteId")]
        public Cliente Cliente { get; set; }

        [ForeignKey("VeiculoId")]
        public Veiculo Veiculo { get; set; }

    }
}
