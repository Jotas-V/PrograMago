using System;
using System.Reflection;
using NUnit.Framework;
using PrograMago.Application;
using PrograMago.Domain;

namespace PrograMago.Tests.Application
{
    public sealed class CombatStrategyCompilerTests
    {
        private const string CompleteStrategy =
            "if (alvo.getElemento().equals(\"gelo\")) { mago.selecionarForma(\"piromante\"); } " +
            "else if (alvo.getElemento().equals(\"fogo\")) { mago.selecionarForma(\"hidromante\"); } " +
            "else if (alvo.getElemento().equals(\"água\")) { mago.selecionarForma(\"eletromante\"); } " +
            "else { mago.selecionarForma(\"neutro\"); } mago.lancarMagia(alvo);";

        [TestCase("gelo", "piromante", CombatElement.Fire)]
        [TestCase("fogo", "hidromante", CombatElement.Water)]
        [TestCase("água", "eletromante", CombatElement.Electric)]
        [TestCase("neutro", "neutro", CombatElement.Neutral)]
        [TestCase("desconhecido", "neutro", CombatElement.Neutral)]
        public void Compile_ConditionalStrategySelectsTheAuthoredForm(
            string targetElement, string expectedForm, CombatElement expectedSpell)
        {
            object strategy = Compile(CompleteStrategy, true);

            Assert.That(Invoke<string>(strategy, "FormFor", targetElement), Is.EqualTo(expectedForm));
            Assert.That(Invoke<CombatElement>(strategy, "SpellFor", targetElement), Is.EqualTo(expectedSpell));
        }

        [TestCase("if (alvo.getElemento().equals(\"gelo\")) { mago.selecionarForma(\"piromante\"); } mago.lancarMagia(alvo);")]
        [TestCase("else { mago.selecionarForma(\"neutro\"); } mago.lancarMagia(alvo);")]
        [TestCase("if (alvo.getElemento().equals(\"gelo\")) { mago.selecionarForma(\"inventado\"); } else { mago.selecionarForma(\"neutro\"); } mago.lancarMagia(alvo);")]
        [TestCase("if (alvo.getElemento().equals(\"gelo\")) { alvo.setVida(0); } else { mago.selecionarForma(\"neutro\"); } mago.lancarMagia(alvo);")]
        public void Compile_RejectsIncompleteOrUnsafeStrategy(string source)
        {
            Compile(source, false);
        }

        private static object Compile(string source, bool expectedSuccess)
        {
            Type compiler = typeof(CodeBlockDocument).Assembly.GetType("PrograMago.Application.CombatCodeCompiler");
            MethodInfo method = compiler?.GetMethod("TryCompileStrategy", BindingFlags.Public | BindingFlags.Static);
            Assert.That(method, Is.Not.Null,
                "O compilador precisa aceitar a estratégia condicional escrita pelo jogador.");

            object[] arguments = { source, "mago", null, null };
            bool success = (bool)method.Invoke(null, arguments);
            Assert.That(success, Is.EqualTo(expectedSuccess), arguments[3] as string);
            return success ? arguments[2] : null;
        }

        private static T Invoke<T>(object target, string name, string value)
        {
            MethodInfo method = target.GetType().GetMethod(name, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(method, Is.Not.Null, "Estratégia compilada precisa expor " + name + ".");
            return (T)method.Invoke(target, new object[] { value });
        }
    }
}
