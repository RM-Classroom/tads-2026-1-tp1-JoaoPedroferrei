using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Locadora_veiculos.Data;
using Locadora_veiculos.DTOs;
using static Locadora_veiculos.Models.Aluguel;

namespace Locadora_veiculos.Controllers
{
    /// <summary>
    /// Endpoints de filtros e relatórios com múltiplos JOINs entre tabelas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FiltrosController : ControllerBase
    {
        private readonly LocadoraDbContext _context;

        public FiltrosController(LocadoraDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────────────
        // FILTRO 1 — Veículos com dados de Fabricante e Categoria
        // JOIN: Veiculos INNER JOIN Fabricantes INNER JOIN Categorias
        // Parâmetros opcionais: disponivel, categoriaId, fabricanteId
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Retorna veículos com informações completas de fabricante e categoria.
        /// Utiliza INNER JOIN entre Veiculos, Fabricantes e Categorias.
        /// </summary>
        [HttpGet("veiculos-completo")]
        [ProducesResponseType(typeof(IEnumerable<VeiculoComCategoriaFabricanteDto>), 200)]
        public async Task<ActionResult<IEnumerable<VeiculoComCategoriaFabricanteDto>>> GetVeiculosCompleto(
            [FromQuery] bool? disponivel,
            [FromQuery] int? categoriaId,
            [FromQuery] int? fabricanteId)
        {
            // INNER JOIN: Veiculos ⟶ Fabricantes ⟶ Categorias
            var query = from v in _context.Veiculos
                        join f in _context.Fabricantes on v.FabricanteId equals f.Id
                        join c in _context.Categorias on v.CategoriaId equals c.Id
                        select new VeiculoComCategoriaFabricanteDto
                        {
                            Id = v.Id,
                            Modelo = v.Modelo,
                            AnoFabricacao = v.AnoFabricacao,
                            ValorDiaria = v.ValorDiaria,
                            Disponivel = v.Disponivel,
                            Fabricante = f.Nome,
                            PaisOrigem = f.PaisOrigem,
                            Categoria = c.Nome,
                            DescricaoCategoria = c.Descricao
                        };

            if (disponivel.HasValue)
                query = query.Where(v => v.Disponivel == disponivel.Value);

            if (categoriaId.HasValue)
                query = from v in _context.Veiculos
                        join f in _context.Fabricantes on v.FabricanteId equals f.Id
                        join c in _context.Categorias on v.CategoriaId equals c.Id
                        where v.CategoriaId == categoriaId.Value
                              && (!disponivel.HasValue || v.Disponivel == disponivel.Value)
                        select new VeiculoComCategoriaFabricanteDto
                        {
                            Id = v.Id,
                            Modelo = v.Modelo,
                            AnoFabricacao = v.AnoFabricacao,
                            ValorDiaria = v.ValorDiaria,
                            Disponivel = v.Disponivel,
                            Fabricante = f.Nome,
                            PaisOrigem = f.PaisOrigem,
                            Categoria = c.Nome,
                            DescricaoCategoria = c.Descricao
                        };

            if (fabricanteId.HasValue)
                query = from v in _context.Veiculos
                        join f in _context.Fabricantes on v.FabricanteId equals f.Id
                        join c in _context.Categorias on v.CategoriaId equals c.Id
                        where v.FabricanteId == fabricanteId.Value
                              && (!categoriaId.HasValue || v.CategoriaId == categoriaId.Value)
                              && (!disponivel.HasValue || v.Disponivel == disponivel.Value)
                        select new VeiculoComCategoriaFabricanteDto
                        {
                            Id = v.Id,
                            Modelo = v.Modelo,
                            AnoFabricacao = v.AnoFabricacao,
                            ValorDiaria = v.ValorDiaria,
                            Disponivel = v.Disponivel,
                            Fabricante = f.Nome,
                            PaisOrigem = f.PaisOrigem,
                            Categoria = c.Nome,
                            DescricaoCategoria = c.Descricao
                        };

            var resultado = await query.OrderBy(v => v.Fabricante).ThenBy(v => v.Modelo).ToListAsync();
            return Ok(resultado);
        }

        // ─────────────────────────────────────────────────────────────────────
        // FILTRO 2 — Aluguéis detalhados (cliente + veículo + fabricante + categoria)
        // JOIN: Alugueis INNER JOIN Clientes INNER JOIN Veiculos INNER JOIN Fabricantes LEFT JOIN Categorias
        // Parâmetros opcionais: status, clienteId, dataInicio, dataFim
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Retorna aluguéis com dados completos do cliente, veículo, fabricante e categoria.
        /// Utiliza INNER JOIN (Alugueis→Clientes, Alugueis→Veiculos→Fabricantes) e LEFT JOIN (→Categorias).
        /// </summary>
        [HttpGet("alugueis-detalhado")]
        [ProducesResponseType(typeof(IEnumerable<AluguelDetalhadoDto>), 200)]
        public async Task<ActionResult<IEnumerable<AluguelDetalhadoDto>>> GetAlugueisDetalhado(
            [FromQuery] string status,
            [FromQuery] int? clienteId,
            [FromQuery] DateTime? dataInicio,
            [FromQuery] DateTime? dataFim)
        {
            // INNER JOIN: Alugueis → Clientes → Veiculos → Fabricantes
            // LEFT JOIN: Veiculos → Categorias (categoria pode não existir em edge cases)
            var query = from a in _context.Alugueis
                        join cl in _context.Clientes on a.ClienteId equals cl.Id          // INNER JOIN
                        join v in _context.Veiculos on a.VeiculoId equals v.Id            // INNER JOIN
                        join f in _context.Fabricantes on v.FabricanteId equals f.Id      // INNER JOIN
                        join c in _context.Categorias on v.CategoriaId equals c.Id into catGroup
                        from cat in catGroup.DefaultIfEmpty()                             // LEFT JOIN
                        select new AluguelDetalhadoDto
                        {
                            AluguelId = a.Id,
                            NomeCliente = cl.Nome,
                            CPFCliente = cl.CPF,
                            ModeloVeiculo = v.Modelo,
                            Fabricante = f.Nome,
                            Categoria = cat != null ? cat.Nome : "Sem categoria",
                            DataInicio = a.DataInicio,
                            DataFim = a.DataFim,
                            ValorDiaria = a.ValorDiaria,
                            ValorTotal = a.ValorTotal,
                            Status = a.Status.ToString()
                        };

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (!Enum.TryParse<StatusAluguel>(status, true, out var statusEnum))
                    return BadRequest(new { mensagem = "Status inválido. Use: Aberto, Concluido ou Cancelado." });
                query = query.Where(a => a.Status == statusEnum.ToString());
            }

            if (clienteId.HasValue)
                query = query.Where(a => _context.Alugueis
                    .Any(al => al.Id == a.AluguelId && al.ClienteId == clienteId.Value));

            if (dataInicio.HasValue)
                query = query.Where(a => a.DataInicio >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(a => a.DataFim <= dataFim.Value);

            var resultado = await query.OrderByDescending(a => a.DataInicio).ToListAsync();
            return Ok(resultado);
        }

        // ─────────────────────────────────────────────────────────────────────
        // FILTRO 3 — Histórico e ranking de clientes por total de aluguéis/gasto
        // JOIN: Clientes LEFT JOIN Alugueis (agrupa por cliente)
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Retorna histórico consolidado de cada cliente: total de aluguéis, valor gasto e data do último aluguel.
        /// Utiliza LEFT JOIN entre Clientes e Alugueis para incluir clientes sem aluguéis.
        /// </summary>
        [HttpGet("historico-clientes")]
        [ProducesResponseType(typeof(IEnumerable<HistoricoClienteDto>), 200)]
        public async Task<ActionResult<IEnumerable<HistoricoClienteDto>>> GetHistoricoClientes(
            [FromQuery] int top = 10)
        {
            if (top < 1 || top > 100)
                return BadRequest(new { mensagem = "O parâmetro 'top' deve estar entre 1 e 100." });

            // LEFT JOIN: Clientes → Alugueis (inclui clientes sem nenhum aluguel)
            var resultado = await (
                from cl in _context.Clientes
                join a in _context.Alugueis on cl.Id equals a.ClienteId into alugueis
                from al in alugueis.DefaultIfEmpty()                 // LEFT JOIN
                group al by new { cl.Id, cl.Nome, cl.Email } into g
                select new HistoricoClienteDto
                {
                    NomeCliente = g.Key.Nome,
                    Email = g.Key.Email,
                    TotalAlugueis = g.Count(x => x != null),
                    TotalGasto = g.Sum(x => x != null ? x.ValorTotal : 0),
                    UltimoAluguel = g.Max(x => x != null ? x.DataInicio : (DateTime?)null)
                })
                .OrderByDescending(c => c.TotalAlugueis)
                .Take(top)
                .ToListAsync();

            return Ok(resultado);
        }

        // ─────────────────────────────────────────────────────────────────────
        // FILTRO 4 — Veículos disponíveis por faixa de preço e/ou ano
        // JOIN: Veiculos INNER JOIN Fabricantes INNER JOIN Categorias
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Retorna veículos disponíveis filtrados por faixa de preço e/ou ano de fabricação.
        /// Utiliza INNER JOIN entre Veiculos, Fabricantes e Categorias.
        /// </summary>
        [HttpGet("veiculos-disponiveis")]
        [ProducesResponseType(typeof(IEnumerable<VeiculoDisponivelFiltroDto>), 200)]
        public async Task<ActionResult<IEnumerable<VeiculoDisponivelFiltroDto>>> GetVeiculosDisponiveis(
            [FromQuery] decimal? precoMin,
            [FromQuery] decimal? precoMax,
            [FromQuery] int? anoMin,
            [FromQuery] int? anoMax)
        {
            if (precoMin.HasValue && precoMax.HasValue && precoMin > precoMax)
                return BadRequest(new { mensagem = "precoMin não pode ser maior que precoMax." });

            if (anoMin.HasValue && anoMax.HasValue && anoMin > anoMax)
                return BadRequest(new { mensagem = "anoMin não pode ser maior que anoMax." });

            // INNER JOIN: Veiculos → Fabricantes → Categorias
            var query = from v in _context.Veiculos
                        join f in _context.Fabricantes on v.FabricanteId equals f.Id   // INNER JOIN
                        join c in _context.Categorias on v.CategoriaId equals c.Id     // INNER JOIN
                        where v.Disponivel == true
                        select new VeiculoDisponivelFiltroDto
                        {
                            Id = v.Id,
                            Modelo = v.Modelo,
                            Fabricante = f.Nome,
                            Categoria = c.Nome,
                            AnoFabricacao = v.AnoFabricacao,
                            ValorDiaria = v.ValorDiaria,
                            Quilometragem = v.Quilometragem
                        };

            if (precoMin.HasValue) query = query.Where(v => v.ValorDiaria >= precoMin.Value);
            if (precoMax.HasValue) query = query.Where(v => v.ValorDiaria <= precoMax.Value);
            if (anoMin.HasValue) query = query.Where(v => v.AnoFabricacao >= anoMin.Value);
            if (anoMax.HasValue) query = query.Where(v => v.AnoFabricacao <= anoMax.Value);

            var resultado = await query.OrderBy(v => v.ValorDiaria).ToListAsync();
            return Ok(resultado);
        }

        // ─────────────────────────────────────────────────────────────────────
        // FILTRO 5 — Relatório de aluguéis e receita agrupados por categoria
        // JOIN: Categorias LEFT JOIN Veiculos INNER JOIN Alugueis
        // ─────────────────────────────────────────────────────────────────────
        /// <summary>
        /// Relatório de receita e quantidade de aluguéis por categoria de veículo.
        /// Utiliza LEFT JOIN (Categorias→Veiculos) e INNER JOIN implícito (Veiculos→Alugueis).
        /// </summary>
        [HttpGet("relatorio-por-categoria")]
        [ProducesResponseType(typeof(IEnumerable<RelatorioAluguelPorCategoriaDto>), 200)]
        public async Task<ActionResult<IEnumerable<RelatorioAluguelPorCategoriaDto>>> GetRelatorioPorCategoria(
            [FromQuery] string statusAluguel = "Concluido")
        {
            if (!Enum.TryParse<StatusAluguel>(statusAluguel, true, out var statusEnum))
                return BadRequest(new { mensagem = "Status inválido. Use: Aberto, Concluido ou Cancelado." });

            // LEFT JOIN: Categorias → Veiculos → Alugueis
            // Categorias sem veículos/aluguéis ainda aparecem no resultado
            var resultado = await (
                from cat in _context.Categorias
                join v in _context.Veiculos on cat.Id equals v.CategoriaId into veiculos
                from vei in veiculos.DefaultIfEmpty()                                      // LEFT JOIN
                join a in _context.Alugueis.Where(x => x.Status == statusEnum)
                    on (vei != null ? vei.Id : -1) equals a.VeiculoId into alugs
                from al in alugs.DefaultIfEmpty()                                          // LEFT JOIN
                group al by cat.Nome into g
                select new RelatorioAluguelPorCategoriaDto
                {
                    Categoria = g.Key,
                    TotalAlugueis = g.Count(x => x != null),
                    ReceitaTotal = g.Sum(x => x != null ? x.ValorTotal : 0),
                    ValorDiariaMedia = g.Average(x => x != null ? x.ValorDiaria : 0)
                })
                .OrderByDescending(r => r.ReceitaTotal)
                .ToListAsync();

            return Ok(resultado);
        }
    }
}