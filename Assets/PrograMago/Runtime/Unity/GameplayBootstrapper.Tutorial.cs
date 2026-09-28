using TMPro;
using UnityEngine;

namespace PrograMago.UnityIntegration
{
    public sealed partial class GameplayBootstrapper
    {
        [SerializeField] private GameObject enemyGuide;
        [SerializeField] private TMP_Text tutorialStepText;
        [SerializeField] private TMP_Text tutorialBodyText;
        private int tutorialStep;
        private static readonly string[] TutorialTitles = {
            "1/6 · Ajuste seu Mago", "2/6 · Declare o inimigo", "3/6 · Guarde os valores",
            "4/6 · Consulte o elemento", "5/6 · Crie o Boneco", "6/6 · Inicie a batalha" };
        private static readonly string[] TutorialBodies = {
            "<b>Objetivo: derrotar o Boneco na casa 16.</b>\n\nO Mago começa na casa 1. Qualquer alcance válido funciona: se o alvo estiver longe, o Mago avança <b>uma casa por turno</b> até poder atacar. O Boneco não anda nem ataca.\n\nOs cinco atributos vêm do seu código, com até <b>25 pontos</b> no total. Exemplo válido:\n• vida = 5\n• dano = 5\n• alcance = 5\n• iniciativa = 5\n• velocidadeAtaque = 5\n\nAjuste os valores de new Mago(...) seguindo a ordem dos parâmetros do <b>seu construtor</b>. Não apague a classe.\n\nUse um bloco vazio para escrever Inimigo nas próximas etapas.",
            "<b>No bloco 2, abra a classe e declare seus atributos:</b>\n\npublic class Inimigo {\n    private String nome;\n    private int vida;\n    private String elemento;\n}\n\nNome e elemento são textos (String). Vida é um número inteiro (int).\n\nNas próximas etapas, o construtor e o método entram <b>dentro das chaves desta classe</b>.",
            "<b>Dentro da classe Inimigo, depois dos atributos:</b>\n\npublic Inimigo(String nome,\n    int vida, String elemento) {\n    this.nome = nome;\n    this.vida = vida;\n    this.elemento = elemento;\n}\n\nO construtor recebe os valores de cada objeto. this.nome é o atributo do objeto; nome é o parâmetro recebido.\n\nAinda mantenha uma chave final para fechar a classe.",
            "<b>Depois do construtor, ainda dentro da classe:</b>\n\npublic String getElemento() {\n    return elemento;\n}\n\nO método devolve o elemento guardado no inimigo. Como o atributo é privado, outros objetos o consultam por esse método.\n\nDepois dele, feche a classe Inimigo com }.",
            "<b>Fora da classe, depois da última chave:</b>\n\nInimigo boneco = new Inimigo(\n    \"Boneco de Treinamento\",\n    10, \"neutro\");\n\nEscreva o nome exatamente assim, com 10 de vida e elemento neutro.\n\n<b>Conferência</b>\n• Bloco Mago completo\n• Bloco Inimigo completo\n• Cada classe fechada com }\n• Instâncias fora das classes\n\nSe precisar comparar, copie o exemplo completo pelo botão abaixo. Ele contém apenas Inimigo.",
            "<b>A faixa inferior reúne os seletores e o comando de batalha.</b>\n\nUse <b>Mago</b> e <b>Inimigo</b> para editar cada classe. Os cinco setters ficam dentro da classe Mago; em <b>Ajustes</b>, chame setVida(...), setDano(...), setAlcance(...), setIniciativa(...) ou setVelocidadeAtaque(...) para redistribuir pontos antes da batalha. Se a soma passar de <b>25</b>, ajuste os outros valores também.\n\nPara o Boneco, <b>Estratégia</b> ainda não é necessária: ele é neutro e fica parado. Quando inimigos elementais e formas do Mago forem ensinados, esse bloco será usado para escolher a forma com if/else. <b>Atacar</b> se repete automaticamente; não é preciso arrastar nem ordenar blocos.\n\nA arena tem 16 casas: o Mago começa na 1 e o Boneco na 16. Fora do alcance, o Mago avança <b>uma casa por turno</b> até atacar. Inimigos móveis avançam para entrar no próprio alcance. <b>Pausar</b> congela a simulação; em caso de derrota, tente novamente ou ajuste sua build."
        };
        public const string EnemyTutorialExample =
            "public class Inimigo {\n    private String nome;\n    private int vida;\n    private String elemento;\n\n" +
            "    public Inimigo(String nome, int vida, String elemento) {\n        this.nome = nome;\n        this.vida = vida;\n        this.elemento = elemento;\n    }\n\n" +
            "    public String getElemento() {\n        return elemento;\n    }\n}\n\n" +
            "Inimigo boneco = new Inimigo(\"Boneco de Treinamento\", 10, \"neutro\");";

        private void CreateEnemyGuide()
        {
            RectTransform panel = CreatePanel("EnemyGuidePanel", titleText.transform.parent,
                Vector2.zero, Vector2.one, new Color32(244, 246, 252, 255));
            panel.offsetMin = new Vector2(12, 135);
            panel.offsetMax = new Vector2(-12, -104);
            enemyGuide = panel.gameObject;
            tutorialStepText = CreateLabel("TutorialStepText", panel, "", new Vector2(0.03f, 0.88f), new Vector2(0.97f, 1));
            tutorialStepText.color = new Color32(67, 47, 130, 255);
            tutorialStepText.fontStyle = FontStyles.Bold;
            tutorialStepText.fontSizeMax = 23;
            tutorialBodyText = CreateLabel("TutorialBodyText", panel, "", new Vector2(0.04f, 0.18f), new Vector2(0.96f, 0.88f));
            tutorialBodyText.alignment = TextAlignmentOptions.TopLeft;
            tutorialBodyText.color = new Color32(31, 34, 48, 255);
            tutorialBodyText.fontSizeMax = 20;
            tutorialBodyText.fontSizeMin = 12;
            tutorialPreviousButton = CreateButton("TutorialPreviousButton", panel, "Anterior", new Vector2(0.02f, 0.09f), new Vector2(0.48f, 0.16f));
            tutorialNextButton = CreateButton("TutorialNextButton", panel, "Próximo", new Vector2(0.52f, 0.09f), new Vector2(0.98f, 0.16f));
            copyEnemyExampleButton = CreateButton("CopyEnemyExampleButton", panel, "Copiar exemplo de Inimigo", new Vector2(0.02f, 0), new Vector2(0.98f, 0.07f));
        }

        private void ShowEnemyGuide(bool visible)
        {
            lessonText.gameObject.SetActive(!visible);
            objectiveText.gameObject.SetActive(!visible);
            hintText.gameObject.SetActive(!visible);
            if (enemyGuide != null) enemyGuide.SetActive(visible);
            if (visible) RenderTutorialStep();
        }

        private void RenderTutorialStep()
        {
            tutorialStepText.text = TutorialTitles[tutorialStep];
            tutorialBodyText.text = TutorialBodies[tutorialStep];
        }

        private void FocusTutorialError(string code)
        {
            if (enemyGuide == null || !enemyGuide.activeSelf) return;
            switch (code)
            {
                case "ENEMY001": tutorialStep = SourceCode.Contains("class Inimigo") ? 4 : 1; break;
                case "ENEMY003": case "ENEMY004": tutorialStep = 1; break;
                case "ENEMY005": case "ENEMY006": tutorialStep = 2; break;
                case "ENEMY007": tutorialStep = 3; break;
                case "ENEMY008": case "ENEMY009": case "ENEMY010": tutorialStep = 4; break;
            }
            RenderTutorialStep();
        }
    }
}
