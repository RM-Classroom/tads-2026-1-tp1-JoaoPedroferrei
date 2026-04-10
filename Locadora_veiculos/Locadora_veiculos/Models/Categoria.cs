using System.ComponentModel.DataAnnotations;

namespace Locadora_veiculos.Models
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Nome { get; set; }

        [MaxLength(255)]
        public string Descricao { get; set; }

        public ICollection<Veiculo> Veiculos { get; set; }
    }
}
