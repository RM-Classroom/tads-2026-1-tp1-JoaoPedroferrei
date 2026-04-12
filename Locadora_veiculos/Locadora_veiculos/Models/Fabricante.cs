using System.ComponentModel.DataAnnotations;

namespace Locadora_veiculos.Models
{
    public class Fabricante
    {
        //(marca do veículo)
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Nome { get; set; }

        [MaxLength(100)]
        public string PaisOrigem { get; set; }

        public ICollection<Veiculo> Veiculos { get; set; }
    }
}
