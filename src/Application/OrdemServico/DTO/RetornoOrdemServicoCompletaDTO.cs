namespace Application.OrdemServico.DTO
{
    /// <summary>
    /// DTO para retorno completo de ordem de serviço
    /// </summary>
    public class RetornoOrdemServicoCompletaDTO
    {
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// ID do veículo associado à ordem de serviço
        /// </summary>
        /// <example>123e4567-e89b-12d3-a456-426614174000</example>
        public Guid VeiculoId { get; set; } = Guid.Empty;

        /// <summary>
        /// Código da ordem de serviço
        /// </summary>
        /// <example>OS-20250125-ABC123</example>
        public string Codigo { get; set; } = string.Empty;

        /// <summary>
        /// Status atual da ordem de serviço
        /// </summary>
        /// <example>recebida</example>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Data de criação da ordem de serviço
        /// </summary>
        /// <example>2025-01-25T10:30:00Z</example>
        public DateTime DataCriacao { get; set; }

        /// <summary>
        /// Data de início da execução (se iniciada)
        /// </summary>
        /// <example>2025-01-26T08:00:00Z</example>
        public DateTime? DataInicioExecucao { get; set; }

        /// <summary>
        /// Data de finalização da execução (se finalizada)
        /// </summary>
        /// <example>2025-01-27T17:00:00Z</example>
        public DateTime? DataFinalizacao { get; set; }

        /// <summary>
        /// Data de entrega (se entregue)
        /// </summary>
        /// <example>2025-01-28T10:00:00Z</example>
        public DateTime? DataEntrega { get; set; }

        /// <summary>
        /// Lista de serviços incluídos na ordem de serviço
        /// </summary>
        public List<RetornoServicoIncluidoDTO> ServicosIncluidos { get; set; } = new();

        /// <summary>
        /// Lista de itens incluídos na ordem de serviço
        /// </summary>
        public List<RetornoItemIncluidoDTO> ItensIncluidos { get; set; } = new();

        /// <summary>
        /// Orçamento gerado (se disponível)
        /// </summary>
        public RetornoOrcamentoDTO? Orcamento { get; set; }
    }
}
