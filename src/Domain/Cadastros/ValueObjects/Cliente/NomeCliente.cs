﻿using Shared.Enums;
using Shared.Exceptions;

namespace Domain.Cadastros.ValueObjects.Cliente
{
    public record NomeCliente
    {
        private readonly string _valor = string.Empty;

        // Construtor sem parâmetro para EF Core
        private NomeCliente() { }

        public NomeCliente(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new DomainException("Nome não pode ser vazio", ErrorType.InvalidInput);

            if (nome.Length > 200)
                throw new DomainException("Nome não pode ter mais de 200 caracteres", ErrorType.InvalidInput);

            _valor = nome;
        }

        public string Valor => _valor;
    }
}
