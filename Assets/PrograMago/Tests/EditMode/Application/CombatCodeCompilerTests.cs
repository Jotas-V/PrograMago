using System;
using System.Reflection;
using NUnit.Framework;
using PrograMago.Application;
using PrograMago.Domain;

namespace PrograMago.Tests.Application
{
    public sealed class CombatCodeCompilerTests
    {
        [Test]
        public void Compile_PreservesCommandsAndTheirSourceBlocks()
        {
            object[] args = Compile(new[] { "analisarAlvo();", "selecionarMagia(); lancarMagia();", "lancarMagia();" });
            Assert.That(args[1], Is.EqualTo(new[] { CombatAction.AnalyzeTarget, CombatAction.SelectSpell, CombatAction.Attack, CombatAction.Attack }));
            Assert.That(args[2], Is.EqualTo(new[] { 0, 1, 1, 2 }));
            Assert.That(args[3], Is.EqualTo(-1));
        }

        [TestCase("destruirTudo();")]
        [TestCase("lancarMagia()")]
        [TestCase("lancarMagia(123);")]
        public void Compile_RejectsUnsupportedCodeInItsOriginalBlock(string invalid)
        {
            object[] args = Compile(new[] { "analisarAlvo();", invalid }, false);
            Assert.That(args[3], Is.EqualTo(1));
            Assert.That(args[4], Is.Not.Empty);
        }

        [Test]
        public void Compile_CommentsAndWhitespaceDoNotChangeExecution()
        {
            object[] args = Compile(new[] { "// alvo\nanalisarAlvo ( );", "selecionarMagia();\nlancarMagia();" });
            Assert.That(args[1], Has.Length.EqualTo(3));
        }

        private static object[] Compile(string[] blocks, bool expectedSuccess = true)
        {
            Type type = typeof(CodeBlockDocument).Assembly.GetType("PrograMago.Application.CombatCodeCompiler");
            Assert.That(type, Is.Not.Null, "O código dos blocos precisa ser compilado para ações reais.");
            object[] args = { blocks, null, null, -1, null };
            bool success = (bool)type.GetMethod("TryCompile", BindingFlags.Public | BindingFlags.Static).Invoke(null, args);
            Assert.That(success, Is.EqualTo(expectedSuccess));
            return args;
        }
    }
}
