﻿namespace Application.Cadastros.DTO
{
    /// <summary>
    /// DTO para criação de servico
    /// </summary>
    public class CriarServicoDTO
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
