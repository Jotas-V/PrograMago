using System;
using System.Reflection;
using NUnit.Framework;
using PrograMago.Domain;

namespace PrograMago.Tests.Domain
{
    public sealed class CombatWizardSpecializationTests
    {
        [TestCase("piromante", "Piromante", CombatElement.Fire)]
        [TestCase("hidromante", "Hidromante", CombatElement.Water)]
        [TestCase("eletromante", "Eletromante", CombatElement.Electric)]
        public void Specialize_CreatesTypedFormWithInheritedCombatData(
            string form, string typeName, CombatElement spell)
        {
            var baseWizard = new CombatWizard(9, 4, 7, 6, 3,
                CombatElement.Neutral, CombatElement.Fire, CombatElement.Water,
                CombatElement.Electric);
            MethodInfo factory = typeof(CombatWizard).GetMethod(
                "Specialize", BindingFlags.Public | BindingFlags.Static);

            Assert.That(factory, Is.Not.Null,
                "CombatWizard deve conseguir criar uma especialização tipada a partir da forma.");
            var specialized = (CombatWizard)factory.Invoke(null, new object[] { baseWizard, form });
            Type expectedType = typeof(CombatWizard).Assembly.GetType(
                "PrograMago.Domain." + typeName);

            Assert.That(expectedType, Is.Not.Null, "A classe " + typeName + " deve existir.");
            Assert.That(specialized.GetType(), Is.EqualTo(expectedType));
            Assert.That(specialized.GetType().IsSubclassOf(typeof(CombatWizard)), Is.True);
            Assert.That(specialized.Life, Is.EqualTo(baseWizard.Life));
            Assert.That(specialized.Damage, Is.EqualTo(baseWizard.Damage));
            Assert.That(specialized.Range, Is.EqualTo(baseWizard.Range));
            Assert.That(specialized.Initiative, Is.EqualTo(baseWizard.Initiative));
            Assert.That(specialized.AttackSpeed, Is.EqualTo(baseWizard.AttackSpeed));
            Assert.That(specialized.Spells, Is.EqualTo(baseWizard.Spells));
            Assert.That(ReadProperty<string>(specialized, "Form"), Is.EqualTo(form));
            Assert.That(ReadProperty<CombatElement>(specialized, "Spell"), Is.EqualTo(spell));
        }

        [Test]
        public void FromMago_ProvidesTheImplementedElementalSpellsToCombat()
        {
            var values = new System.Collections.Generic.Dictionary<string, int>
            {
                ["vida"] = 5,
                ["dano"] = 5,
                ["alcance"] = 5,
                ["iniciativa"] = 5,
                ["velocidadeAtaque"] = 5
            };
            MagoState mago = MagoState.FromValidatedProgram(new ValidatedMagoProgram(
                "Mago", values.Keys, "heroi", values, 25));

            CombatWizard wizard = CombatWizard.FromMago(mago);

            CollectionAssert.AreEquivalent(new[]
            {
                CombatElement.Neutral,
                CombatElement.Fire,
                CombatElement.Water,
                CombatElement.Electric
            }, wizard.Spells);
        }
        [Test]
        public void Specialize_NeutralFormKeepsTheBaseWizard()
        {
            var baseWizard = new CombatWizard(9, 4, 7, 6, 3, CombatElement.Neutral);
            MethodInfo factory = typeof(CombatWizard).GetMethod(
                "Specialize", BindingFlags.Public | BindingFlags.Static);

            Assert.That(factory, Is.Not.Null,
                "A forma neutra deve continuar disponível no modelo base.");
            Assert.That(factory.Invoke(null, new object[] { baseWizard, "neutro" }),
                Is.SameAs(baseWizard));
        }

        private static T ReadProperty<T>(object instance, string name)
        {
            PropertyInfo property = instance.GetType().GetProperty(name);
            Assert.That(property, Is.Not.Null, "A especialização deve expor " + name + ".");
            return (T)property.GetValue(instance);
        }
    }
}