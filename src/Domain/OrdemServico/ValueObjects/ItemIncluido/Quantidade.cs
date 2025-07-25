using System.Net;
using Shared.Exceptions;

namespace Domain.OrdemServico.ValueObjects.ItemIncluido
{
    public record Quantidade
    {
        private readonly int _valor;

        // Construtor sem parâmetro para o EF Core
        private Quantidade() { }

        public Quantidade(int quantidade)
        {
            if (quantidade < 0)
                throw new DomainException("Quantidade não pode ser negativa", HttpStatusCode.BadRequest);

            _valor = quantidade;
        }

        public int Valor => _valor;
    }
}
