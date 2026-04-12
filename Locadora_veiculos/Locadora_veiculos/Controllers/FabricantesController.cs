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
    public class FabricantesController : ControllerBase
    {
        private readonly LocadoraDbContext _context;

        public FabricantesController(LocadoraDbContext context)
        {
            _context = context;
        }

        // GET: api/Fabricantes
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<FabricanteResponseDto>), 200)]
        public async Task<ActionResult<IEnumerable<FabricanteResponseDto>>> GetFabricantes()
        {
            var fabricantes = await _context.Fabricantes
                .Include(f => f.Veiculos)
                .Select(f => new FabricanteResponseDto
                {
                    Id = f.Id,
                    Nome = f.Nome,
                    PaisOrigem = f.PaisOrigem,
                    TotalVeiculos = f.Veiculos.Count()
                })
                .ToListAsync();

            return Ok(fabricantes);
        }

        // GET: api/Fabricantes/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(FabricanteResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<FabricanteResponseDto>> GetFabricante(int id)
        {
            var fabricante = await _context.Fabricantes
                .Include(f => f.Veiculos)
                .Where(f => f.Id == id)
                .Select(f => new FabricanteResponseDto
                {
                    Id = f.Id,
                    Nome = f.Nome,
                    PaisOrigem = f.PaisOrigem,
                    TotalVeiculos = f.Veiculos.Count()
                })
                .FirstOrDefaultAsync();

            if (fabricante == null)
                return NotFound(new { mensagem = $"Fabricante com Id {id} não encontrado." });

            return Ok(fabricante);
        }

        // POST: api/Fabricantes
        [HttpPost]
        [ProducesResponseType(typeof(FabricanteResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<FabricanteResponseDto>> PostFabricante(FabricanteCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Verifica duplicidade de nome
            bool nomeExiste = await _context.Fabricantes
                .AnyAsync(f => f.Nome.ToLower() == dto.Nome.ToLower());
            if (nomeExiste)
                return BadRequest(new { mensagem = "Já existe um fabricante com esse nome." });

            var fabricante = new Fabricante
            {
                Nome = dto.Nome,
                PaisOrigem = dto.PaisOrigem
            };

            _context.Fabricantes.Add(fabricante);
            await _context.SaveChangesAsync();

            var response = new FabricanteResponseDto
            {
                Id = fabricante.Id,
                Nome = fabricante.Nome,
                PaisOrigem = fabricante.PaisOrigem,
                TotalVeiculos = 0
            };

            return CreatedAtAction(nameof(GetFabricante), new { id = fabricante.Id }, response);
        }

        // PUT: api/Fabricantes/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(FabricanteResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<FabricanteResponseDto>> PutFabricante(int id, FabricanteCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var fabricante = await _context.Fabricantes.FindAsync(id);
            if (fabricante == null)
                return NotFound(new { mensagem = $"Fabricante com Id {id} não encontrado." });

            // Verifica duplicidade (ignorando o próprio registro)
            bool nomeExiste = await _context.Fabricantes
                .AnyAsync(f => f.Nome.ToLower() == dto.Nome.ToLower() && f.Id != id);
            if (nomeExiste)
                return BadRequest(new { mensagem = "Já existe outro fabricante com esse nome." });

            fabricante.Nome = dto.Nome;
            fabricante.PaisOrigem = dto.PaisOrigem;

            await _context.SaveChangesAsync();

            return Ok(new FabricanteResponseDto
            {
                Id = fabricante.Id,
                Nome = fabricante.Nome,
                PaisOrigem = fabricante.PaisOrigem,
                TotalVeiculos = await _context.Veiculos.CountAsync(v => v.FabricanteId == id)
            });
        }

        // DELETE: api/Fabricantes/5
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteFabricante(int id)
        {
            var fabricante = await _context.Fabricantes.FindAsync(id);
            if (fabricante == null)
                return NotFound(new { mensagem = $"Fabricante com Id {id} não encontrado." });

            bool possuiVeiculos = await _context.Veiculos.AnyAsync(v => v.FabricanteId == id);
            if (possuiVeiculos)
                return BadRequest(new { mensagem = "Não é possível excluir um fabricante que possui veículos cadastrados." });

            _context.Fabricantes.Remove(fabricante);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}