using System.Linq;
using PrograMago.UnityIntegration;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PrograMago.Editor
{
    public static class TCC48GrimoireSceneBuilder
    {
        private static TMP_FontAsset font;
        private static readonly Color Ink = new Color32(65, 46, 36, 255);
        private static readonly Color Paper = new Color32(239, 221, 174, 255);

        [MenuItem("PrograMago/TCC-48/Construir Grimório do Mago")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Pare o Play Mode antes de preparar o grimório.");
            TCC48GrimoireContent.Apply();
            ImportBook("Closed"); ImportBook("Open");
            var original = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            var camera = new GameObject("GrimoireCamera", typeof(Camera)).GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color32(18, 28, 30, 255);
            camera.cullingMask = 0;
            new GameObject("GrimoireEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var root = new GameObject("GrimoireCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            var view = root.AddComponent<GrimoireView>();
            Text("GrimoireTitle", root.transform, "GRIMÓRIO DO MAGO", new Vector2(.08f, .91f), new Vector2(.74f, .985f), 40, Paper);
            var close = Button("CloseGrimoireButton", root.transform, "Fechar [Esc]", new Vector2(.79f, .92f), new Vector2(.95f, .977f));
            var book = Rect("OpenBook", root.transform, new Vector2(.055f, .14f), new Vector2(.945f, .85f));
            Graphic(book, Color.white).sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/PrograMago/Art/UI/Grimoire/Grimoire-Open-PixelArt.png");
            var left = Rect("LeftLeaf", book, new Vector2(.09f, .25f), new Vector2(.455f, .84f));
            var right = Rect("RightLeaf", book, new Vector2(.545f, .25f), new Vector2(.91f, .84f));
            Text("LeftLeafDecoration", left, "APRENDA", new Vector2(.07f, .90f), new Vector2(.93f, .98f), 23, Ink);
            Text("RightLeafDecoration", right, "COLOQUE EM PRÁTICA", new Vector2(.07f, .90f), new Vector2(.93f, .98f), 23, Ink);
            var leftScroll = ScrollText("GrimoireLeftText", left, new Vector2(.075f, .08f), new Vector2(.925f, .89f), out var leftText);
            var rightScroll = ScrollText("GrimoireRightText", right, new Vector2(.075f, .145f), new Vector2(.925f, .89f), out var rightText);
            var hint = Button("GrimoireHintButton", right, "Preciso de uma dica", new Vector2(.075f, .01f), new Vector2(.925f, .12f));
            Text("ReadingHelp", left, "Role para continuar a leitura", new Vector2(.07f, .02f), new Vector2(.93f, .065f), 18, Ink);
            var title = Text("GrimoirePageTitle", root.transform, "Fase 1", new Vector2(.08f, .855f), new Vector2(.92f, .90f), 29, Paper);
            var number = Text("GrimoirePageNumber", root.transform, "Página 1 de 9", new Vector2(.4f, .015f), new Vector2(.6f, .06f), 23, Paper);
            var previous = Button("PreviousGrimoirePage", root.transform, "← Anterior", new Vector2(.055f, .06f), new Vector2(.19f, .12f));
            var next = Button("NextGrimoirePage", root.transform, "Próxima →", new Vector2(.81f, .06f), new Vector2(.945f, .12f));
            var pages = new Button[9];
            for (int i = 0; i < pages.Length; i++) {
                float x = .225f + i * .062f;
                pages[i] = Button("GrimoirePage" + (i + 1), root.transform, (i + 1).ToString(), new Vector2(x, .065f), new Vector2(x + .050f, .115f));
            }
            var serialized = new SerializedObject(view);
            Set(serialized, "book", book); Set(serialized, "pageTitle", title); Set(serialized, "leftText", leftText);
            Set(serialized, "rightText", rightText); Set(serialized, "pageNumber", number);
            Set(serialized, "previousButton", previous); Set(serialized, "nextButton", next);
            Set(serialized, "closeButton", close); Set(serialized, "hintButton", hint);
            PolishBook(root.transform);
            Set(serialized, "leftScroll", leftScroll); Set(serialized, "rightScroll", rightScroll);
            var array = serialized.FindProperty("pageButtons"); array.arraySize = pages.Length;
            for (int i = 0; i < pages.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = pages[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            const string path = "Assets/Scenes/GrimoireScene.unity";
            EditorSceneManager.SaveScene(scene, path);
            EditorSceneManager.CloseScene(scene, true);
            if (original.IsValid() && original.isLoaded) SceneManager.SetActiveScene(original);
            var scenes = EditorBuildSettings.scenes.Where(item => item.path != path).ToList();
            scenes.Add(new EditorBuildSettingsScene(path, true)); EditorBuildSettings.scenes = scenes.ToArray();
            var gameplayScene = SceneManager.GetSceneByPath("Assets/Scenes/MainScene.unity");
            bool loadedHere = !gameplayScene.isLoaded;
            if (loadedHere) gameplayScene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Additive);
            foreach (var item in gameplayScene.GetRootGameObjects()) {
                var game = item.GetComponentInChildren<GameplayBootstrapper>();
                if (game != null) { game.PrepareGrimoirePresentation(); break; }
            }
            EditorSceneManager.SaveScene(gameplayScene);
            if (loadedHere) EditorSceneManager.CloseScene(gameplayScene, true);
            AssetDatabase.SaveAssets();
        }

        private static void ImportBook(string variant)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/PrograMago/Art/UI/Grimoire/Grimoire-" + variant + "-PixelArt.png");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 2048;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.SaveAndReimport();
        }

        [MenuItem("PrograMago/TCC-48/Aplicar polimento")]
        public static void ApplyPolish()
        {
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Pare o Play Mode.");
            var scene = SceneManager.GetSceneByPath("Assets/Scenes/GrimoireScene.unity");
            bool loadedHere = !scene.isLoaded;
            if (loadedHere) scene = EditorSceneManager.OpenScene("Assets/Scenes/GrimoireScene.unity", OpenSceneMode.Additive);
            var canvas = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Canvas>()).Single();
            PolishBook(canvas.transform);
            EditorSceneManager.SaveScene(scene); if (loadedHere) EditorSceneManager.CloseScene(scene, true);
            scene = SceneManager.GetSceneByPath("Assets/Scenes/MainScene.unity");
            loadedHere = !scene.isLoaded;
            if (loadedHere) scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Additive);
            var game = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<GameplayBootstrapper>()).Single();
            game.RefreshTimelineLayout();
            game.PrepareGrimoirePresentation();
            var layout = typeof(GameplayBootstrapper).GetMethod("EnsureWorkspaceControls", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            layout.Invoke(game, null);
            EditorSceneManager.SaveScene(scene); if (loadedHere) EditorSceneManager.CloseScene(scene, true);
        }

        private static void PolishBook(Transform root)
        {
            var book = root.GetComponentsInChildren<RectTransform>(true).Single(t => t.name == "OpenBook");
            var frame = root.Find("OpenBookFrame") as RectTransform;
            if (frame == null) frame = Rect("OpenBookFrame", root, new Vector2(.04f,.085f), new Vector2(.96f,.88f));
            book.SetParent(frame, false);
            var fitter = book.GetComponent<AspectRatioFitter>();
            if (fitter == null) fitter = book.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent; fitter.aspectRatio = 1.5f;
            foreach (string leaf in new[]{"LeftLeaf","RightLeaf"}) {
                var parent = book.Find(leaf);
                string name = leaf == "LeftLeaf" ? "LeftLockedRunes" : "RightLockedRunes";
                if (parent.Find(name) == null) {
                    var runes = Rect(name,parent,new Vector2(.075f,.17f),new Vector2(.925f,.57f));
                    var graphic = runes.gameObject.AddComponent<GrimoireRuneWriting>();
                    graphic.raycastTarget = false; graphic.color = new Color32(135,100,61,205);
                    runes.gameObject.SetActive(false);
                }
            }
            var title = root.Find("GrimoirePageTitle") as RectTransform;
            title.anchorMin = new Vector2(.08f,.885f); title.anchorMax = new Vector2(.92f,.93f);
            title.offsetMin = title.offsetMax = Vector2.zero;
            var heading = root.Find("GrimoireTitle") as RectTransform;
            heading.anchorMin = new Vector2(.08f,.94f); heading.anchorMax = new Vector2(.74f,.995f);
            heading.offsetMin = heading.offsetMax = Vector2.zero;
            foreach (var item in root.GetComponentsInChildren<RectTransform>(true)) {
                if (item.name == "PreviousGrimoirePage" || item.name == "NextGrimoirePage" ||
                    (item.name.StartsWith("GrimoirePage") && item.name != "GrimoirePageTitle" && item.name != "GrimoirePageNumber")) {
                    item.anchorMin = new Vector2(item.anchorMin.x,.03f);
                    item.anchorMax = new Vector2(item.anchorMax.x,.075f);
                }
            }
            var number = root.Find("GrimoirePageNumber") as RectTransform;
            number.anchorMin = new Vector2(.4f,0); number.anchorMax = new Vector2(.6f,.028f);
            number.offsetMin = number.offsetMax = Vector2.zero;
        }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        private static Image Graphic(RectTransform rect, Color color, bool interactive = false)
        {
            var image = rect.gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = interactive; return image;
        }
        private static TMP_Text Text(string name, Transform parent, string value, Vector2 min, Vector2 max, float size, Color color)
        {
            var text = Rect(name, parent, min, max).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font; text.text = value; text.fontSize = size; text.color = color; text.raycastTarget = false;
            text.alignment = TextAlignmentOptions.Center; text.textWrappingMode = TextWrappingModes.Normal; return text;
        }
        private static Button Button(string name, Transform parent, string value, Vector2 min, Vector2 max)
        {
            var rect = Rect(name, parent, min, max); Graphic(rect, new Color32(185, 150, 81, 255), true);
            var inner = Rect("Fill", rect, Vector2.zero, Vector2.one);
            inner.offsetMin = new Vector2(3, 3); inner.offsetMax = new Vector2(-3, -3);
            var image = Graphic(inner, new Color32(68, 49, 47, 255));
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            Text("Label", rect, value, new Vector2(.03f, .03f), new Vector2(.97f, .97f), 24, Paper);
            rect.gameObject.AddComponent<ButtonJuice>(); return button;
        }
        private static ScrollRect ScrollText(string name, Transform parent, Vector2 min, Vector2 max, out TMP_Text text)
        {
            var rect = Rect(name + "Scroll", parent, min, max);
            Graphic(rect, new Color(1, 1, 1, .001f), true);
            var scroll = rect.gameObject.AddComponent<ScrollRect>();
            var viewport = Rect("Viewport", rect, Vector2.zero, Vector2.one);
            viewport.gameObject.AddComponent<RectMask2D>();
            var content = Rect(name, viewport, new Vector2(0, 1), Vector2.one);
            content.pivot = new Vector2(.5f, 1);
            text = content.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font; text.fontSize = 26; text.color = Ink; text.raycastTarget = false;
            text.alignment = TextAlignmentOptions.TopLeft; text.textWrappingMode = TextWrappingModes.Normal;
            text.lineSpacing = 5; text.paragraphSpacing = 5;
            content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 40;
            return scroll;
        }
        private static void Set(SerializedObject target, string name, Object value) => target.FindProperty(name).objectReferenceValue = value;
    }
}
