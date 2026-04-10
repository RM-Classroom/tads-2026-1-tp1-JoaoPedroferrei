using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Locadora_veiculos.Models
{
    public class Veiculo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FabricanteId { get; set; }

        [Required]
        public int CategoriaId { get; set; }

        [Required, MaxLength(100)]
        public string Modelo { get; set; }

        [Required]
        public int AnoFabricacao { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, double.MaxValue)]
        public decimal Quilometragem { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal ValorDiaria { get; set; }

        public bool Disponivel { get; set; } = true;

        // Navegação
        [ForeignKey("FabricanteId")]
        public Fabricante Fabricante { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; }

        public ICollection<Aluguel> Alugueis { get; set; }
    }
}
