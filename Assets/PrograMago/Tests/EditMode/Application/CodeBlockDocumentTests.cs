using System;
using NUnit.Framework;
using PrograMago.Application;

namespace PrograMago.Tests.Application
{
    public sealed class CodeBlockDocumentTests
    {
        [Test]
        public void SwitchingBlocks_PreservesHiddenTextAndComposesBothSources()
        {
            Type type = typeof(SubmitCodeUseCase).Assembly.GetType(
                "PrograMago.Application.CodeBlockDocument");
            Assert.That(type, Is.Not.Null);

            object document = Activator.CreateInstance(type);
            type.GetMethod("SetActiveText").Invoke(document, new object[] { "public class Inimigo {}" });
            type.GetMethod("Select").Invoke(document, new object[] { 1 });
            type.GetMethod("SetActiveText").Invoke(document, new object[] { "public class Mago {}" });
            type.GetMethod("Select").Invoke(document, new object[] { 0 });

            Assert.That(type.GetProperty("ActiveText").GetValue(document),
                Is.EqualTo("public class Inimigo {}"));
            Assert.That(type.GetProperty("SourceCode").GetValue(document),
                Is.EqualTo("public class Inimigo {}\npublic class Mago {}"));
        }

        [Test]
        public void SnapshotAndRestore_KeepThreeIndependentTextsAndActiveSelection()
        {
            var original = new CodeBlockDocument();
            original.SetActiveText("classe");
            original.Select(1);
            original.SetActiveText("instância");
            original.Select(2);
            original.SetActiveText("batalha");
            string[] snapshot = original.Snapshot();
            snapshot[0] = "alterado";
            Assert.That(original.SourceCode, Is.EqualTo("classe\ninstância\nbatalha"));

            var restored = new CodeBlockDocument();
            restored.Restore(original.Snapshot(), 2);
            Assert.That(restored.ActiveIndex, Is.EqualTo(2));
            Assert.That(restored.ActiveText, Is.EqualTo("batalha"));
            restored.Select(0);
            Assert.That(restored.ActiveText, Is.EqualTo("classe"));
        }

        [Test]
        public void Locate_MapsCombinedOffsetToItsBlockAndLocalLine()
        {
            var document = new CodeBlockDocument();
            document.SetActiveText("public class Mago {}\n");
            document.Select(1);
            document.SetActiveText("\n#");
            int errorOffset = document.SourceCode.IndexOf('#');

            CodeBlockLocation location = document.Locate(errorOffset);

            Assert.That(location.BlockNumber, Is.EqualTo(2));
            Assert.That(location.Line, Is.EqualTo(2));
            Assert.That(location.Column, Is.EqualTo(1));
        }

        [Test]
        public void InvalidBlockSelection_IsRejectedWithoutChangingActiveText()
        {
            var document = new CodeBlockDocument();
            document.SetActiveText("Mago");

            Assert.Throws<ArgumentOutOfRangeException>(() => document.Select(3));
            Assert.That(document.ActiveIndex, Is.Zero);
            Assert.That(document.ActiveText, Is.EqualTo("Mago"));
        }
    }
}
