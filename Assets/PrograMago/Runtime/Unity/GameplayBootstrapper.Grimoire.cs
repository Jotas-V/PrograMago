using System.Collections;
using PrograMago.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PrograMago.UnityIntegration
{
    public sealed partial class GameplayBootstrapper
    {
        [SerializeField] private Button openGrimoireButton;
        private bool grimoireTransition;
        private bool grimoirePausedCombat;
        public bool IsGrimoireActive { get; private set; }
        public GrimoireView Grimoire { get; private set; }

        private static readonly string[] GrimoireTasks = {
            "Declare uma classe pública chamada Mago, com corpo vazio.",
            "Adicione os cinco atributos privados do Mago.",
            "Crie o construtor e uma instância do Mago. Use até 25 pontos.",
            "Crie os cinco setters dentro da classe Mago.",
            "Crie a classe Inimigo e o Boneco de Treinamento. Vença a batalha.",
            "Declare a magia no Mago e faça o objeto chamar esse método.",
            "Crie Piromante como uma especialização de Mago.",
            "Sobrescreva a magia do Piromante e enfrente o Golem de Gelo.",
            "Escolha a forma do Mago conforme o elemento de cada alvo." };

        private void InitializeGrimoireNavigation()
        {
            if (openGrimoireButton != null) openGrimoireButton.onClick.AddListener(() => OpenGrimoire());
            LayoutGrimoireButton();
        }

        private void LayoutGrimoireButton()
        {
            if (openGrimoireButton == null) return;
            var buttonRect = openGrimoireButton.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(.10f, .18f);
            buttonRect.anchorMax = new Vector2(.90f, .65f);
            buttonRect.offsetMin = buttonRect.offsetMax = Vector2.zero;
            // Keep a generous invisible hit area instead of framing the book as a paper button.
            openGrimoireButton.transition = Selectable.Transition.None;
            foreach (var border in openGrimoireButton.GetComponentsInChildren<PaperPanelBorder>(true))
                border.enabled = false;
            foreach (var image in openGrimoireButton.GetComponentsInChildren<Image>(true))
                if (image.name != "GrimoireBookIcon") image.color = Color.clear;
            var icon = openGrimoireButton.transform.Find("GrimoireBookIcon") as RectTransform;
            if (icon != null) {
                icon.anchorMin = new Vector2(0, .09f);
                icon.anchorMax = new Vector2(1, 1.05f);
                icon.offsetMin = icon.offsetMax = new Vector2(0, -30);
                // The visible book remains clickable where it extends above the parent rectangle.
                icon.GetComponent<Image>().raycastTarget = true;
            }
            var orbit = openGrimoireButton.transform.Find("GrimoireMagicCircle") as RectTransform;
            if (orbit != null) {
                orbit.anchorMin = new Vector2(-.06f, .08f);
                orbit.anchorMax = new Vector2(1.06f, 1.07f);
                orbit.offsetMin = orbit.offsetMax = new Vector2(0, -30);
            }
            feedbackText.rectTransform.anchorMin = new Vector2(.06f, .02f);
            feedbackText.rectTransform.anchorMax = new Vector2(.94f, .17f);
            feedbackText.rectTransform.offsetMin = feedbackText.rectTransform.offsetMax = Vector2.zero;
            feedbackText.rectTransform.localScale = Vector3.one;
            feedbackText.enableAutoSizing = true;
            feedbackText.fontSizeMin = 18; feedbackText.fontSizeMax = 22;
            var label = openGrimoireButton.GetComponentInChildren<TMP_Text>();
            label.rectTransform.anchorMin = new Vector2(.05f, .03f);
            label.rectTransform.anchorMax = new Vector2(.95f, .18f);
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = new Vector2(0, -30);
            label.fontSizeMax = 28;
            label.color = new Color32(65, 46, 36, 255);
        }

        private void ShowGrimoireTask(int index)
        {
            if (openGrimoireButton == null) return;
            lessonText.gameObject.SetActive(false);
            hintText.gameObject.SetActive(false);
            enemyGuide.SetActive(false);
            objectiveText.gameObject.SetActive(true);
            objectiveText.text = "<b>SUA TAREFA</b>\n" + GrimoireTasks[index] + "\n\nConsulte o grimório para aprender e ver os passos.";
        }

        public bool OpenGrimoire()
        {
            if (learningFlowPresenter == null || IsMapActive || mapTransition || IsGrimoireActive || grimoireTransition)
                return false;
            if (!UnityEngine.Application.CanStreamedLevelBeLoaded(GrimoireView.SceneName)) {
                Debug.LogError("GrimoireScene precisa estar nas cenas da build."); return false;
            }
            SaveProgress();
            codeInput.DeactivateInputField();
            IsGrimoireActive = grimoireTransition = true;
            grimoirePausedCombat = combat != null && combat.Outcome == CombatOutcome.InProgress && !combat.IsPaused;
            if (grimoirePausedCombat) combat.Pause();
            HideGameplayForMap();
            StartCoroutine(OpenGrimoireScene());
            return true;
        }

        private IEnumerator OpenGrimoireScene()
        {
            yield return SceneManager.LoadSceneAsync(GrimoireView.SceneName, LoadSceneMode.Additive);
            var scene = SceneManager.GetSceneByName(GrimoireView.SceneName);
            foreach (var root in scene.GetRootGameObjects()) {
                Grimoire = root.GetComponentInChildren<GrimoireView>();
                if (Grimoire != null) break;
            }
            if (Grimoire == null) {
                Debug.LogError("GrimoireScene precisa de GrimoireView configurado.");
                yield return CloseGrimoireScene(); yield break;
            }
            SceneManager.SetActiveScene(scene);
            Grimoire.Configure(learningFlowPresenter.Progress, () => {
                if (!grimoireTransition) StartCoroutine(CloseGrimoireScene());
            }, SaveProgress);
            grimoireTransition = false;
        }

        private IEnumerator CloseGrimoireScene()
        {
            grimoireTransition = true;
            SaveProgress();
            SceneManager.SetActiveScene(gameObject.scene);
            var scene = SceneManager.GetSceneByName(GrimoireView.SceneName);
            if (scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            foreach (var item in mapHiddenBehaviours) if (item != null) item.enabled = true;
            mapHiddenBehaviours.Clear();
            if (grimoirePausedCombat && combat != null && combat.Outcome == CombatOutcome.InProgress) combat.Resume();
            Grimoire = null;
            grimoirePausedCombat = IsGrimoireActive = grimoireTransition = false;
        }

#if UNITY_EDITOR
        public void PrepareGrimoirePresentation()
        {
            if (UnityEngine.Application.isPlaying) return;
            if (openGrimoireButton == null) {
                openGrimoireButton = CreateButton("OpenGrimoireButton", titleText.transform.parent,
                    "Abrir Grimório", new Vector2(.10f, .10f), new Vector2(.90f, .55f));
                var label = openGrimoireButton.GetComponentInChildren<TMP_Text>();
                label.rectTransform.anchorMin = new Vector2(.05f, .03f);
                label.rectTransform.anchorMax = new Vector2(.95f, .23f);
                label.fontSizeMax = 28;
                var icon = new GameObject("GrimoireBookIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                icon.transform.SetParent(openGrimoireButton.transform, false);
                var rect = icon.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(.15f, .27f); rect.anchorMax = new Vector2(.85f, .94f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
            }
            var bookIcon = openGrimoireButton.transform.Find("GrimoireBookIcon");
            var bookImage = bookIcon.GetComponent<Image>();
            if (bookImage == null) bookImage = bookIcon.gameObject.AddComponent<Image>();
            bookImage.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/PrograMago/Art/UI/Grimoire/Grimoire-Closed-PixelArt.png");
            bookImage.color = Color.white; bookImage.preserveAspect = true; bookImage.raycastTarget = false;
            if (bookIcon.GetComponent<GrimoireBookIdle>() == null) bookIcon.gameObject.AddComponent<GrimoireBookIdle>();
            if (openGrimoireButton.transform.Find("GrimoireMagicCircle") == null) {
                var orbit = new GameObject("GrimoireMagicCircle", typeof(RectTransform), typeof(CanvasRenderer), typeof(GrimoireMagicOrbit));
                orbit.transform.SetParent(openGrimoireButton.transform, false);
                var rect = orbit.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(.02f, .24f); rect.anchorMax = new Vector2(.98f, .97f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                orbit.GetComponent<GrimoireMagicOrbit>().raycastTarget = false;
            }
            LayoutGrimoireButton();
            objectiveText.rectTransform.anchorMin = new Vector2(.06f, .57f);
            objectiveText.rectTransform.anchorMax = new Vector2(.94f, .86f);
            objectiveText.rectTransform.offsetMin = objectiveText.rectTransform.offsetMax = Vector2.zero;
            objectiveText.fontSizeMin = objectiveText.fontSizeMax = 22;
            var path = learningPath.ToDomain();
            ShowBattle(path.Battles[0], 1, path.Battles.Count);
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }
}
