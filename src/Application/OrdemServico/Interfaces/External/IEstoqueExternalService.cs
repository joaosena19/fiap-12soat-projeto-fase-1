using Application.OrdemServico.DTO.External;
using Domain.OrdemServico.Enums;

namespace Application.OrdemServico.Interfaces.External
{
    /// <summary>
    /// Interface anti-corruption para acessar dados do bounded context de Estoque
    /// </summary>
    public interface IEstoqueExternalService
    {
        Task<ItemEstoqueExternalDTO?> ObterItemEstoquePorIdAsync(Guid itemId);
        Task<bool> VerificarDisponibilidadeAsync(Guid itemId, int quantidadeNecessaria);
        Task AtualizarQuantidadeEstoqueAsync(Guid itemId, int novaQuantidade);
    }
}
