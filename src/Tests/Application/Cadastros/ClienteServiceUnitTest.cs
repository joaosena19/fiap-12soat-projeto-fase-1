using Application.Cadastros.Interfaces;
using Application.Cadastros.Services;
using AutoMapper;
using Domain.Cadastros.Aggregates;
using FluentAssertions;
using Moq;
using Shared.Exceptions;
using Application;

namespace Tests.Application.Cadastros
{
    public class ClienteServiceUnitTest
    {
        private readonly Mock<IClienteRepository> _repoMock;
        private readonly IMapper _mapper;
        private readonly ClienteService _service;

        public ClienteServiceUnitTest()
        {
            _repoMock = new Mock<IClienteRepository>();
            _mapper = AutoMapperConfig.CreateMapper();
            _service = new ClienteService(_repoMock.Object, _mapper);
        }

        [Fact(DisplayName = "N�o deve criar cliente se CPF j� existir")]
        [Trait("Metodo", "CriarCliente")]
        public async Task CriarCliente_DeveLancarExcecao_SeCpfJaExistir()
        {
            // Arrange
            var cpf = "12345678909";
            var nome = "Jo�o";
            var clienteExistente = Cliente.Criar(nome, cpf);

            _repoMock.Setup(r => r.ObterPorDocumentoAsync(cpf))
                .ReturnsAsync(clienteExistente);

            // Act
            var act = async () => await _service.CriarCliente(nome, cpf);

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("J� existe um cliente cadastrado com este documento.");

            _repoMock.Verify(r => r.SalvarAsync(It.IsAny<Cliente>()), Times.Never);
        }

        [Fact(DisplayName = "Deve criar cliente se CPF for novo")]
        [Trait("Metodo", "CriarCliente")]
        public async Task CriarCliente_DeveSalvarCliente_SeCpfNaoExistir()
        {
            // Arrange
            var cpf = "12345678909";
            var nome = "Jo�o";

            var clienteNovo = Cliente.Criar(nome, cpf);

            _repoMock.Setup(r => r.ObterPorDocumentoAsync(cpf))
                .ReturnsAsync((Cliente?)null);

            _repoMock.Setup(r => r.SalvarAsync(It.IsAny<Cliente>()))
                .ReturnsAsync(clienteNovo);

            // Act
            await _service.CriarCliente(nome, cpf);

            // Assert
            _repoMock.Verify(r => r.SalvarAsync(It.Is<Cliente>(c =>
                c.DocumentoIdentificador.Valor == cpf && c.Nome.Valor == nome
            )), Times.Once);
        }

        [Fact(DisplayName = "Deve atualizar cliente se existir")]
        [Trait("Metodo", "AtualizarCliente")]
        public async Task AtualizarCliente_DeveAtualizarCliente_SeClienteExistir()
        {
            // Arrange
            var id = Guid.NewGuid();
            var nomeOriginal = "Jo�o";
            var nomeNovo = "Jo�o Silva";
            var cpf = "12345678909";

            var clienteExistente = Cliente.Criar(nomeOriginal, cpf);
            var clienteAtualizado = Cliente.Criar(nomeNovo, cpf);

            _repoMock.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync(clienteExistente);

            _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<Cliente>()))
                .ReturnsAsync(clienteAtualizado);

            // Act
            var result = await _service.AtualizarCliente(id, nomeNovo);

            // Assert
            result.Should().NotBeNull();
            result.Nome.Should().Be(nomeNovo);
            _repoMock.Verify(r => r.ObterPorIdAsync(id), Times.Once);
            _repoMock.Verify(r => r.AtualizarAsync(It.IsAny<Cliente>()), Times.Once);
        }

        [Fact(DisplayName = "N�o deve atualizar cliente se n�o existir")]
        [Trait("Metodo", "AtualizarCliente")]
        public async Task AtualizarCliente_DeveLancarExcecao_SeClienteNaoExistir()
        {
            // Arrange
            var id = Guid.NewGuid();
            var nome = "Jo�o Silva";

            _repoMock.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync((Cliente?)null);

            // Act
            var act = async () => await _service.AtualizarCliente(id, nome);

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Cliente n�o encontrado.");

            _repoMock.Verify(r => r.AtualizarAsync(It.IsAny<Cliente>()), Times.Never);
        }

        [Fact(DisplayName = "Deve buscar todos os clientes")]
        [Trait("Metodo", "Buscar")]
        public async Task Buscar_DeveRetornarTodosOsClientes()
        {
            // Arrange
            var cliente1 = Cliente.Criar("Jo�o", "12345678909");
            var cliente2 = Cliente.Criar("Maria", "35434856015");
            var clientes = new List<Cliente> { cliente1, cliente2 };

            _repoMock.Setup(r => r.ObterTodosAsync())
                .ReturnsAsync(clientes);

            // Act
            var result = await _service.Buscar();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Nome == "Jo�o" && c.DocumentoIdentificador == "12345678909");
            result.Should().Contain(c => c.Nome == "Maria" && c.DocumentoIdentificador == "35434856015");
            _repoMock.Verify(r => r.ObterTodosAsync(), Times.Once);
        }

        [Fact(DisplayName = "Deve retornar lista vazia quando n�o h� clientes")]
        [Trait("Metodo", "Buscar")]
        public async Task Buscar_DeveRetornarListaVazia_QuandoNaoHaClientes()
        {
            // Arrange
            _repoMock.Setup(r => r.ObterTodosAsync())
                .ReturnsAsync(new List<Cliente>());

            // Act
            var result = await _service.Buscar();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _repoMock.Verify(r => r.ObterTodosAsync(), Times.Once);
        }

        [Fact(DisplayName = "Deve buscar cliente por ID quando existir")]
        [Trait("Metodo", "BuscarPorId")]
        public async Task BuscarPorId_DeveRetornarCliente_QuandoClienteExistir()
        {
            // Arrange
            var id = Guid.NewGuid();
            var nome = "Jo�o";
            var cpf = "12345678909";
            var cliente = Cliente.Criar(nome, cpf);

            _repoMock.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync(cliente);

            // Act
            var result = await _service.BuscarPorId(id);

            // Assert
            result.Should().NotBeNull();
            result.Nome.Should().Be(nome);
            result.DocumentoIdentificador.Should().Be(cpf);
            _repoMock.Verify(r => r.ObterPorIdAsync(id), Times.Once);
        }

        [Fact(DisplayName = "Deve lan�ar exce��o ao buscar cliente por ID quando n�o existir")]
        [Trait("Metodo", "BuscarPorId")]
        public async Task BuscarPorId_DeveLancarExcecao_QuandoClienteNaoExistir()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repoMock.Setup(r => r.ObterPorIdAsync(id))
                .ReturnsAsync((Cliente?)null);

            // Act
            var act = async () => await _service.BuscarPorId(id);

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Cliente n�o encontrado.");
            _repoMock.Verify(r => r.ObterPorIdAsync(id), Times.Once);
        }

        [Fact(DisplayName = "Deve buscar cliente por documento quando existir")]
        [Trait("Metodo", "BuscarPorDocumento")]
        public async Task BuscarPorDocumento_DeveRetornarCliente_QuandoClienteExistir()
        {
            // Arrange
            var cpf = "12345678909";
            var nome = "Jo�o";
            var cliente = Cliente.Criar(nome, cpf);

            _repoMock.Setup(r => r.ObterPorDocumentoAsync(cpf))
                .ReturnsAsync(cliente);

            // Act
            var result = await _service.BuscarPorDocumento(cpf);

            // Assert
            result.Should().NotBeNull();
            result.Nome.Should().Be(nome);
            result.DocumentoIdentificador.Should().Be(cpf);
            _repoMock.Verify(r => r.ObterPorDocumentoAsync(cpf), Times.Once);
        }

        [Fact(DisplayName = "Deve lan�ar exce��o ao buscar cliente por CPF quando n�o existir")]
        [Trait("Metodo", "BuscarPorDocumento")]
        public async Task BuscarPorDocumento_DeveLancarExcecao_QuandoClienteNaoExistir()
        {
            // Arrange
            var cpf = "12345678901";

            _repoMock.Setup(r => r.ObterPorDocumentoAsync(cpf))
                .ReturnsAsync((Cliente?)null);

            // Act
            var act = async () => await _service.BuscarPorDocumento(cpf);

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .WithMessage("Cliente n�o encontrado.");
            _repoMock.Verify(r => r.ObterPorDocumentoAsync(cpf), Times.Once);
        }
    }
}
