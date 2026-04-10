using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Locadora_veiculos.Models
{
    public class Cliente
    { 
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Nome { get; set; }

        [Required, MaxLength(14)]
        public string CPF { get; set; }

        [Required, MaxLength(150)]
        public string Email { get; set; }

        [MaxLength(20)]
        public string Telefone { get; set; }

        public DateTime? DataNascimento { get; set; }

        // Navegação
        public ICollection<Aluguel> Alugueis { get; set; }
    }
}
