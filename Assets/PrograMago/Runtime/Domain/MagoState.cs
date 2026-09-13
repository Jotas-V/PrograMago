using System;

namespace PrograMago.Domain
{
    public sealed class MagoState
    {
        private MagoState(
            string instanceName,
            int vida,
            int dano,
            int alcance,
            int iniciativa,
            int velocidadeAtaque,
            int remainingPoints)
        {
            InstanceName = instanceName;
            Vida = vida;
            Dano = dano;
            Alcance = alcance;
            Iniciativa = iniciativa;
            VelocidadeAtaque = velocidadeAtaque;
            RemainingPoints = remainingPoints;
        }

        public string InstanceName { get; }
        public int Vida { get; }
        public int Dano { get; }
        public int Alcance { get; }
        public int Iniciativa { get; }
        public int VelocidadeAtaque { get; }
        public int RemainingPoints { get; }

        public static MagoState FromValidatedProgram(ValidatedMagoProgram program)
        {
            if (program == null)
            {
                throw new ArgumentNullException(nameof(program));
            }

            if (string.IsNullOrWhiteSpace(program.InstanceName))
            {
                throw new ArgumentException("O programa ainda não instancia um Mago.", nameof(program));
            }

            return new MagoState(
                program.InstanceName,
                program.InitialValues["vida"],
                program.InitialValues["dano"],
                program.InitialValues["alcance"],
                program.InitialValues["iniciativa"],
                program.InitialValues["velocidadeAtaque"],
                program.RemainingPoints);
        }
    }
}
