using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Locadora_veiculos.Data;
using Locadora_veiculos.Models;
using Locadora_veiculos.DTOs;

namespace Locadora_veiculos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VeiculosController : ControllerBase
    {
        private readonly LocadoraDbContext _context;

        public VeiculosController(LocadoraDbContext context)
        {
            _context = context;
        }

        // GET: api/Veiculos
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<VeiculoResponseDto>), 200)]
        public async Task<ActionResult<IEnumerable<VeiculoResponseDto>>> GetVeiculos()
        {
            var veiculos = await _context.Veiculos
                .Include(v => v.Fabricante)
                .Include(v => v.Categoria)
                .Select(v => new VeiculoResponseDto
                {
                    Id = v.Id,
                    Modelo = v.Modelo,
                    AnoFabricacao = v.AnoFabricacao,
                    Quilometragem = v.Quilometragem,
                    ValorDiaria = v.ValorDiaria,
                    Disponivel = v.Disponivel,
                    NomeFabricante = v.Fabricante.Nome,
                    NomeCategoria = v.Categoria.Nome
                })
                .ToListAsync();

            return Ok(veiculos);
        }

        // GET: api/Veiculos/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VeiculoResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VeiculoResponseDto>> GetVeiculo(int id)
        {
            var veiculo = await _context.Veiculos
                .Include(v => v.Fabricante)
                .Include(v => v.Categoria)
                .Where(v => v.Id == id)
                .Select(v => new VeiculoResponseDto
                {
                    Id = v.Id,
                    Modelo = v.Modelo,
                    AnoFabricacao = v.AnoFabricacao,
                    Quilometragem = v.Quilometragem,
                    ValorDiaria = v.ValorDiaria,
                    Disponivel = v.Disponivel,
                    NomeFabricante = v.Fabricante.Nome,
                    NomeCategoria = v.Categoria.Nome
                })
                .FirstOrDefaultAsync();

            if (veiculo == null)
                return NotFound(new { mensagem = $"Veículo com Id {id} não encontrado." });

            return Ok(veiculo);
        }

        // POST: api/Veiculos
        [HttpPost]
        [ProducesResponseType(typeof(VeiculoResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<VeiculoResponseDto>> PostVeiculo(VeiculoCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool fabricanteExiste = await _context.Fabricantes.AnyAsync(f => f.Id == dto.FabricanteId);
            if (!fabricanteExiste)
                return BadRequest(new { mensagem = $"Fabricante com Id {dto.FabricanteId} não encontrado." });

            bool categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId);
            if (!categoriaExiste)
                return BadRequest(new { mensagem = $"Categoria com Id {dto.CategoriaId} não encontrada." });

            var veiculo = new Veiculo
            {
                FabricanteId = dto.FabricanteId,
                CategoriaId = dto.CategoriaId,
                Modelo = dto.Modelo,
                AnoFabricacao = dto.AnoFabricacao,
                Quilometragem = dto.Quilometragem,
                ValorDiaria = dto.ValorDiaria,
                Disponivel = dto.Disponivel
            };

            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();

            await _context.Entry(veiculo).Reference(v => v.Fabricante).LoadAsync();
            await _context.Entry(veiculo).Reference(v => v.Categoria).LoadAsync();

            return CreatedAtAction(nameof(GetVeiculo), new { id = veiculo.Id }, new VeiculoResponseDto
            {
                Id = veiculo.Id,
                Modelo = veiculo.Modelo,
                AnoFabricacao = veiculo.AnoFabricacao,
                Quilometragem = veiculo.Quilometragem,
                ValorDiaria = veiculo.ValorDiaria,
                Disponivel = veiculo.Disponivel,
                NomeFabricante = veiculo.Fabricante.Nome,
                NomeCategoria = veiculo.Categoria.Nome
            });
        }

        // PUT: api/Veiculos/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(VeiculoResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VeiculoResponseDto>> PutVeiculo(int id, VeiculoUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var veiculo = await _context.Veiculos
                .Include(v => v.Fabricante)
                .Include(v => v.Categoria)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (veiculo == null)
                return NotFound(new { mensagem = $"Veículo com Id {id} não encontrado." });

            bool fabricanteExiste = await _context.Fabricantes.AnyAsync(f => f.Id == dto.FabricanteId);
            if (!fabricanteExiste)
                return BadRequest(new { mensagem = $"Fabricante com Id {dto.FabricanteId} não encontrado." });

            bool categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == dto.CategoriaId);
            if (!categoriaExiste)
                return BadRequest(new { mensagem = $"Categoria com Id {dto.CategoriaId} não encontrada." });

            veiculo.FabricanteId = dto.FabricanteId;
            veiculo.CategoriaId = dto.CategoriaId;
            veiculo.Modelo = dto.Modelo;
            veiculo.AnoFabricacao = dto.AnoFabricacao;
            veiculo.Quilometragem = dto.Quilometragem;
            veiculo.ValorDiaria = dto.ValorDiaria;
            veiculo.Disponivel = dto.Disponivel;

            await _context.SaveChangesAsync();

            await _context.Entry(veiculo).Reference(v => v.Fabricante).LoadAsync();
            await _context.Entry(veiculo).Reference(v => v.Categoria).LoadAsync();

            return Ok(new VeiculoResponseDto
            {
                Id = veiculo.Id,
                Modelo = veiculo.Modelo,
                AnoFabricacao = veiculo.AnoFabricacao,
                Quilometragem = veiculo.Quilometragem,
                ValorDiaria = veiculo.ValorDiaria,
                Disponivel = veiculo.Disponivel,
                NomeFabricante = veiculo.Fabricante.Nome,
                NomeCategoria = veiculo.Categoria.Nome
            });
        }

        // DELETE: api/Veiculos/5
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteVeiculo(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo == null)
                return NotFound(new { mensagem = $"Veículo com Id {id} não encontrado." });

            bool possuiAlugueis = await _context.Alugueis.AnyAsync(a => a.VeiculoId == id);
            if (possuiAlugueis)
                return BadRequest(new { mensagem = "Não é possível excluir um veículo que possui aluguéis registrados." });

            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}