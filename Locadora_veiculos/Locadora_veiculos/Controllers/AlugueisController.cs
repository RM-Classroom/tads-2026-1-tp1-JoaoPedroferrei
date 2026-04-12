using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Locadora_veiculos.Data;
using Locadora_veiculos.Models;
using Locadora_veiculos.DTOs;
using static Locadora_veiculos.Models.Aluguel;

namespace Locadora_veiculos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AlugueisController : ControllerBase
    {
        private readonly LocadoraDbContext _context;

        public AlugueisController(LocadoraDbContext context)
        {
            _context = context;
        }

        // GET: api/Alugueis
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AluguelResponseDto>), 200)]
        public async Task<ActionResult<IEnumerable<AluguelResponseDto>>> GetAlugueis()
        {
            var alugueis = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo).ThenInclude(v => v.Fabricante)
                .Select(a => new AluguelResponseDto
                {
                    Id = a.Id,
                    ClienteId = a.ClienteId,
                    NomeCliente = a.Cliente.Nome,
                    VeiculoId = a.VeiculoId,
                    ModeloVeiculo = a.Veiculo.Modelo,
                    FabricanteVeiculo = a.Veiculo.Fabricante.Nome,
                    DataInicio = a.DataInicio,
                    DataFim = a.DataFim,
                    DataDevolucao = a.DataDevolucao,
                    KmInicial = a.KmInicial,
                    KmFinal = a.KmFinal,
                    ValorDiaria = a.ValorDiaria,
                    ValorTotal = a.ValorTotal,
                    Status = a.Status.ToString()
                })
                .ToListAsync();

            return Ok(alugueis);
        }

        // GET: api/Alugueis/5
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AluguelResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AluguelResponseDto>> GetAluguel(int id)
        {
            var aluguel = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo).ThenInclude(v => v.Fabricante)
                .Where(a => a.Id == id)
                .Select(a => new AluguelResponseDto
                {
                    Id = a.Id,
                    ClienteId = a.ClienteId,
                    NomeCliente = a.Cliente.Nome,
                    VeiculoId = a.VeiculoId,
                    ModeloVeiculo = a.Veiculo.Modelo,
                    FabricanteVeiculo = a.Veiculo.Fabricante.Nome,
                    DataInicio = a.DataInicio,
                    DataFim = a.DataFim,
                    DataDevolucao = a.DataDevolucao,
                    KmInicial = a.KmInicial,
                    KmFinal = a.KmFinal,
                    ValorDiaria = a.ValorDiaria,
                    ValorTotal = a.ValorTotal,
                    Status = a.Status.ToString()
                })
                .FirstOrDefaultAsync();

            if (aluguel == null)
                return NotFound(new { mensagem = $"Aluguel com Id {id} não encontrado." });

            return Ok(aluguel);
        }

        // POST: api/Alugueis
        [HttpPost]
        [ProducesResponseType(typeof(AluguelResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<AluguelResponseDto>> PostAluguel(AluguelCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.DataFim <= dto.DataInicio)
                return BadRequest(new { mensagem = "A data de fim deve ser posterior à data de início." });

            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null)
                return BadRequest(new { mensagem = $"Cliente com Id {dto.ClienteId} não encontrado." });

            var veiculo = await _context.Veiculos
                .Include(v => v.Fabricante)
                .FirstOrDefaultAsync(v => v.Id == dto.VeiculoId);
            if (veiculo == null)
                return BadRequest(new { mensagem = $"Veículo com Id {dto.VeiculoId} não encontrado." });

            if (!veiculo.Disponivel)
                return BadRequest(new { mensagem = "Veículo não disponível para locação." });

            if (dto.KmInicial < veiculo.Quilometragem)
                return BadRequest(new { mensagem = $"KM inicial ({dto.KmInicial}) não pode ser menor que a quilometragem atual do veículo ({veiculo.Quilometragem})." });

            // Calcula o valor da diária com base no veículo
            var totalDias = (dto.DataFim - dto.DataInicio).Days;
            if (totalDias < 1) totalDias = 1;

            var aluguel = new Aluguel
            {
                ClienteId = dto.ClienteId,
                VeiculoId = dto.VeiculoId,
                DataInicio = dto.DataInicio,
                DataFim = dto.DataFim,
                KmInicial = dto.KmInicial,
                ValorDiaria = veiculo.ValorDiaria,
                Status = StatusAluguel.Aberto
            };

            // Marca veículo como indisponível
            veiculo.Disponivel = false;

            _context.Alugueis.Add(aluguel);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAluguel), new { id = aluguel.Id }, new AluguelResponseDto
            {
                Id = aluguel.Id,
                ClienteId = aluguel.ClienteId,
                NomeCliente = cliente.Nome,
                VeiculoId = aluguel.VeiculoId,
                ModeloVeiculo = veiculo.Modelo,
                FabricanteVeiculo = veiculo.Fabricante.Nome,
                DataInicio = aluguel.DataInicio,
                DataFim = aluguel.DataFim,
                DataDevolucao = aluguel.DataDevolucao,
                KmInicial = aluguel.KmInicial,
                KmFinal = aluguel.KmFinal,
                ValorDiaria = aluguel.ValorDiaria,
                ValorTotal = aluguel.ValorTotal,
                Status = aluguel.Status.ToString()
            });
        }

        // PUT: api/Alugueis/5/devolver  → devolução do veículo
        [HttpPut("{id}/devolver")]
        [ProducesResponseType(typeof(AluguelResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AluguelResponseDto>> DevolverVeiculo(int id, AluguelDevolucaoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var aluguel = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo).ThenInclude(v => v.Fabricante)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aluguel == null)
                return NotFound(new { mensagem = $"Aluguel com Id {id} não encontrado." });

            if (aluguel.Status != StatusAluguel.Aberto)
                return BadRequest(new { mensagem = $"Aluguel com status '{aluguel.Status}' não pode ser devolvido." });

            if (dto.KmFinal < aluguel.KmInicial)
                return BadRequest(new { mensagem = "KM final não pode ser menor que o KM inicial." });

            if (dto.DataDevolucao < aluguel.DataInicio)
                return BadRequest(new { mensagem = "Data de devolução não pode ser anterior à data de início." });

            // Calcula valor total com base na data de devolução real
            var diasEfetivos = (dto.DataDevolucao - aluguel.DataInicio).Days;
            if (diasEfetivos < 1) diasEfetivos = 1;

            aluguel.KmFinal = dto.KmFinal;
            aluguel.DataDevolucao = dto.DataDevolucao;
            aluguel.ValorTotal = diasEfetivos * aluguel.ValorDiaria;
            aluguel.Status = StatusAluguel.Concluido;

            // Atualiza quilometragem do veículo e disponibilidade
            aluguel.Veiculo.Quilometragem = dto.KmFinal;
            aluguel.Veiculo.Disponivel = true;

            await _context.SaveChangesAsync();

            return Ok(new AluguelResponseDto
            {
                Id = aluguel.Id,
                ClienteId = aluguel.ClienteId,
                NomeCliente = aluguel.Cliente.Nome,
                VeiculoId = aluguel.VeiculoId,
                ModeloVeiculo = aluguel.Veiculo.Modelo,
                FabricanteVeiculo = aluguel.Veiculo.Fabricante.Nome,
                DataInicio = aluguel.DataInicio,
                DataFim = aluguel.DataFim,
                DataDevolucao = aluguel.DataDevolucao,
                KmInicial = aluguel.KmInicial,
                KmFinal = aluguel.KmFinal,
                ValorDiaria = aluguel.ValorDiaria,
                ValorTotal = aluguel.ValorTotal,
                Status = aluguel.Status.ToString()
            });
        }

        // PUT: api/Alugueis/5/cancelar
        [HttpPut("{id}/cancelar")]
        [ProducesResponseType(typeof(AluguelResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AluguelResponseDto>> CancelarAluguel(int id)
        {
            var aluguel = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo).ThenInclude(v => v.Fabricante)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aluguel == null)
                return NotFound(new { mensagem = $"Aluguel com Id {id} não encontrado." });

            if (aluguel.Status != StatusAluguel.Aberto)
                return BadRequest(new { mensagem = $"Apenas aluguéis com status 'Aberto' podem ser cancelados." });

            aluguel.Status = StatusAluguel.Cancelado;
            aluguel.Veiculo.Disponivel = true;

            await _context.SaveChangesAsync();

            return Ok(new AluguelResponseDto
            {
                Id = aluguel.Id,
                ClienteId = aluguel.ClienteId,
                NomeCliente = aluguel.Cliente.Nome,
                VeiculoId = aluguel.VeiculoId,
                ModeloVeiculo = aluguel.Veiculo.Modelo,
                FabricanteVeiculo = aluguel.Veiculo.Fabricante.Nome,
                DataInicio = aluguel.DataInicio,
                DataFim = aluguel.DataFim,
                DataDevolucao = aluguel.DataDevolucao,
                KmInicial = aluguel.KmInicial,
                KmFinal = aluguel.KmFinal,
                ValorDiaria = aluguel.ValorDiaria,
                ValorTotal = aluguel.ValorTotal,
                Status = aluguel.Status.ToString()
            });
        }

        // DELETE: api/Alugueis/5
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteAluguel(int id)
        {
            var aluguel = await _context.Alugueis
                .Include(a => a.Veiculo)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aluguel == null)
                return NotFound(new { mensagem = $"Aluguel com Id {id} não encontrado." });

            if (aluguel.Status == StatusAluguel.Aberto)
                return BadRequest(new { mensagem = "Cancele o aluguel antes de excluí-lo." });

            _context.Alugueis.Remove(aluguel);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}