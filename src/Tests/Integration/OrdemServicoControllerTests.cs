using Application.Cadastros.DTO;
using Application.Estoque.DTO;
using Application.OrdemServico.DTO;
using Domain.Cadastros.Enums;
using Domain.Estoque.Enums;
using Domain.OrdemServico.Enums;
using FluentAssertions;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Tests.Integration
{
    public class OrdemServicoControllerTests : IClassFixture<TestWebApplicationFactory<Program>>
    {
        private readonly TestWebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public OrdemServicoControllerTests(TestWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        #region Método Get Tests

        [Fact(DisplayName = "GET /api/ordens-servico deve retornar status 200 com lista vazia")]
        [Trait("Method", "Get")]
        public async Task Get_SemOrdensServico_DeveRetornarStatus200ComListaVazia()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Garantir que o banco de dados esteja limpo para este teste
            context.OrdensServico.RemoveRange(context.OrdensServico);
            await context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/ordens-servico");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var ordensServico = await response.Content.ReadFromJsonAsync<List<RetornoOrdemServicoCompletaDTO>>();
            ordensServico.Should().NotBeNull();
            ordensServico.Should().BeEmpty();
        }

        [Fact(DisplayName = "GET /api/ordens-servico deve retornar status 200 com lista de ordens")]
        [Trait("Method", "Get")]
        public async Task Get_ComOrdensServico_DeveRetornarStatus200ComLista()
        {
            // Arrange
            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Get Test",
                DocumentoIdentificador = "33604492076"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "GET1234",
                Modelo = "Civic Get",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2020,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            var createDto = new CriarOrdemServicoDTO
            {
                VeiculoId = veiculo!.Id
            };

            var createResponse = await _client.PostAsJsonAsync("/api/ordens-servico", createDto);
            createResponse.EnsureSuccessStatusCode();

            // Act
            var response = await _client.GetAsync("/api/ordens-servico");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var ordensServico = await response.Content.ReadFromJsonAsync<List<RetornoOrdemServicoCompletaDTO>>();
            ordensServico.Should().NotBeNull();
            ordensServico.Should().HaveCountGreaterThan(1);
            ordensServico!.Where(os => os.VeiculoId == veiculo.Id).Should().HaveCount(1);
        }

        #endregion

        #region Método GetById Tests

        [Fact(DisplayName = "GET /api/ordens-servico/{id} deve retornar status 200 quando ordem existir")]
        [Trait("Method", "GetById")]
        public async Task GetById_ComOrdemServicoExistente_DeveRetornarStatus200()
        {
            // Arrange
            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente GetById Test",
                DocumentoIdentificador = "47730216086"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "GID1234",
                Modelo = "Civic GetById",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2021,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            var createDto = new CriarOrdemServicoDTO
            {
                VeiculoId = veiculo!.Id
            };

            var createResponse = await _client.PostAsJsonAsync("/api/ordens-servico", createDto);
            createResponse.EnsureSuccessStatusCode();
            
            var createdOrdem = await createResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();
            createdOrdem.Should().NotBeNull();

            // Act
            var response = await _client.GetAsync($"/api/ordens-servico/{createdOrdem!.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var ordemServico = await response.Content.ReadFromJsonAsync<RetornoOrdemServicoCompletaDTO>();
            ordemServico.Should().NotBeNull();
            ordemServico!.Id.Should().Be(createdOrdem.Id);
            ordemServico.VeiculoId.Should().Be(veiculo.Id);
            ordemServico.Codigo.Should().Be(createdOrdem.Codigo);
        }

        [Fact(DisplayName = "GET /api/ordens-servico/{id} deve retornar status 404 quando ordem não existir")]
        [Trait("Method", "GetById")]
        public async Task GetById_ComOrdemServicoInexistente_DeveRetornarStatus404()
        {
            // Arrange
            var idInexistente = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/ordens-servico/{idInexistente}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region Método GetByCodigo Tests

        [Fact(DisplayName = "GET /api/ordens-servico/codigo/{codigo} deve retornar status 200 quando código existir")]
        [Trait("Method", "GetByCodigo")]
        public async Task GetByCodigo_ComCodigoExistente_DeveRetornarStatus200()
        {
            // Arrange
            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente GetByCodigo Test",
                DocumentoIdentificador = "02593875097"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "COD1234",
                Modelo = "Civic Codigo",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2022,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            var createDto = new CriarOrdemServicoDTO
            {
                VeiculoId = veiculo!.Id
            };

            var createResponse = await _client.PostAsJsonAsync("/api/ordens-servico", createDto);
            createResponse.EnsureSuccessStatusCode();
            
            var createdOrdem = await createResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();
            createdOrdem.Should().NotBeNull();

            // Act
            var response = await _client.GetAsync($"/api/ordens-servico/codigo/{createdOrdem!.Codigo}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var ordemServico = await response.Content.ReadFromJsonAsync<RetornoOrdemServicoCompletaDTO>();
            ordemServico.Should().NotBeNull();
            ordemServico!.Id.Should().Be(createdOrdem.Id);
            ordemServico.Codigo.Should().Be(createdOrdem.Codigo);
            ordemServico.VeiculoId.Should().Be(veiculo.Id);
        }

        [Fact(DisplayName = "GET /api/ordens-servico/codigo/{codigo} deve retornar status 404 quando código não existir")]
        [Trait("Method", "GetByCodigo")]
        public async Task GetByCodigo_ComCodigoInexistente_DeveRetornarStatus404()
        {
            // Arrange
            var codigoInexistente = "OS9999999";

            // Act
            var response = await _client.GetAsync($"/api/ordens-servico/codigo/{codigoInexistente}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region Método Post Tests

        [Fact(DisplayName = "POST /api/ordens-servico deve retornar status 201 com dados válidos")]
        [Trait("Method", "Post")]
        public async Task Post_ComDadosValidos_DeveRetornarStatus201()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Post Test",
                DocumentoIdentificador = "46809315071"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "PST1234",
                Modelo = "Civic Post",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            var createDto = new CriarOrdemServicoDTO
            {
                VeiculoId = veiculo!.Id
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/ordens-servico", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var createdOrdem = await response.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();
            createdOrdem.Should().NotBeNull();
            createdOrdem!.VeiculoId.Should().Be(veiculo.Id);
            createdOrdem.Codigo.Should().NotBeNullOrWhiteSpace();
            createdOrdem.Status.Should().Be("Recebida");

            // Verificar se a ordem criada existe no banco de dados
            var ordemNoDB = await context.OrdensServico
                .FirstOrDefaultAsync(o => o.Id == createdOrdem.Id);
            ordemNoDB.Should().NotBeNull();
            ordemNoDB!.VeiculoId.Should().Be(veiculo.Id);

            // Verificar cabeçalho Location
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location!.ToString().Should().Contain($"/api/ordens-servico/{createdOrdem.Id}");
        }

        [Fact(DisplayName = "POST /api/ordens-servico deve retornar status 422 com veículo inexistente")]
        [Trait("Method", "Post")]
        public async Task Post_ComVeiculoInexistente_DeveRetornarStatus422()
        {
            // Arrange
            var createDto = new CriarOrdemServicoDTO
            {
                VeiculoId = Guid.NewGuid() // Veículo que não existe
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/ordens-servico", createDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [Fact(DisplayName = "POST /api/ordens-servico deve criar ordem com código único")]
        [Trait("Method", "Post")]
        public async Task Post_CriarMultiplasOrdens_DeveTerCodigosUnicos()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Multi Test",
                DocumentoIdentificador = "59918722010"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "MLT1234",
                Modelo = "Civic Multi",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2024,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            var createDto1 = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var createDto2 = new CriarOrdemServicoDTO { VeiculoId = veiculo.Id };

            // Act
            var response1 = await _client.PostAsJsonAsync("/api/ordens-servico", createDto1);
            var response2 = await _client.PostAsJsonAsync("/api/ordens-servico", createDto2);

            // Assert
            response1.StatusCode.Should().Be(HttpStatusCode.Created);
            response2.StatusCode.Should().Be(HttpStatusCode.Created);

            var ordem1 = await response1.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();
            var ordem2 = await response2.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            ordem1.Should().NotBeNull();
            ordem2.Should().NotBeNull();
            ordem1!.Codigo.Should().NotBe(ordem2!.Codigo);
            ordem1.Id.Should().NotBe(ordem2.Id);

            // Verificar se ambas as ordens existem no banco de dados
            var ordensNoDB = await context.OrdensServico
                .Where(o => o.Id == ordem1.Id || o.Id == ordem2.Id)
                .ToListAsync();
            ordensNoDB.Should().HaveCount(2);
        }

        #endregion

        #region Método AdicionarServicos Tests

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/servicos deve retornar status 200 com dados válidos")]
        [Trait("Method", "AdicionarServicos")]
        public async Task AdicionarServicos_ComDadosValidos_DeveRetornarStatus200()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente AddServicos Test",
                DocumentoIdentificador = "90216912059"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "SRV1234",
                Modelo = "Civic Servicos",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar serviço de teste
            var servicoDto = new CriarServicoDTO
            {
                Nome = "Troca de Óleo Teste",
                Preco = 80.00m
            };

            var servicoResponse = await _client.PostAsJsonAsync("/api/cadastros/servicos", servicoDto);
            servicoResponse.EnsureSuccessStatusCode();
            var servico = await servicoResponse.Content.ReadFromJsonAsync<RetornoServicoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Iniciar diagnóstico para permitir adicionar serviços
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarServicosDto = new AdicionarServicosDTO
            {
                ServicosOriginaisIds = new List<Guid> { servico!.Id }
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", adicionarServicosDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var resultado = await response.Content.ReadFromJsonAsync<RetornoOrdemServicoComServicosItensDTO>();
            resultado.Should().NotBeNull();
            resultado!.ServicosIncluidos.Should().HaveCount(1);
            resultado.ServicosIncluidos.First().ServicoOriginalId.Should().Be(servico.Id);

            // Verificar se o serviço foi persistido no banco de dados
            var ordemNoDB = await context.OrdensServico
                .Include(o => o.ServicosIncluidos)
                .FirstOrDefaultAsync(o => o.Id == ordem.Id);
            ordemNoDB.Should().NotBeNull();
            ordemNoDB!.ServicosIncluidos.Should().HaveCount(1);
            ordemNoDB.ServicosIncluidos.First().ServicoOriginalId.Should().Be(servico.Id);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/servicos deve retornar status 404 quando ordem não existir")]
        [Trait("Method", "AdicionarServicos")]
        public async Task AdicionarServicos_ComOrdemInexistente_DeveRetornarStatus404()
        {
            // Arrange
            var ordemIdInexistente = Guid.NewGuid();
            var adicionarServicosDto = new AdicionarServicosDTO
            {
                ServicosOriginaisIds = new List<Guid> { Guid.NewGuid() }
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/ordens-servico/{ordemIdInexistente}/servicos", adicionarServicosDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/servicos deve retornar status 422 quando serviço não existir")]
        [Trait("Method", "AdicionarServicos")]
        public async Task AdicionarServicos_ComServicoInexistente_DeveRetornarStatus422()
        {
            // Arrange
            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Servico Inexistente",
                DocumentoIdentificador = "91351688030"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "SIN1234",
                Modelo = "Civic Sin",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Iniciar diagnóstico
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarServicosDto = new AdicionarServicosDTO
            {
                ServicosOriginaisIds = new List<Guid> { Guid.NewGuid() } // Serviço inexistente
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", adicionarServicosDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        #endregion

        #region Método AdicionarItem Tests

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/itens deve retornar status 200 com dados válidos")]
        [Trait("Method", "AdicionarItem")]
        public async Task AdicionarItem_ComDadosValidos_DeveRetornarStatus200()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente AddItem Test",
                DocumentoIdentificador = "92875848003"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "ITM1234",
                Modelo = "Civic Item",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar item de estoque de teste
            var itemEstoqueDto = new CriarItemEstoqueDTO
            {
                Nome = "Filtro de Óleo Item Test",
                Quantidade = 100,
                TipoItemEstoque = TipoItemEstoqueEnum.Peca,
                Preco = 25.00m
            };

            var itemResponse = await _client.PostAsJsonAsync("/api/estoque/itens", itemEstoqueDto);
            itemResponse.EnsureSuccessStatusCode();
            var itemEstoque = await itemResponse.Content.ReadFromJsonAsync<RetornoItemEstoqueDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Iniciar diagnóstico para permitir adicionar itens
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarItemDto = new AdicionarItemDTO
            {
                ItemEstoqueOriginalId = itemEstoque!.Id,
                Quantidade = 2
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/itens", adicionarItemDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var resultado = await response.Content.ReadFromJsonAsync<RetornoOrdemServicoComServicosItensDTO>();
            resultado.Should().NotBeNull();
            resultado!.ItensIncluidos.Should().HaveCount(1);
            resultado.ItensIncluidos.First().ItemEstoqueOriginalId.Should().Be(itemEstoque.Id);
            resultado.ItensIncluidos.First().Quantidade.Should().Be(2);

            // Verificar se o item foi persistido no banco de dados
            var ordemNoDB = await context.OrdensServico
                .Include(o => o.ItensIncluidos)
                .FirstOrDefaultAsync(o => o.Id == ordem.Id);
            ordemNoDB.Should().NotBeNull();
            ordemNoDB!.ItensIncluidos.Should().HaveCount(1);
            ordemNoDB.ItensIncluidos.First().ItemEstoqueOriginalId.Should().Be(itemEstoque.Id);
            ordemNoDB.ItensIncluidos.First().Quantidade.Valor.Should().Be(2);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/itens deve incrementar quantidade quando item já existe")]
        [Trait("Method", "AdicionarItem")]
        public async Task AdicionarItem_ComItemJaExistente_DeveIncrementarQuantidade()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Increment Test",
                DocumentoIdentificador = "63546196031"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "INC1234",
                Modelo = "Civic Increment",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar item de estoque de teste
            var itemEstoqueDto = new CriarItemEstoqueDTO
            {
                Nome = "Filtro Increment Test",
                Quantidade = 100,
                TipoItemEstoque = TipoItemEstoqueEnum.Peca,
                Preco = 30.00m
            };

            var itemResponse = await _client.PostAsJsonAsync("/api/estoque/itens", itemEstoqueDto);
            itemResponse.EnsureSuccessStatusCode();
            var itemEstoque = await itemResponse.Content.ReadFromJsonAsync<RetornoItemEstoqueDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Iniciar diagnóstico
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarItemDto = new AdicionarItemDTO
            {
                ItemEstoqueOriginalId = itemEstoque!.Id,
                Quantidade = 2
            };

            // Adicionar item pela primeira vez
            var firstResponse = await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/itens", adicionarItemDto);
            firstResponse.EnsureSuccessStatusCode();

            // Act - Adicionar o mesmo item novamente
            var secondResponse = await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/itens", adicionarItemDto);

            // Assert
            secondResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var resultado = await secondResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoComServicosItensDTO>();
            resultado.Should().NotBeNull();
            resultado!.ItensIncluidos.Should().HaveCount(1); // Deve ter apenas 1 item, não 2
            resultado.ItensIncluidos.First().ItemEstoqueOriginalId.Should().Be(itemEstoque.Id);
            resultado.ItensIncluidos.First().Quantidade.Should().Be(4); // 2 + 2 = 4

            // Verificar se o incremento foi persistido no banco de dados
            var ordemNoDB = await context.OrdensServico
                .Include(o => o.ItensIncluidos)
                .FirstOrDefaultAsync(o => o.Id == ordem.Id);
            ordemNoDB.Should().NotBeNull();
            ordemNoDB!.ItensIncluidos.Should().HaveCount(1); // Apenas 1 item
            ordemNoDB.ItensIncluidos.First().Quantidade.Valor.Should().Be(4); // Quantidade incrementada
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/itens deve retornar status 404 quando ordem não existir")]
        [Trait("Method", "AdicionarItem")]
        public async Task AdicionarItem_ComOrdemInexistente_DeveRetornarStatus404()
        {
            // Arrange
            var ordemIdInexistente = Guid.NewGuid();
            var adicionarItemDto = new AdicionarItemDTO
            {
                ItemEstoqueOriginalId = Guid.NewGuid(),
                Quantidade = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/ordens-servico/{ordemIdInexistente}/itens", adicionarItemDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/itens deve retornar status 422 quando item não existir")]
        [Trait("Method", "AdicionarItem")]
        public async Task AdicionarItem_ComItemInexistente_DeveRetornarStatus422()
        {
            // Arrange
            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Item Inexistente",
                DocumentoIdentificador = "53154657053"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "IIN1234",
                Modelo = "Civic Item Inexistente",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Iniciar diagnóstico
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarItemDto = new AdicionarItemDTO
            {
                ItemEstoqueOriginalId = Guid.NewGuid(), // Item inexistente
                Quantidade = 1
            };

            // Act
            var response = await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/itens", adicionarItemDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        #endregion

        #region Método RemoverServico Tests

        [Fact(DisplayName = "DELETE /api/ordens-servico/{id}/servicos/{servicoId} deve retornar status 204 quando serviço existir")]
        [Trait("Method", "RemoverServico")]
        public async Task RemoverServico_ComServicoExistente_DeveRetornarStatus204()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Remove Servico",
                DocumentoIdentificador = "68297761045"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "RMS1234",
                Modelo = "Civic Remove Servico",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar serviço de teste
            var servicoDto = new CriarServicoDTO
            {
                Nome = "Serviço Remove Test",
                Preco = 90.00m
            };

            var servicoResponse = await _client.PostAsJsonAsync("/api/cadastros/servicos", servicoDto);
            servicoResponse.EnsureSuccessStatusCode();
            var servico = await servicoResponse.Content.ReadFromJsonAsync<RetornoServicoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Iniciar diagnóstico e adicionar serviço
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarServicosDto = new AdicionarServicosDTO
            {
                ServicosOriginaisIds = new List<Guid> { servico!.Id }
            };

            var addResponse = await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", adicionarServicosDto);
            addResponse.EnsureSuccessStatusCode();
            var ordemComServico = await addResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoComServicosItensDTO>();
            var servicoIncluidoId = ordemComServico!.ServicosIncluidos.First().Id;

            // Act
            var response = await _client.DeleteAsync($"/api/ordens-servico/{ordem.Id}/servicos/{servicoIncluidoId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verificar se o serviço foi removido do banco de dados
            var ordemNoDB = await context.OrdensServico
                .Include(o => o.ServicosIncluidos)
                .FirstOrDefaultAsync(o => o.Id == ordem.Id);
            ordemNoDB.Should().NotBeNull();
            ordemNoDB!.ServicosIncluidos.Should().BeEmpty();
        }

        [Fact(DisplayName = "DELETE /api/ordens-servico/{id}/servicos/{servicoId} deve retornar status 404 quando ordem não existir")]
        [Trait("Method", "RemoverServico")]
        public async Task RemoverServico_ComOrdemInexistente_DeveRetornarStatus404()
        {
            // Arrange
            var ordemIdInexistente = Guid.NewGuid();
            var servicoIdInexistente = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/ordens-servico/{ordemIdInexistente}/servicos/{servicoIdInexistente}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "DELETE /api/ordens-servico/{id}/servicos/{servicoId} deve retornar status 422 quando ordem em status inválido")]
        [Trait("Method", "RemoverServico")]
        public async Task RemoverServico_ComStatusInvalido_DeveRetornarStatus422()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Status Inválido",
                DocumentoIdentificador = "08252118089"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "STS1234",
                Modelo = "Civic Status",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar serviço de teste
            var servicoDto = new CriarServicoDTO
            {
                Nome = "Serviço Status Test",
                Preco = 100.00m
            };

            var servicoResponse = await _client.PostAsJsonAsync("/api/cadastros/servicos", servicoDto);
            servicoResponse.EnsureSuccessStatusCode();
            var servico = await servicoResponse.Content.ReadFromJsonAsync<RetornoServicoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Iniciar diagnóstico e adicionar serviço
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarServicosDto = new AdicionarServicosDTO
            {
                ServicosOriginaisIds = new List<Guid> { servico!.Id }
            };

            var addResponse = await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", adicionarServicosDto);
            addResponse.EnsureSuccessStatusCode();
            var ordemComServico = await addResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoComServicosItensDTO>();
            var servicoIncluidoId = ordemComServico!.ServicosIncluidos.First().Id;

            // Gerar orçamento para mudar o status para AguardandoAprovacao
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento", null);

            // Act - Tentar remover serviço com status inválido (AguardandoAprovacao)
            var response = await _client.DeleteAsync($"/api/ordens-servico/{ordem.Id}/servicos/{servicoIncluidoId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        #endregion

        #region Método RemoverItem Tests

        [Fact(DisplayName = "DELETE /api/ordens-servico/{id}/itens/{itemId} deve retornar status 204 quando item existir")]
        [Trait("Method", "RemoverItem")]
        public async Task RemoverItem_ComItemExistente_DeveRetornarStatus204()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Remove Item",
                DocumentoIdentificador = "66466232018"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "RMI1234",
                Modelo = "Civic Remove Item",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar item de estoque de teste
            var itemEstoqueDto = new CriarItemEstoqueDTO
            {
                Nome = "Item Remove Test",
                Quantidade = 100,
                TipoItemEstoque = TipoItemEstoqueEnum.Peca,
                Preco = 35.00m
            };

            var itemResponse = await _client.PostAsJsonAsync("/api/estoque/itens", itemEstoqueDto);
            itemResponse.EnsureSuccessStatusCode();
            var itemEstoque = await itemResponse.Content.ReadFromJsonAsync<RetornoItemEstoqueDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Iniciar diagnóstico e adicionar item
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarItemDto = new AdicionarItemDTO
            {
                ItemEstoqueOriginalId = itemEstoque!.Id,
                Quantidade = 1
            };

            var addResponse = await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/itens", adicionarItemDto);
            addResponse.EnsureSuccessStatusCode();
            var ordemComItem = await addResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoComServicosItensDTO>();
            var itemIncluidoId = ordemComItem!.ItensIncluidos.First().Id;

            // Act
            var response = await _client.DeleteAsync($"/api/ordens-servico/{ordem.Id}/itens/{itemIncluidoId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verificar se o item foi removido do banco de dados
            var ordemNoDB = await context.OrdensServico
                .Include(o => o.ItensIncluidos)
                .FirstOrDefaultAsync(o => o.Id == ordem.Id);
            ordemNoDB.Should().NotBeNull();
            ordemNoDB!.ItensIncluidos.Should().BeEmpty();
        }

        [Fact(DisplayName = "DELETE /api/ordens-servico/{id}/itens/{itemId} deve retornar status 404 quando ordem não existir")]
        [Trait("Method", "RemoverItem")]
        public async Task RemoverItem_ComOrdemInexistente_DeveRetornarStatus404()
        {
            // Arrange
            var ordemIdInexistente = Guid.NewGuid();
            var itemIdInexistente = Guid.NewGuid();

            // Act
            var response = await _client.DeleteAsync($"/api/ordens-servico/{ordemIdInexistente}/itens/{itemIdInexistente}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "DELETE /api/ordens-servico/{id}/itens/{itemId} deve retornar status 422 quando ordem em status inválido")]
        [Trait("Method", "RemoverItem")]
        public async Task RemoverItem_ComStatusInvalido_DeveRetornarStatus422()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Item Status Inválido",
                DocumentoIdentificador = "07564613084"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "IST1234",
                Modelo = "Civic Item Status",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar item de estoque de teste
            var itemEstoqueDto = new CriarItemEstoqueDTO
            {
                Nome = "Item Status Test",
                Quantidade = 100,
                TipoItemEstoque = TipoItemEstoqueEnum.Peca,
                Preco = 40.00m
            };

            var itemResponse = await _client.PostAsJsonAsync("/api/estoque/itens", itemEstoqueDto);
            itemResponse.EnsureSuccessStatusCode();
            var itemEstoque = await itemResponse.Content.ReadFromJsonAsync<RetornoItemEstoqueDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Iniciar diagnóstico e adicionar item
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarItemDto = new AdicionarItemDTO
            {
                ItemEstoqueOriginalId = itemEstoque!.Id,
                Quantidade = 1
            };

            var addResponse = await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/itens", adicionarItemDto);
            addResponse.EnsureSuccessStatusCode();
            var ordemComItem = await addResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoComServicosItensDTO>();
            var itemIncluidoId = ordemComItem!.ItensIncluidos.First().Id;

            // Gerar orçamento para mudar o status para AguardandoAprovacao
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento", null);

            // Act - Tentar remover item com status inválido (AguardandoAprovacao)
            var response = await _client.DeleteAsync($"/api/ordens-servico/{ordem.Id}/itens/{itemIncluidoId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        #endregion

        #region Método Cancelar Tests

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/cancelar deve retornar status 204 com dados válidos")]
        [Trait("Method", "Cancelar")]
        public async Task Cancelar_ComDadosValidos_DeveRetornarStatus204()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Cancelar Test",
                DocumentoIdentificador = "97212050016"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "CAN1234",
                Modelo = "Civic Cancelar",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Act
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/cancelar", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verificar se o status foi atualizado no banco de dados
            var ordemNoDB = await context.OrdensServico.FirstOrDefaultAsync(o => o.Id == ordem.Id);
            ordemNoDB.Should().NotBeNull();
            ordemNoDB!.Status.Valor.Should().Be(StatusOrdemServicoEnum.Cancelada);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/cancelar deve retornar status 404 quando ordem não existir")]
        [Trait("Method", "Cancelar")]
        public async Task Cancelar_ComOrdemInexistente_DeveRetornarStatus404()
        {
            // Arrange
            var ordemIdInexistente = Guid.NewGuid();

            // Act
            var response = await _client.PostAsync($"/api/ordens-servico/{ordemIdInexistente}/cancelar", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region Método IniciarDiagnostico Tests

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/iniciar-diagnostico deve retornar status 204 com dados válidos")]
        [Trait("Method", "IniciarDiagnostico")]
        public async Task IniciarDiagnostico_ComDadosValidos_DeveRetornarStatus204()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Diagnostico Test",
                DocumentoIdentificador = "61810280052"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "DIA1234",
                Modelo = "Civic Diagnostico",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Act
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verificar se o status foi atualizado no banco de dados
            var ordemNoDB = await context.OrdensServico.FirstOrDefaultAsync(o => o.Id == ordem.Id);
            ordemNoDB.Should().NotBeNull();
            ordemNoDB!.Status.Valor.Should().Be(StatusOrdemServicoEnum.EmDiagnostico);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/iniciar-diagnostico deve retornar status 404 quando ordem não existir")]
        [Trait("Method", "IniciarDiagnostico")]
        public async Task IniciarDiagnostico_ComOrdemInexistente_DeveRetornarStatus404()
        {
            // Arrange
            var ordemIdInexistente = Guid.NewGuid();

            // Act
            var response = await _client.PostAsync($"/api/ordens-servico/{ordemIdInexistente}/iniciar-diagnostico", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/iniciar-diagnostico deve retornar status 422 quando ordem em status inválido")]
        [Trait("Method", "IniciarDiagnostico")]
        public async Task IniciarDiagnostico_ComStatusInvalido_DeveRetornarStatus422()
        {
            // Arrange
            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Diagnostico Status Inválido",
                DocumentoIdentificador = "45574376059"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "DSI1235",
                Modelo = "Civic Diag Status Inválido",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Cancelar a ordem para deixá-la em status inválido
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/cancelar", null);

            // Act - Tentar iniciar diagnóstico em ordem cancelada
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/iniciar-diagnostico", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        #endregion

        #region Método GerarOrcamento Tests

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/orcamento deve retornar status 201 com dados válidos")]
        [Trait("Method", "GerarOrcamento")]
        public async Task GerarOrcamento_ComDadosValidos_DeveRetornarStatus201()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Orçamento Test",
                DocumentoIdentificador = "00995435081"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "ORC1234",
                Modelo = "Civic Orçamento",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar serviço de teste
            var servicoDto = new CriarServicoDTO
            {
                Nome = "Serviço Orçamento Test",
                Preco = 120.00m
            };

            var servicoResponse = await _client.PostAsJsonAsync("/api/cadastros/servicos", servicoDto);
            servicoResponse.EnsureSuccessStatusCode();
            var servico = await servicoResponse.Content.ReadFromJsonAsync<RetornoServicoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Iniciar diagnóstico e adicionar serviço
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarServicosDto = new AdicionarServicosDTO
            {
                ServicosOriginaisIds = new List<Guid> { servico!.Id }
            };

            await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", adicionarServicosDto);

            // Act
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var resultado = await response.Content.ReadFromJsonAsync<RetornoOrcamentoDTO>();
            resultado.Should().NotBeNull();
            resultado!.Preco.Should().BeGreaterThan(0);

            // Verificar se o status foi atualizado no banco de dados
            var ordemNoDB = await context.OrdensServico.FirstOrDefaultAsync(o => o.Id == ordem.Id);
            ordemNoDB.Should().NotBeNull();
            ordemNoDB!.Status.Valor.Should().Be(StatusOrdemServicoEnum.AguardandoAprovacao);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/orcamento deve retornar status 404 quando ordem não existir")]
        [Trait("Method", "GerarOrcamento")]
        public async Task GerarOrcamento_ComOrdemInexistente_DeveRetornarStatus404()
        {
            // Arrange
            var ordemIdInexistente = Guid.NewGuid();

            // Act
            var response = await _client.PostAsync($"/api/ordens-servico/{ordemIdInexistente}/orcamento", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/orcamento deve retornar status 422 quando ordem em status inválido")]
        [Trait("Method", "GerarOrcamento")]
        public async Task GerarOrcamento_ComStatusInvalido_DeveRetornarStatus422()
        {
            // Arrange
            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Orçamento Status Inválido",
                DocumentoIdentificador = "10703165046"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "OSI1234",
                Modelo = "Civic Orç Status Inválido",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Act - Tentar gerar orçamento com ordem em status Recebida (sem serviços)
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/orcamento", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/orcamento deve retornar status 409 quando orçamento já existe")]
        [Trait("Method", "GerarOrcamento")]
        public async Task GerarOrcamento_ComOrcamentoJaExistente_DeveRetornarStatus409()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Orçamento Existente",
                DocumentoIdentificador = "01305624084"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "OEX1234",
                Modelo = "Civic Orçamento Existente",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar serviço de teste
            var servicoDto = new CriarServicoDTO
            {
                Nome = "Serviço Orçamento Existente",
                Preco = 130.00m
            };

            var servicoResponse = await _client.PostAsJsonAsync("/api/cadastros/servicos", servicoDto);
            servicoResponse.EnsureSuccessStatusCode();
            var servico = await servicoResponse.Content.ReadFromJsonAsync<RetornoServicoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Iniciar diagnóstico e adicionar serviço
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarServicosDto = new AdicionarServicosDTO
            {
                ServicosOriginaisIds = new List<Guid> { servico!.Id }
            };

            await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", adicionarServicosDto);

            // Gerar orçamento pela primeira vez
            var firstResponse = await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento", null);
            firstResponse.EnsureSuccessStatusCode();

            // Act - Tentar gerar orçamento novamente
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        #endregion

        #region Método AprovarOrcamento Tests

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/orcamento/aprovar deve retornar status 204 com dados válidos")]
        [Trait("Method", "AprovarOrcamento")]
        public async Task AprovarOrcamento_ComDadosValidos_DeveRetornarStatus204()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Aprovar Orçamento",
                DocumentoIdentificador = "33507068001"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "APR1234",
                Modelo = "Civic Aprovar",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar serviço de teste
            var servicoDto = new CriarServicoDTO
            {
                Nome = "Serviço Aprovar Test",
                Preco = 140.00m
            };

            var servicoResponse = await _client.PostAsJsonAsync("/api/cadastros/servicos", servicoDto);
            servicoResponse.EnsureSuccessStatusCode();
            var servico = await servicoResponse.Content.ReadFromJsonAsync<RetornoServicoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Iniciar diagnóstico, adicionar serviço e gerar orçamento
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarServicosDto = new AdicionarServicosDTO
            {
                ServicosOriginaisIds = new List<Guid> { servico!.Id }
            };

            await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", adicionarServicosDto);
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento", null);

            // Act
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento/aprovar", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verificar se o status foi atualizado no banco de dados
            var ordemNoDB = await context.OrdensServico.FirstOrDefaultAsync(o => o.Id == ordem.Id);
            ordemNoDB.Should().NotBeNull();
            ordemNoDB!.Status.Valor.Should().Be(StatusOrdemServicoEnum.EmExecucao);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/orcamento/aprovar deve retornar status 404 quando ordem não existir")]
        [Trait("Method", "AprovarOrcamento")]
        public async Task AprovarOrcamento_ComOrdemInexistente_DeveRetornarStatus404()
        {
            // Arrange
            var ordemIdInexistente = Guid.NewGuid();

            // Act
            var response = await _client.PostAsync($"/api/ordens-servico/{ordemIdInexistente}/orcamento/aprovar", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/orcamento/aprovar deve retornar status 422 quando ordem em status inválido")]
        [Trait("Method", "AprovarOrcamento")]
        public async Task AprovarOrcamento_ComStatusInvalido_DeveRetornarStatus422()
        {
            // Arrange
            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Aprovar Status Inválido",
                DocumentoIdentificador = "19345824090"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "ASI1234",
                Modelo = "Civic Aprovar Status Inválido",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Act - Tentar aprovar orçamento em ordem sem orçamento (status Recebida)
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/orcamento/aprovar", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/orcamento/aprovar deve retornar status 422 quando itens em estoque não estão disponíveis")]
        [Trait("Method", "AprovarOrcamento")]
        public async Task AprovarOrcamento_ComItensIndisponiveis_DeveRetornarStatus422()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Estoque Insuficiente",
                DocumentoIdentificador = "71102408000"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "EST1234",
                Modelo = "Civic Estoque",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar item de estoque com quantidade limitada
            var itemEstoqueDto = new
            {
                Nome = "Item Quantidade Limitada",
                Quantidade = 1, // Quantidade baixa
                TipoItemEstoque = 1,
                Preco = 50.00m
            };

            var itemResponse = await _client.PostAsJsonAsync("/api/estoque/itens", itemEstoqueDto);
            itemResponse.EnsureSuccessStatusCode();
            var itemEstoque = await itemResponse.Content.ReadFromJsonAsync<RetornoItemEstoqueDTO>();

            // Criar serviço de teste
            var servicoDto = new CriarServicoDTO
            {
                Nome = "Serviço Estoque Test",
                Preco = 200.00m
            };

            var servicoResponse = await _client.PostAsJsonAsync("/api/cadastros/servicos", servicoDto);
            servicoResponse.EnsureSuccessStatusCode();
            var servico = await servicoResponse.Content.ReadFromJsonAsync<RetornoServicoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Iniciar diagnóstico, adicionar serviço e item com quantidade maior que disponível
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarServicosDto = new AdicionarServicosDTO
            {
                ServicosOriginaisIds = new List<Guid> { servico!.Id }
            };

            await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", adicionarServicosDto);

            var adicionarItemDto = new AdicionarItemDTO
            {
                ItemEstoqueOriginalId = itemEstoque!.Id,
                Quantidade = 5 // Maior que a quantidade disponível (1)
            };

            await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/itens", adicionarItemDto);
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento", null);

            // Act - Tentar aprovar orçamento com estoque insuficiente
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento/aprovar", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/orcamento/aprovar deve atualizar quantidade em estoque no banco de dados")]
        [Trait("Method", "AprovarOrcamento")]
        public async Task AprovarOrcamento_ComDadosValidos_DeveAtualizarEstoqueNoBanco()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Atualizar Estoque",
                DocumentoIdentificador = "66901151004"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "ATE1234",
                Modelo = "Civic Atualizar Estoque",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar item de estoque com quantidade suficiente
            var itemEstoqueDto = new CriarItemEstoqueDTO
            {
                Nome = "Item Atualizar Estoque",
                Quantidade = 10,
                TipoItemEstoque = TipoItemEstoqueEnum.Peca,
                Preco = 60.00m
            };

            var itemResponse = await _client.PostAsJsonAsync("/api/estoque/itens", itemEstoqueDto);
            itemResponse.EnsureSuccessStatusCode();
            var itemEstoque = await itemResponse.Content.ReadFromJsonAsync<RetornoItemEstoqueDTO>();

            // Criar serviço de teste
            var servicoDto = new CriarServicoDTO
            {
                Nome = "Serviço Atualizar Estoque",
                Preco = 250.00m
            };

            var servicoResponse = await _client.PostAsJsonAsync("/api/cadastros/servicos", servicoDto);
            servicoResponse.EnsureSuccessStatusCode();
            var servico = await servicoResponse.Content.ReadFromJsonAsync<RetornoServicoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Iniciar diagnóstico, adicionar serviço e item
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarServicosDto = new AdicionarServicosDTO
            {
                ServicosOriginaisIds = new List<Guid> { servico!.Id }
            };

            await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", adicionarServicosDto);

            var quantidadeUtilizada = 3;
            var adicionarItemDto = new AdicionarItemDTO
            {
                ItemEstoqueOriginalId = itemEstoque!.Id,
                Quantidade = quantidadeUtilizada
            };

            await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/itens", adicionarItemDto);
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento", null);

            // Verificar quantidade inicial no estoque
            var estoqueAntes = await context.ItensEstoque.FirstOrDefaultAsync(i => i.Id == itemEstoque.Id);
            var quantidadeAntes = estoqueAntes!.Quantidade.Valor;

            // Act
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento/aprovar", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Limpar o tracking do EF Core para garantir que obtemos dados atualizados do banco
            context.ChangeTracker.Clear();

            // Verificar se a quantidade em estoque foi reduzida no banco de dados
            var estoqueDepois = await context.ItensEstoque.FirstOrDefaultAsync(i => i.Id == itemEstoque.Id);
            estoqueDepois.Should().NotBeNull();
            estoqueDepois!.Quantidade.Valor.Should().Be(quantidadeAntes - quantidadeUtilizada);
        }

        #endregion

        #region Método DesaprovarOrcamento Tests

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/orcamento/desaprovar deve retornar status 204 com dados válidos")]
        [Trait("Method", "DesaprovarOrcamento")]
        public async Task DesaprovarOrcamento_ComDadosValidos_DeveRetornarStatus204()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Desaprovar Orçamento",
                DocumentoIdentificador = "99591675003"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "DES1234",
                Modelo = "Civic Desaprovar",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar serviço de teste
            var servicoDto = new CriarServicoDTO
            {
                Nome = "Serviço Desaprovar Test",
                Preco = 150.00m
            };

            var servicoResponse = await _client.PostAsJsonAsync("/api/cadastros/servicos", servicoDto);
            servicoResponse.EnsureSuccessStatusCode();
            var servico = await servicoResponse.Content.ReadFromJsonAsync<RetornoServicoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Iniciar diagnóstico, adicionar serviço e gerar orçamento
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarServicosDto = new AdicionarServicosDTO
            {
                ServicosOriginaisIds = new List<Guid> { servico!.Id }
            };

            await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", adicionarServicosDto);
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento", null);

            // Act
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento/desaprovar", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verificar se o status foi atualizado no banco de dados
            var ordemNoDB = await context.OrdensServico.FirstOrDefaultAsync(o => o.Id == ordem.Id);
            ordemNoDB.Should().NotBeNull();
            ordemNoDB!.Status.Valor.Should().Be(StatusOrdemServicoEnum.Cancelada);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/orcamento/desaprovar deve retornar status 404 quando ordem não existir")]
        [Trait("Method", "DesaprovarOrcamento")]
        public async Task DesaprovarOrcamento_ComOrdemInexistente_DeveRetornarStatus404()
        {
            // Arrange
            var ordemIdInexistente = Guid.NewGuid();

            // Act
            var response = await _client.PostAsync($"/api/ordens-servico/{ordemIdInexistente}/orcamento/desaprovar", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/orcamento/desaprovar deve retornar status 422 quando ordem em status inválido")]
        [Trait("Method", "DesaprovarOrcamento")]
        public async Task DesaprovarOrcamento_ComStatusInvalido_DeveRetornarStatus422()
        {
            // Arrange
            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Desaprovar Status Inválido",
                DocumentoIdentificador = "02686881097"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "DSI1234",
                Modelo = "Civic Desaprovar Status Inválido",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Act - Tentar desaprovar orçamento em ordem sem orçamento (status Recebida)
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/orcamento/desaprovar", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        #endregion

        #region Método FinalizarExecucao Tests

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/finalizar-execucao deve retornar status 204 com dados válidos")]
        [Trait("Method", "FinalizarExecucao")]
        public async Task FinalizarExecucao_ComDadosValidos_DeveRetornarStatus204()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Finalizar Execução",
                DocumentoIdentificador = "37152507040"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "FIN1234",
                Modelo = "Civic Finalizar",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar serviço de teste
            var servicoDto = new CriarServicoDTO
            {
                Nome = "Serviço Finalizar Test",
                Preco = 160.00m
            };

            var servicoResponse = await _client.PostAsJsonAsync("/api/cadastros/servicos", servicoDto);
            servicoResponse.EnsureSuccessStatusCode();
            var servico = await servicoResponse.Content.ReadFromJsonAsync<RetornoServicoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Fluxo completo: diagnóstico -> serviço -> orçamento -> aprovar
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarServicosDto = new AdicionarServicosDTO
            {
                ServicosOriginaisIds = new List<Guid> { servico!.Id }
            };

            await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", adicionarServicosDto);
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento", null);
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento/aprovar", null);

            // Act
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/finalizar-execucao", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verificar se o status foi atualizado no banco de dados
            var ordemNoDB = await context.OrdensServico.FirstOrDefaultAsync(o => o.Id == ordem.Id);
            ordemNoDB.Should().NotBeNull();
            ordemNoDB!.Status.Valor.Should().Be(StatusOrdemServicoEnum.Finalizada);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/finalizar-execucao deve retornar status 404 quando ordem não existir")]
        [Trait("Method", "FinalizarExecucao")]
        public async Task FinalizarExecucao_ComOrdemInexistente_DeveRetornarStatus404()
        {
            // Arrange
            var ordemIdInexistente = Guid.NewGuid();

            // Act
            var response = await _client.PostAsync($"/api/ordens-servico/{ordemIdInexistente}/finalizar-execucao", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/finalizar-execucao deve retornar status 422 quando ordem em status inválido")]
        [Trait("Method", "FinalizarExecucao")]
        public async Task FinalizarExecucao_ComStatusInvalido_DeveRetornarStatus422()
        {
            // Arrange
            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Finalizar Status Inválido",
                DocumentoIdentificador = "22311077082"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "FSI1234",
                Modelo = "Civic Finalizar Status Inválido",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Act - Tentar finalizar execução em ordem sem estar em execução (status Recebida)
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/finalizar-execucao", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        #endregion

        #region Método Entregar Tests

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/entregar deve retornar status 204 com dados válidos")]
        [Trait("Method", "Entregar")]
        public async Task Entregar_ComDadosValidos_DeveRetornarStatus204()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Entregar",
                DocumentoIdentificador = "69876716050"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "ENT1234",
                Modelo = "Civic Entregar",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar serviço de teste
            var servicoDto = new CriarServicoDTO
            {
                Nome = "Serviço Entregar Test",
                Preco = 170.00m
            };

            var servicoResponse = await _client.PostAsJsonAsync("/api/cadastros/servicos", servicoDto);
            servicoResponse.EnsureSuccessStatusCode();
            var servico = await servicoResponse.Content.ReadFromJsonAsync<RetornoServicoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Fluxo completo: diagnóstico -> serviço -> orçamento -> aprovar -> finalizar
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarServicosDto = new AdicionarServicosDTO
            {
                ServicosOriginaisIds = new List<Guid> { servico!.Id }
            };

            await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", adicionarServicosDto);
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento", null);
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento/aprovar", null);
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/finalizar-execucao", null);

            // Act
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/entregar", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Verificar se o status foi atualizado no banco de dados
            var ordemNoDB = await context.OrdensServico.FirstOrDefaultAsync(o => o.Id == ordem.Id);
            ordemNoDB.Should().NotBeNull();
            ordemNoDB!.Status.Valor.Should().Be(StatusOrdemServicoEnum.Entregue);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/entregar deve retornar status 404 quando ordem não existir")]
        [Trait("Method", "Entregar")]
        public async Task Entregar_ComOrdemInexistente_DeveRetornarStatus404()
        {
            // Arrange
            var ordemIdInexistente = Guid.NewGuid();

            // Act
            var response = await _client.PostAsync($"/api/ordens-servico/{ordemIdInexistente}/entregar", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/{id}/entregar deve retornar status 422 quando ordem em status inválido")]
        [Trait("Method", "Entregar")]
        public async Task Entregar_ComStatusInvalido_DeveRetornarStatus422()
        {
            // Arrange
            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Entregar Status Inválido",
                DocumentoIdentificador = "42255228068"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "ESI1234",
                Modelo = "Civic Entregar Status Inválido",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Act - Tentar entregar ordem sem estar finalizada (status Recebida)
            var response = await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/entregar", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        #endregion

        #region Método ObterTempoMedio Tests

        [Fact(DisplayName = "GET /api/ordens-servico/tempo-medio deve retornar status 200 com dados válidos")]
        [Trait("Method", "ObterTempoMedio")]
        public async Task ObterTempoMedio_ComDadosValidos_DeveRetornarStatus200()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Tempo Médio",
                DocumentoIdentificador = "47403229002"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "TMP1234",
                Modelo = "Civic Tempo Médio",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar serviço de teste
            var servicoDto = new CriarServicoDTO
            {
                Nome = "Serviço Tempo Médio",
                Preco = 300.00m
            };

            var servicoResponse = await _client.PostAsJsonAsync("/api/cadastros/servicos", servicoDto);
            servicoResponse.EnsureSuccessStatusCode();
            var servico = await servicoResponse.Content.ReadFromJsonAsync<RetornoServicoDTO>();

            // Criar e completar uma ordem de serviço para ter dados no cálculo
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            // Completar fluxo da ordem de serviço
            await _client.PostAsync($"/api/ordens-servico/{ordem!.Id}/iniciar-diagnostico", null);

            var adicionarServicosDto = new AdicionarServicosDTO
            {
                ServicosOriginaisIds = new List<Guid> { servico!.Id }
            };

            await _client.PostAsJsonAsync($"/api/ordens-servico/{ordem.Id}/servicos", adicionarServicosDto);
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento", null);
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/orcamento/aprovar", null);
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/finalizar-execucao", null);
            await _client.PostAsync($"/api/ordens-servico/{ordem.Id}/entregar", null);

            var quantidadeDias = 30;

            // Act
            var response = await _client.GetAsync($"/api/ordens-servico/tempo-medio?quantidadeDias={quantidadeDias}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var resultado = await response.Content.ReadFromJsonAsync<RetornoTempoMedioDTO>();
            resultado.Should().NotBeNull();
            resultado!.QuantidadeDias.Should().Be(quantidadeDias);
            resultado.QuantidadeOrdensAnalisadas.Should().BeGreaterThanOrEqualTo(1);
            resultado.DataInicio.Should().BeOnOrBefore(DateTime.UtcNow);
            resultado.DataFim.Should().BeOnOrAfter(resultado.DataInicio);
        }

        [Fact(DisplayName = "GET /api/ordens-servico/tempo-medio deve retornar status 400 com parâmetros inválidos")]
        [Trait("Method", "ObterTempoMedio")]
        public async Task ObterTempoMedio_ComParametrosInvalidos_DeveRetornarStatus400()
        {
            // Arrange
            var quantidadeDiasInvalida = 500; // Maior que 365

            // Act
            var response = await _client.GetAsync($"/api/ordens-servico/tempo-medio?quantidadeDias={quantidadeDiasInvalida}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "GET /api/ordens-servico/tempo-medio deve retornar status 422 quando nenhuma ordem entregue")]
        [Trait("Method", "ObterTempoMedio")]
        public async Task ObterTempoMedio_SemOrdensEntregues_DeveRetornarStatus400()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Limpar todas as ordens de serviço para garantir que não há ordens entregues
            var ordensExistentes = context.OrdensServico.ToList();
            context.OrdensServico.RemoveRange(ordensExistentes);
            await context.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync("/api/ordens-servico/tempo-medio?quantidadeDias=30");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableContent);
        }

        [Fact(DisplayName = "GET /api/ordens-servico/tempo-medio deve retornar status 422 quando não há ordens no período especificado")]
        [Trait("Method", "ObterTempoMedio")]
        public async Task ObterTempoMedio_SemOrdensNoPeriodo_DeveRetornarStatus422()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Limpar todas as ordens de serviço para garantir que não há ordens entregues
            var ordensExistentes = context.OrdensServico.ToList();
            context.OrdensServico.RemoveRange(ordensExistentes);

            // Act - Buscar por período de 30 dias (não incluirá a ordem antiga)
            var response = await _client.GetAsync("/api/ordens-servico/tempo-medio?quantidadeDias=30");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }

        #endregion

        #region Método BuscaPublica Tests

        [Fact(DisplayName = "POST /api/ordens-servico/busca-publica deve retornar status 200 com dados válidos")]
        [Trait("Method", "BuscaPublica")]
        public async Task BuscaPublica_ComDadosValidos_DeveRetornarStatus200()
        {
            // Arrange
            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Busca Pública",
                DocumentoIdentificador = "56838566044"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "BUS1234",
                Modelo = "Civic Busca Pública",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            var buscaDto = new BuscaPublicaOrdemServicoDTO
            {
                CodigoOrdemServico = ordem!.Codigo,
                DocumentoIdentificadorCliente = cliente.DocumentoIdentificador
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/ordens-servico/busca-publica", buscaDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var resultado = await response.Content.ReadFromJsonAsync<RetornoOrdemServicoCompletaDTO>();
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(ordem.Id);
            resultado.Codigo.Should().Be(ordem.Codigo);
        }

        [Fact(DisplayName = "POST /api/ordens-servico/busca-publica deve retornar status 200 mesmo com ordem inexistente")]
        [Trait("Method", "BuscaPublica")]
        public async Task BuscaPublica_ComOrdemInexistente_DeveRetornarStatus200()
        {
            // Arrange
            var buscaDto = new BuscaPublicaOrdemServicoDTO
            {
                CodigoOrdemServico = "OS-INEXISTENTE-123",
                DocumentoIdentificadorCliente = "99999999999"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/ordens-servico/busca-publica", buscaDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("null"); // Deve retornar null como JSON
        }

        [Fact(DisplayName = "POST /api/ordens-servico/busca-publica deve retornar status 200 mesmo com cliente inexistente")]
        [Trait("Method", "BuscaPublica")]
        public async Task BuscaPublica_ComClienteInexistente_DeveRetornarStatus200()
        {
            // Arrange
            // Criar cliente de teste
            var clienteDto = new CriarClienteDTO
            {
                Nome = "Cliente Para Codigo",
                DocumentoIdentificador = "31944036059"
            };

            var clienteResponse = await _client.PostAsJsonAsync("/api/cadastros/clientes", clienteDto);
            clienteResponse.EnsureSuccessStatusCode();
            var cliente = await clienteResponse.Content.ReadFromJsonAsync<RetornoClienteDTO>();

            // Criar veículo de teste
            var veiculoDto = new CriarVeiculoDTO
            {
                ClienteId = cliente!.Id,
                Placa = "CLI1234",
                Modelo = "Civic Cliente Inexistente",
                Marca = "Honda",
                Cor = "Vermelho",
                Ano = 2023,
                TipoVeiculo = TipoVeiculoEnum.Carro
            };

            var veiculoResponse = await _client.PostAsJsonAsync("/api/cadastros/veiculos", veiculoDto);
            veiculoResponse.EnsureSuccessStatusCode();
            var veiculo = await veiculoResponse.Content.ReadFromJsonAsync<RetornoVeiculoDTO>();

            // Criar ordem de serviço
            var ordemDto = new CriarOrdemServicoDTO { VeiculoId = veiculo!.Id };
            var ordemResponse = await _client.PostAsJsonAsync("/api/ordens-servico", ordemDto);
            ordemResponse.EnsureSuccessStatusCode();
            var ordem = await ordemResponse.Content.ReadFromJsonAsync<RetornoOrdemServicoDTO>();

            var buscaDto = new BuscaPublicaOrdemServicoDTO
            {
                CodigoOrdemServico = ordem!.Codigo,
                DocumentoIdentificadorCliente = "88888888888" // Cliente inexistente
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/ordens-servico/busca-publica", buscaDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("null"); // Deve retornar null como JSON
        }

        [Fact(DisplayName = "POST /api/ordens-servico/busca-publica deve retornar status 200 mesmo com dados completamente inválidos")]
        [Trait("Method", "BuscaPublica")]
        public async Task BuscaPublica_ComDadosInvalidos_DeveRetornarStatus200()
        {
            // Arrange
            var buscaDto = new BuscaPublicaOrdemServicoDTO
            {
                CodigoOrdemServico = "",
                DocumentoIdentificadorCliente = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/ordens-servico/busca-publica", buscaDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            content.Should().Be("null"); // Deve retornar null como JSON
        }

        #endregion
    }
}
