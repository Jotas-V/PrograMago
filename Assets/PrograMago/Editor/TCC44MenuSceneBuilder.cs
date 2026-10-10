using System.Collections.Generic;
using System.Linq;
using PrograMago.UnityIntegration;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace PrograMago.Editor
{
    public static class TCC44MenuSceneBuilder
    {
        private static TMP_FontAsset font;
        private static readonly Color32 Cream = new Color32(244, 229, 186, 255);
        private static readonly Color32 Ink = new Color32(24, 42, 38, 255);
        [MenuItem("PrograMago/TCC-44/Construir menu inicial")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Pare o Play Mode antes de construir o menu.");
            const string path = "Assets/Scenes/MenuScene.unity";
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            var camera = new GameObject("MenuCamera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Ink;
            var view = new GameObject("MainMenu").AddComponent<MainMenuView>();
            var root = new GameObject("MenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            var background = Rect("MenuForest", root.transform, Vector2.zero, Vector2.one);
            var bg = Graphic(background, Color.white);
            bg.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/PrograMago/Resources/Arena/ForestArena.png");
            var aspect = background.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            aspect.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.EnvelopeParent;
            aspect.aspectRatio = bg.sprite.rect.width / bg.sprite.rect.height;
            Graphic(Rect("ForestShade", root.transform, Vector2.zero, Vector2.one), new Color32(13, 30, 29, 70));

            // Keep all duel actors in one proportional illustration on the right.
            var illustration = Rect("MenuIllustration", root.transform, new Vector2(.42f, .12f), new Vector2(.99f, .72f));
            var duel = Rect("MenuDuel", illustration, Vector2.zero, Vector2.one);
            var duelAspect = duel.gameObject.AddComponent<UnityEngine.UI.AspectRatioFitter>();
            duelAspect.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent; duelAspect.aspectRatio = 1.62f;
            var wizard = Actor("MenuWizard", duel, new Vector2(.02f, .1f), new Vector2(.4f, .96f));
            var dummy = Actor("MenuTrainingDummy", duel, new Vector2(.71f, .1f), new Vector2(.96f, .79f));
            var catalog = AssetDatabase.LoadAssetAtPath<TCC40VisualCatalog>("Assets/PrograMago/Resources/Visuals/TCC40VisualCatalog.asset");
            dummy.sprite = catalog.trainingDummyIdleSprite;
            var projectile = Actor("MenuSpell", duel, new Vector2(.31f, .31f), new Vector2(.4f, .44f));
            projectile.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/PrograMago/Art/Combat/NeutralProjectile-PixelArt.png");
            var animation = duel.gameObject.AddComponent<MenuDuelAnimation>();
            var anim = new SerializedObject(animation);
            Set(anim, "wizard", wizard); Set(anim, "dummy", dummy); Set(anim, "projectile", projectile);
            var idle = Frames("Idle"); wizard.sprite = idle[0];
            Array(anim, "idleFrames", idle); Array(anim, "attackFrames", Frames("Attack")); anim.ApplyModifiedPropertiesWithoutUndo();

            var rail = Rect("MenuRail", root.transform, new Vector2(0, 0), new Vector2(.42f, 1));
            Graphic(rail, new Color32(20, 37, 34, 231));
            Graphic(Rect("RailAccent", rail, new Vector2(.97f, .08f), new Vector2(.975f, .92f)), new Color32(188, 165, 105, 130));
            var title = Text("GameTitle", rail, "PrograMago", new Vector2(.09f, .73f), new Vector2(.92f, .90f), 75, Cream);
            title.fontStyle = FontStyles.Bold; title.enableAutoSizing = true; title.fontSizeMin = 38; title.fontSizeMax = 75;
            Text("GameSubtitle", rail, "Sua magia começa com uma linha de código.", new Vector2(.1f, .65f), new Vector2(.91f, .74f), 24, new Color32(193, 210, 177, 255));
            var play = Button("PlayMenuButton", rail, "Jogar", new Vector2(.12f, .48f), new Vector2(.84f, .57f), true);
            var options = Button("OptionsMenuButton", rail, "Opções", new Vector2(.12f, .36f), new Vector2(.84f, .45f));
            var controls = Button("ControlsMenuButton", rail, "Controles", new Vector2(.12f, .24f), new Vector2(.84f, .33f));
            var quit = Button("QuitMenuButton", rail, "Sair", new Vector2(.12f, .12f), new Vector2(.84f, .21f));
            Text("MenuFootnote", root.transform, "Aprenda. Experimente. Lance sua magia.", new Vector2(.45f, .06f), new Vector2(.98f, .13f), 23, Cream);
            var notice = Text("MenuNotice", rail, "", new Vector2(.05f, .01f), new Vector2(.94f, .1f), 20, Cream);
            var close = new List<UnityEngine.UI.Button>();
            var journey = Panel("JourneyPanel", root.transform, "Sua jornada", "Continue de onde parou ou comece uma nova aventura.");
            var resume = Button("ContinueJourneyButton", journey, "Continuar", new Vector2(.12f, .37f), new Vector2(.88f, .48f), true);
            var fresh = Button("NewJourneyButton", journey, "Novo jogo", new Vector2(.12f, .22f), new Vector2(.88f, .33f));
            close.Add(Button("CloseJourneyButton", journey, "Voltar", new Vector2(.32f, .06f), new Vector2(.68f, .16f)));
            var reset = Panel("ResetJourneyPanel", root.transform, "Começar de novo?", "Seu progresso e o código salvo serão substituídos.\nEssa ação não pode ser desfeita.");
            var confirm = Button("ConfirmNewJourneyButton", reset, "Sim, iniciar novo jogo", new Vector2(.12f, .3f), new Vector2(.88f, .43f));
            close.Add(Button("CancelResetButton", reset, "Manter minha jornada", new Vector2(.12f, .13f), new Vector2(.88f, .26f), true));
            var settings = Panel("OptionsPanel", root.transform, "Opções", "Ajuste o volume da sua aventura.");
            var master = Volume("MasterVolume", settings, "Volume geral", .51f);
            var music = Volume("MusicVolume", settings, "Música", .35f);
            var effects = Volume("EffectsVolume", settings, "Efeitos", .19f);
            close.Add(Button("CloseOptionsButton", settings, "Voltar", new Vector2(.32f, .035f), new Vector2(.68f, .13f)));
            var help = Panel("ControlsPanel", root.transform, "Controles", "");
            var controlsText = Text("ControlsText", help,
                "<b>Clique</b> · escolha uma fase ou um botão. No mapa, o mago caminha até a fase e entra automaticamente.\n\n" +
                "<b>M</b> · abre ou fecha o mapa fora da digitação no editor de código. Você também pode usar Mapa [M].\n\n" +
                "<b>R por 5 segundos</b> · reinicia a jornada. Evite segurar a tecla enquanto escreve código.\n\n" +
                "<b>Blocos 1, 2 e 3</b> · clique para alternar os documentos de código. Ajustes redistribui atributos.\n\n" +
                "<b>Validar / Batalhar</b> · confere o código e inicia a atividade. Durante o combate, use os botões Pausar e Tentar novamente.\n\n" +
                "<b>Esc</b> · fecha os painéis deste menu.",
                new Vector2(.08f, .19f), new Vector2(.92f, .78f), 24, Cream, TextAlignmentOptions.Left);
            controlsText.enableAutoSizing = true; controlsText.fontSizeMin = 20; controlsText.fontSizeMax = 24;
            close.Add(Button("CloseControlsButton", help, "Voltar", new Vector2(.32f, .04f), new Vector2(.68f, .14f)));
            var serialized = new SerializedObject(view);
            Set(serialized, "learningPath", AssetDatabase.LoadAssetAtPath<LearningPathAsset>("Assets/PrograMago/Content/LearningPath.asset"));
            Set(serialized, "journeyPanel", journey.parent.gameObject); Set(serialized, "resetPanel", reset.parent.gameObject);
            Set(serialized, "optionsPanel", settings.parent.gameObject); Set(serialized, "controlsPanel", help.parent.gameObject);
            Set(serialized, "playButton", play); Set(serialized, "optionsButton", options); Set(serialized, "controlsButton", controls); Set(serialized, "quitButton", quit);
            Set(serialized, "continueButton", resume); Set(serialized, "newButton", fresh); Set(serialized, "confirmButton", confirm);
            Set(serialized, "masterVolume", master); Set(serialized, "musicVolume", music); Set(serialized, "effectsVolume", effects); Set(serialized, "notice", notice);
            Array(serialized, "closeButtons", close.ToArray()); serialized.ApplyModifiedPropertiesWithoutUndo();
            new GameObject("MenuEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            EditorSceneManager.SaveScene(scene, path); SceneManager.SetActiveScene(previous); EditorSceneManager.CloseScene(scene, true);
            AddMapReturnButton();
            var scenes = EditorBuildSettings.scenes.Where(item => item.path != path).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(path, true)); EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
        }
        private static void AddMapReturnButton()
        {
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MapScene.unity", OpenSceneMode.Additive);
            var map = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<LevelMapView>(true)).First();
            var header = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<RectTransform>(true)).First(item => item.name == "MapHeader");
            var existing = header.Find("ReturnToMenuButton");
            var button = existing != null ? existing.GetComponent<UnityEngine.UI.Button>() : Button("ReturnToMenuButton", header, "Menu", new Vector2(.61f, .20f), new Vector2(.74f, .80f));
            var serialized = new SerializedObject(map); Set(serialized, "menuButton", button); serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene); SceneManager.SetActiveScene(previous); EditorSceneManager.CloseScene(scene, true);
        }
        private static Sprite[] Frames(string state)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/PrograMago/Art/Characters/Animations/Mago-" + state + ".anim");
            var binding = AnimationUtility.GetObjectReferenceCurveBindings(clip).First(item => item.propertyName == "m_Sprite");
            return AnimationUtility.GetObjectReferenceCurve(clip, binding).Select(key => (Sprite)key.value).ToArray();
        }
        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        private static UnityEngine.UI.Image Graphic(RectTransform rect, Color color, bool raycast = false)
        {
            var graphic = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); graphic.color = color; graphic.raycastTarget = raycast; return graphic;
        }
        private static UnityEngine.UI.Image Actor(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var actor = Graphic(Rect(name, parent, min, max), Color.white); actor.preserveAspect = true; return actor;
        }
        private static TMP_Text Text(string name, Transform parent, string value, Vector2 min, Vector2 max, float size, Color color, TextAlignmentOptions align = TextAlignmentOptions.Center)
        {
            var text = Rect(name, parent, min, max).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font; text.text = value; text.fontSize = size; text.color = color; text.alignment = align;
            text.textWrappingMode = TextWrappingModes.Normal; text.overflowMode = TextOverflowModes.Ellipsis;
            text.raycastTarget = false; return text;
        }
        private static UnityEngine.UI.Button Button(string name, Transform parent, string label, Vector2 min, Vector2 max, bool primary = false)
        {
            var rect = Rect(name, parent, min, max); Graphic(rect, new Color32(182, 158, 102, 255), true);
            var inner = Rect("ButtonFill", rect, Vector2.zero, Vector2.one); inner.offsetMin = new Vector2(4, 4); inner.offsetMax = new Vector2(-4, -4);
            var image = Graphic(inner, primary ? new Color32(62, 101, 75, 255) : new Color32(71, 57, 43, 255));
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image;
            Text("Label", rect, label, new Vector2(.02f, .05f), new Vector2(.98f, .95f), 30, Cream);
            rect.gameObject.AddComponent<ButtonJuice>(); return button;
        }
        private static RectTransform Panel(string name, Transform parent, string title, string description)
        {
            var blocker = Rect(name, parent, Vector2.zero, Vector2.one); Graphic(blocker, new Color32(8, 20, 20, 216), true);
            var card = Rect("Card", blocker, new Vector2(.25f, .13f), new Vector2(.75f, .87f)); Graphic(card, new Color32(178, 150, 91, 255), true);
            var fill = Rect("PanelFill", card, Vector2.zero, Vector2.one); fill.offsetMin = new Vector2(5, 5); fill.offsetMax = new Vector2(-5, -5); Graphic(fill, Ink);
            Text("PanelTitle", card, title, new Vector2(.08f, .80f), new Vector2(.92f, .96f), 45, Cream);
            Text("PanelDescription", card, description, new Vector2(.1f, .57f), new Vector2(.9f, .79f), 27, Cream);
            blocker.gameObject.SetActive(false); return card;
        }
        private static UnityEngine.UI.Slider Volume(string name, Transform parent, string label, float y)
        {
            Text(name + "Label", parent, label, new Vector2(.1f, y + .065f), new Vector2(.9f, y + .13f), 27, Cream, TextAlignmentOptions.Left);
            var rect = Rect(name, parent, new Vector2(.1f, y), new Vector2(.9f, y + .05f));
            var track = Rect("Track", rect, new Vector2(0, .3f), new Vector2(1, .7f)); Graphic(track, new Color32(80, 91, 70, 255), true);
            var fillArea = Rect("FillArea", rect, Vector2.zero, Vector2.one);
            var fill = Rect("Fill", fillArea, Vector2.zero, Vector2.one); Graphic(fill, new Color32(193, 167, 103, 255));
            var handles = Rect("HandleArea", rect, Vector2.zero, Vector2.one);
            var handle = Rect("Handle", handles, Vector2.zero, Vector2.one); handle.sizeDelta = new Vector2(24, 0);
            var handleImage = Graphic(handle, Cream, true);
            var slider = rect.gameObject.AddComponent<UnityEngine.UI.Slider>(); slider.fillRect = fill; slider.handleRect = handle;
            slider.targetGraphic = handleImage; slider.minValue = 0; slider.maxValue = 1; slider.value = 1; return slider;
        }
        private static void Set(SerializedObject obj, string name, Object value) => obj.FindProperty(name).objectReferenceValue = value;
        private static void Array<T>(SerializedObject obj, string name, T[] values) where T : Object
        {
            var array = obj.FindProperty(name); array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
