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
    public class ClientesController : ControllerBase
    {
        private readonly LocadoraDbContext _context;

        public ClientesController(LocadoraDbContext context)
        {
            _context = context;
        }

        // GET: api/Clientes
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponseDto>), 200)]
        public async Task<ActionResult<IEnumerable<ClienteResponseDto>>> GetClientes()
        {
            var clientes = await _context.Clientes
                .Include(c => c.Alugueis)
                .Select(c => new ClienteResponseDto
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    CPF = c.CPF,
                    Email = c.Email,
                    Telefone = c.Telefone,
                    DataNascimento = c.DataNascimento,
                    TotalAlugueis = c.Alugueis.Count()
                })
                .ToListAsync();

            return Ok(clientes);
        }

        // GET: api/Clientes/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ClienteResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ClienteResponseDto>> GetCliente(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Alugueis)
                .Where(c => c.Id == id)
                .Select(c => new ClienteResponseDto
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    CPF = c.CPF,
                    Email = c.Email,
                    Telefone = c.Telefone,
                    DataNascimento = c.DataNascimento,
                    TotalAlugueis = c.Alugueis.Count()
                })
                .FirstOrDefaultAsync();

            if (cliente == null)
                return NotFound(new { mensagem = $"Cliente com Id {id} não encontrado." });

            return Ok(cliente);
        }

        // POST: api/Clientes
        [HttpPost]
        [ProducesResponseType(typeof(ClienteResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ClienteResponseDto>> PostCliente(ClienteCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool cpfExiste = await _context.Clientes.AnyAsync(c => c.CPF == dto.CPF);
            if (cpfExiste)
                return BadRequest(new { mensagem = "CPF já cadastrado." });

            bool emailExiste = await _context.Clientes.AnyAsync(c => c.Email.ToLower() == dto.Email.ToLower());
            if (emailExiste)
                return BadRequest(new { mensagem = "Email já cadastrado." });

            var cliente = new Cliente
            {
                Nome = dto.Nome,
                CPF = dto.CPF,
                Email = dto.Email,
                Telefone = dto.Telefone,
                DataNascimento = dto.DataNascimento
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, new ClienteResponseDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                CPF = cliente.CPF,
                Email = cliente.Email,
                Telefone = cliente.Telefone,
                DataNascimento = cliente.DataNascimento,
                TotalAlugueis = 0
            });
        }

        // PUT: api/Clientes/5
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ClienteResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ClienteResponseDto>> PutCliente(int id, ClienteUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound(new { mensagem = $"Cliente com Id {id} não encontrado." });

            bool cpfExiste = await _context.Clientes.AnyAsync(c => c.CPF == dto.CPF && c.Id != id);
            if (cpfExiste)
                return BadRequest(new { mensagem = "CPF já cadastrado em outro cliente." });

            bool emailExiste = await _context.Clientes
                .AnyAsync(c => c.Email.ToLower() == dto.Email.ToLower() && c.Id != id);
            if (emailExiste)
                return BadRequest(new { mensagem = "Email já cadastrado em outro cliente." });

            cliente.Nome = dto.Nome;
            cliente.CPF = dto.CPF;
            cliente.Email = dto.Email;
            cliente.Telefone = dto.Telefone;
            cliente.DataNascimento = dto.DataNascimento;

            await _context.SaveChangesAsync();

            return Ok(new ClienteResponseDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                CPF = cliente.CPF,
                Email = cliente.Email,
                Telefone = cliente.Telefone,
                DataNascimento = cliente.DataNascimento,
                TotalAlugueis = await _context.Alugueis.CountAsync(a => a.ClienteId == id)
            });
        }

        // DELETE: api/Clientes/5
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound(new { mensagem = $"Cliente com Id {id} não encontrado." });

            bool possuiAlugueis = await _context.Alugueis.AnyAsync(a => a.ClienteId == id);
            if (possuiAlugueis)
                return BadRequest(new { mensagem = "Não é possível excluir um cliente que possui aluguéis registrados." });

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}