using NUnit.Framework;
using PrograMago.Application;

namespace PrograMago.Tests.Application
{
    public sealed class MagoSourceInsertionTests
    {
        private const string ApprovedSource =
            "public class Mago {\n" +
            "    private int vida;\n" +
            "    public Mago(int vida) { this.vida = vida; }\n" +
            "}\n" +
            "Mago mago = new Mago(5);";

        [Test]
        public void TryExtract_AppendBeforeClassClose_PreservesApprovedCodeAndInstance()
        {
            const string addition = "\n    public void setVida(int valor) { this.vida = valor; }\n";
            int close = ApprovedSource.IndexOf("}\nMago", System.StringComparison.Ordinal);
            string draft = ApprovedSource.Insert(close, addition);

            bool accepted = MagoSourceInsertion.TryExtract(ApprovedSource, draft,
                out string inserted, out string error);

            Assert.That(accepted, Is.True, error);
            Assert.That(inserted, Is.EqualTo(addition));
        }

        [TestCase("this.vida = vida;", "this.vida = dano;")]
        [TestCase("new Mago(5)", "new Mago(4)")]
        public void TryExtract_EditingApprovedText_RejectsDraft(string oldText, string replacement)
        {
            string draft = ApprovedSource.Replace(oldText, replacement);

            bool accepted = MagoSourceInsertion.TryExtract(ApprovedSource, draft,
                out _, out string error);

            Assert.That(accepted, Is.False);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void TryExtract_AppendAfterClass_RejectsDraft()
        {
            string draft = ApprovedSource + "\npublic void setVida(int valor) {}";

            bool accepted = MagoSourceInsertion.TryExtract(ApprovedSource, draft,
                out _, out string error);

            Assert.That(accepted, Is.False);
            Assert.That(error, Does.Contain("dentro da classe"));
        }
    }
}
