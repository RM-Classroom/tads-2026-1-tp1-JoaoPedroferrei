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
    public class CategoriasController : ControllerBase
    {
        private readonly LocadoraDbContext _context;

        public CategoriasController(LocadoraDbContext context)
        {
            _context = context;
        }

        // GET: api/Categorias
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoriaResponseDto>), 200)]
        public async Task<ActionResult<IEnumerable<CategoriaResponseDto>>> GetCategorias()
        {
            var categorias = await _context.Categorias
                .Include(c => c.Veiculos)
                .Select(c => new CategoriaResponseDto
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Descricao = c.Descricao,
                    TotalVeiculos = c.Veiculos.Count()
                })
                .ToListAsync();

            return Ok(categorias);
        }

        // GET: api/Categorias/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoriaResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<CategoriaResponseDto>> GetCategoria(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.Veiculos)
                .Where(c => c.Id == id)
                .Select(c => new CategoriaResponseDto
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Descricao = c.Descricao,
                    TotalVeiculos = c.Veiculos.Count()
                })
                .FirstOrDefaultAsync();

            if (categoria == null)
                return NotFound(new { mensagem = $"Categoria com Id {id} não encontrada." });

            return Ok(categoria);
        }

        // POST: api/Categorias
        [HttpPost]
        [ProducesResponseType(typeof(CategoriaResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<CategoriaResponseDto>> PostCategoria(CategoriaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool nomeExiste = await _context.Categorias
                .AnyAsync(c => c.Nome.ToLower() == dto.Nome.ToLower());
            if (nomeExiste)
                return BadRequest(new { mensagem = "Já existe uma categoria com esse nome." });

            var categoria = new Categoria
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            var response = new CategoriaResponseDto
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                Descricao = categoria.Descricao,
                TotalVeiculos = 0
            };

            return CreatedAtAction(nameof(GetCategoria), new { id = categoria.Id }, response);
        }

        // PUT: api/Categorias/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CategoriaResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<CategoriaResponseDto>> PutCategoria(int id, CategoriaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound(new { mensagem = $"Categoria com Id {id} não encontrada." });

            bool nomeExiste = await _context.Categorias
                .AnyAsync(c => c.Nome.ToLower() == dto.Nome.ToLower() && c.Id != id);
            if (nomeExiste)
                return BadRequest(new { mensagem = "Já existe outra categoria com esse nome." });

            categoria.Nome = dto.Nome;
            categoria.Descricao = dto.Descricao;

            await _context.SaveChangesAsync();

            return Ok(new CategoriaResponseDto
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                Descricao = categoria.Descricao,
                TotalVeiculos = await _context.Veiculos.CountAsync(v => v.CategoriaId == id)
            });
        }

        // DELETE: api/Categorias/5
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound(new { mensagem = $"Categoria com Id {id} não encontrada." });

            bool possuiVeiculos = await _context.Veiculos.AnyAsync(v => v.CategoriaId == id);
            if (possuiVeiculos)
                return BadRequest(new { mensagem = "Não é possível excluir uma categoria que possui veículos cadastrados." });

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}