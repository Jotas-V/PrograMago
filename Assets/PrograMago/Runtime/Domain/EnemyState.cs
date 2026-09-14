using System;

namespace PrograMago.Domain
{
    public sealed class EnemyState
    {
        public EnemyState(string variableName, string name, int vida, string elemento)
        {
            VariableName = string.IsNullOrWhiteSpace(variableName)
                ? throw new ArgumentException("Nome da variável obrigatório.", nameof(variableName))
                : variableName;
            Name = string.IsNullOrWhiteSpace(name)
                ? throw new ArgumentException("Nome do inimigo obrigatório.", nameof(name))
                : name;
            Vida = vida > 0 ? vida : throw new ArgumentOutOfRangeException(nameof(vida));
            Elemento = string.IsNullOrWhiteSpace(elemento)
                ? throw new ArgumentException("Elemento obrigatório.", nameof(elemento))
                : elemento;
        }

        public string VariableName { get; }

        public string Name { get; }

        public int Vida { get; }

        public string Elemento { get; }
    }
}
