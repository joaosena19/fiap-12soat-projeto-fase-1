namespace Application.Cadastros.DTO
{
    /// <summary>
    /// DTO para atualização de serviço
    /// </summary>
    public class AtualizarServicoDTO
    {
        /// <summary>
        /// Nome completo do servico
        /// </summary>
        /// <example>Troca de óleo</example>
        public string Nome { get; set; } = string.Empty;

        /// <summary>
        /// Preço do serviço
        /// </summary>
        /// <example>100.00</example>
        public decimal Preco { get; set; } = 0L;
    }
}
