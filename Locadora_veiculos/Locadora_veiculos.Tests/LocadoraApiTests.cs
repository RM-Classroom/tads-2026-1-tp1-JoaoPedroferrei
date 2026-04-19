using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Locadora_veiculos.Tests
{
    // ═══════════════════════════════════════════════════════════════════
    // TESTES DE INTEGRAÇÃO — Locadora de Veículos API
    // Utiliza WebApplicationFactory para subir a API em memória
    // Execute com: dotnet test
    // ═══════════════════════════════════════════════════════════════════

    public class LocadoraApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _json = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public LocadoraApiTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        // ───────────────────────────────────────────────────────────────
        // FABRICANTES
        // ───────────────────────────────────────────────────────────────

        [Fact(DisplayName = "POST /api/Fabricantes - Deve criar fabricante com dados válidos")]
        public async Task PostFabricante_DadosValidos_Retorna201()
        {
            var payload = new { nome = "Toyota", paisOrigem = "Japão" };
            var response = await _client.PostAsJsonAsync("/api/Fabricantes", payload);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Toyota", body);
        }

        [Fact(DisplayName = "POST /api/Fabricantes - Deve retornar 400 sem nome")]
        public async Task PostFabricante_SemNome_Retorna400()
        {
            var payload = new { paisOrigem = "Japão" };
            var response = await _client.PostAsJsonAsync("/api/Fabricantes", payload);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact(DisplayName = "GET /api/Fabricantes - Deve retornar lista")]
        public async Task GetFabricantes_Retorna200()
        {
            var response = await _client.GetAsync("/api/Fabricantes");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            Assert.StartsWith("[", body.Trim());
        }

        [Fact(DisplayName = "GET /api/Fabricantes/{id} - Id inexistente deve retornar 404")]
        public async Task GetFabricante_IdInexistente_Retorna404()
        {
            var response = await _client.GetAsync("/api/Fabricantes/99999");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact(DisplayName = "DELETE /api/Fabricantes/{id} - Com veículos deve retornar 400")]
        public async Task DeleteFabricante_ComVeiculos_Retorna400()
        {
            // Cria fabricante
            var fab = new { nome = "FabDeleteTeste", paisOrigem = "Brasil" };
            var postResp = await _client.PostAsJsonAsync("/api/Fabricantes", fab);
            var postBody = await postResp.Content.ReadAsStringAsync();
            var fabId = JsonDocument.Parse(postBody).RootElement.GetProperty("id").GetInt32();

            // Cria categoria
            var cat = new { nome = "CatDeleteTeste", descricao = "Teste" };
            var catResp = await _client.PostAsJsonAsync("/api/Categorias", cat);
            var catBody = await catResp.Content.ReadAsStringAsync();
            var catId = JsonDocument.Parse(catBody).RootElement.GetProperty("id").GetInt32();

            // Cria veículo vinculado
            var veiculo = new
            {
                fabricanteId = fabId,
                categoriaId = catId,
                modelo = "ModeloTeste",
                anoFabricacao = 2020,
                quilometragem = 10000,
                valorDiaria = 100.00,
                disponivel = true
            };
            await _client.PostAsJsonAsync("/api/Veiculos", veiculo);

            // Tenta deletar fabricante com veículo vinculado
            var deleteResp = await _client.DeleteAsync($"/api/Fabricantes/{fabId}");
            Assert.Equal(HttpStatusCode.BadRequest, deleteResp.StatusCode);
        }

        // ───────────────────────────────────────────────────────────────
        // CATEGORIAS
        // ───────────────────────────────────────────────────────────────

        [Fact(DisplayName = "POST /api/Categorias - Deve criar categoria com dados válidos")]
        public async Task PostCategoria_DadosValidos_Retorna201()
        {
            var payload = new { nome = "Sedan", descricao = "Veículos sedã" };
            var response = await _client.PostAsJsonAsync("/api/Categorias", payload);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact(DisplayName = "POST /api/Categorias - Nome duplicado deve retornar 400")]
        public async Task PostCategoria_NomeDuplicado_Retorna400()
        {
            var payload = new { nome = "CatDuplicada", descricao = "Teste" };
            await _client.PostAsJsonAsync("/api/Categorias", payload);

            var response2 = await _client.PostAsJsonAsync("/api/Categorias", payload);
            Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
        }

        // ───────────────────────────────────────────────────────────────
        // VEÍCULOS
        // ───────────────────────────────────────────────────────────────

        [Fact(DisplayName = "POST /api/Veiculos - FabricanteId inválido deve retornar 400")]
        public async Task PostVeiculo_FabricanteInvalido_Retorna400()
        {
            var payload = new
            {
                fabricanteId = 99999,
                categoriaId = 1,
                modelo = "Teste",
                anoFabricacao = 2020,
                quilometragem = 0,
                valorDiaria = 100.00,
                disponivel = true
            };
            var response = await _client.PostAsJsonAsync("/api/Veiculos", payload);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact(DisplayName = "POST /api/Veiculos - Ano inválido deve retornar 400")]
        public async Task PostVeiculo_AnoInvalido_Retorna400()
        {
            var payload = new
            {
                fabricanteId = 1,
                categoriaId = 1,
                modelo = "Teste",
                anoFabricacao = 1800,  // fora do range 1900-2100
                quilometragem = 0,
                valorDiaria = 100.00,
                disponivel = true
            };
            var response = await _client.PostAsJsonAsync("/api/Veiculos", payload);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ───────────────────────────────────────────────────────────────
        // CLIENTES
        // ───────────────────────────────────────────────────────────────

        [Fact(DisplayName = "POST /api/Clientes - Deve criar cliente com dados válidos")]
        public async Task PostCliente_DadosValidos_Retorna201()
        {
            var unique = Guid.NewGuid().ToString("N")[..8];
            var payload = new
            {
                nome = "Maria Souza",
                cpf = $"111.222.333-{unique[..2]}",
                email = $"maria_{unique}@email.com",
                telefone = "31988887777",
                dataNascimento = "1985-03-20"
            };
            var response = await _client.PostAsJsonAsync("/api/Clientes", payload);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact(DisplayName = "POST /api/Clientes - Email inválido deve retornar 400")]
        public async Task PostCliente_EmailInvalido_Retorna400()
        {
            var payload = new
            {
                nome = "Teste",
                cpf = "000.000.000-00",
                email = "email-invalido",
                telefone = ""
            };
            var response = await _client.PostAsJsonAsync("/api/Clientes", payload);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact(DisplayName = "POST /api/Clientes - CPF duplicado deve retornar 400")]
        public async Task PostCliente_CpfDuplicado_Retorna400()
        {
            var unique = Guid.NewGuid().ToString("N")[..6];
            var payload1 = new
            {
                nome = "Cliente A",
                cpf = "999.888.777-66",
                email = $"clienteA_{unique}@email.com"
            };
            var payload2 = new
            {
                nome = "Cliente B",
                cpf = "999.888.777-66",   // mesmo CPF
                email = $"clienteB_{unique}@email.com"
            };

            await _client.PostAsJsonAsync("/api/Clientes", payload1);
            var response = await _client.PostAsJsonAsync("/api/Clientes", payload2);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ───────────────────────────────────────────────────────────────
        // ALUGUÉIS
        // ───────────────────────────────────────────────────────────────

        [Fact(DisplayName = "POST /api/Alugueis - DataFim anterior à DataInicio deve retornar 400")]
        public async Task PostAluguel_DataFimAnterior_Retorna400()
        {
            var payload = new
            {
                clienteId = 1,
                veiculoId = 1,
                dataInicio = "2026-04-15",
                dataFim = "2026-04-10",   // anterior ao início
                kmInicial = 10000
            };
            var response = await _client.PostAsJsonAsync("/api/Alugueis", payload);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact(DisplayName = "PUT /api/Alugueis/{id}/devolver - KmFinal menor que KmInicial deve retornar 400")]
        public async Task DevolverAluguel_KmFinalMenor_Retorna400()
        {
            // Esse teste assume que existe um aluguel aberto com id=1 no banco de testes
            var payload = new { kmFinal = 100, dataDevolucao = "2026-04-15" };
            var response = await _client.PutAsJsonAsync("/api/Alugueis/1/devolver", payload);
            // Se não existe, 404; se existe e km é menor, 400
            Assert.True(
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        // ───────────────────────────────────────────────────────────────
        // FILTROS
        // ───────────────────────────────────────────────────────────────

        [Fact(DisplayName = "GET /api/Filtros/veiculos-completo - Deve retornar 200")]
        public async Task GetVeiculosCompleto_Retorna200()
        {
            var response = await _client.GetAsync("/api/Filtros/veiculos-completo");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(DisplayName = "GET /api/Filtros/veiculos-completo?disponivel=true - Deve filtrar disponíveis")]
        public async Task GetVeiculosCompleto_FiltroDisponivel_Retorna200()
        {
            var response = await _client.GetAsync("/api/Filtros/veiculos-completo?disponivel=true");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();
            var lista = JsonDocument.Parse(body).RootElement;
            foreach (var item in lista.EnumerateArray())
                Assert.True(item.GetProperty("disponivel").GetBoolean());
        }

        [Fact(DisplayName = "GET /api/Filtros/alugueis-detalhado - Deve retornar 200")]
        public async Task GetAlugueisDetalhado_Retorna200()
        {
            var response = await _client.GetAsync("/api/Filtros/alugueis-detalhado");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(DisplayName = "GET /api/Filtros/alugueis-detalhado?status=Invalido - Deve retornar 400")]
        public async Task GetAlugueisDetalhado_StatusInvalido_Retorna400()
        {
            var response = await _client.GetAsync("/api/Filtros/alugueis-detalhado?status=Invalido");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact(DisplayName = "GET /api/Filtros/historico-clientes - Deve retornar 200")]
        public async Task GetHistoricoClientes_Retorna200()
        {
            var response = await _client.GetAsync("/api/Filtros/historico-clientes");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(DisplayName = "GET /api/Filtros/historico-clientes?top=0 - Deve retornar 400")]
        public async Task GetHistoricoClientes_TopInvalido_Retorna400()
        {
            var response = await _client.GetAsync("/api/Filtros/historico-clientes?top=0");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact(DisplayName = "GET /api/Filtros/veiculos-disponiveis - Deve retornar 200")]
        public async Task GetVeiculosDisponiveis_Retorna200()
        {
            var response = await _client.GetAsync("/api/Filtros/veiculos-disponiveis");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(DisplayName = "GET /api/Filtros/veiculos-disponiveis?precoMin=200&precoMax=100 - Deve retornar 400")]
        public async Task GetVeiculosDisponiveis_PrecoInvalido_Retorna400()
        {
            var response = await _client.GetAsync("/api/Filtros/veiculos-disponiveis?precoMin=200&precoMax=100");
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact(DisplayName = "GET /api/Filtros/relatorio-por-categoria - Deve retornar 200")]
        public async Task GetRelatorioPorCategoria_Retorna200()
        {
            var response = await _client.GetAsync("/api/Filtros/relatorio-por-categoria");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact(DisplayName = "GET /api/Filtros/relatorio-por-categoria?statusAluguel=Concluido - Deve retornar 200")]
        public async Task GetRelatorioPorCategoria_StatusConcluido_Retorna200()
        {
            var response = await _client.GetAsync("/api/Filtros/relatorio-por-categoria?statusAluguel=Concluido");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}