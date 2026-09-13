using System.Collections.Generic;
using NUnit.Framework;
using PrograMago.Domain;

namespace PrograMago.Tests.Domain
{
    public sealed class MagoStateTests
    {
        [Test]
        public void FromValidatedProgram_MapsValuesByAttributeNameAndPreservesRemainingPoints()
        {
            var values = new Dictionary<string, int>
            {
                ["dano"] = 7,
                ["vida"] = 5,
                ["velocidadeAtaque"] = 2,
                ["alcance"] = 3,
                ["iniciativa"] = 4
            };
            var program = new ValidatedMagoProgram(
                "Mago", values.Keys, "heroi", values, 25);
            MagoState state = MagoState.FromValidatedProgram(program);

            Assert.That(state.InstanceName, Is.EqualTo("heroi"));
            Assert.That(state.Vida, Is.EqualTo(5));
            Assert.That(state.Dano, Is.EqualTo(7));
            Assert.That(state.Alcance, Is.EqualTo(3));
            Assert.That(state.Iniciativa, Is.EqualTo(4));
            Assert.That(state.VelocidadeAtaque, Is.EqualTo(2));
            Assert.That(state.RemainingPoints, Is.EqualTo(4));
        }
    }
}
