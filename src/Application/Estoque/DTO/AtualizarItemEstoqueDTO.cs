using Domain.Estoque.Enums;

namespace Application.Estoque.DTO
{
    /// <summary>
    /// DTO para atualização de item de estoque
    /// </summary>
    public class AtualizarItemEstoqueDTO
    {
        /// <summary>
        /// Nome do item de estoque
        /// </summary>
        /// <example>Filtro de Óleo</example>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Quantidade em estoque
        /// </summary>
        /// <example>50</example>
        public int Quantidade { get; set; }

        /// <summary>
        /// Tipo do item de estoque
        /// </summary>
        /// <example>Peca</example>
        public TipoItemEstoqueEnum TipoItemEstoque { get; set; }

        /// <summary>
        /// Preço unitário do item
        /// </summary>
        /// <example>25.50</example>
        public decimal Preco { get; set; }
    }
}
