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
    public static class TCC37MapSceneBuilder
    {
        [MenuItem("PrograMago/TCC-37/Construir cena do mapa")]
        public static void Build()
        {
            const string scenePath = "Assets/Scenes/MapScene.unity";
            const string imagePath = "Assets/PrograMago/Resources/Map/ForestMapBackground.png";
            if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Pare o Play Mode antes de construir a cena.");
            AssetDatabase.ImportAsset(imagePath);
            var importer = (TextureImporter)AssetImporter.GetAtPath(imagePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
            var previous = SceneManager.GetActiveScene();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var view = new GameObject("LevelMap").AddComponent<LevelMapView>();
            var camera = new GameObject("MapCamera", typeof(Camera)).GetComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 6;
            camera.transform.position = new Vector3(-10, 0, -10);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(25, 46, 44, 255); camera.cullingMask = 1 << 5;

            var world = new GameObject("ForestMapCanvas", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.GraphicRaycaster));
            world.layer = 5;
            var canvas = world.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = camera;
            var worldRect = world.GetComponent<RectTransform>();
            worldRect.sizeDelta = new Vector2(3600, 1200); worldRect.localScale = Vector3.one * 0.01f;
            var background = Rect("ForestBackground", world.transform, Vector2.zero, new Vector2(3600, 1200));
            var backgroundImage = background.gameObject.AddComponent<UnityEngine.UI.Image>();
            backgroundImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(imagePath); backgroundImage.raycastTarget = false;
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            var points = LevelMapView.PhasePositions;
            for (int i = 0; i < points.Length - 1; i++) {
                Vector2 direction = points[i + 1] - points[i];
                float length = direction.magnitude;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                var trail = Rect("Trail" + (i + 1), world.transform, (points[i] + points[i + 1]) / 2f, new Vector2(length + 35, 34));
                trail.localRotation = Quaternion.Euler(0, 0, angle); Image(trail, new Color32(77, 71, 43, 210), false);
                for (float along = 20; along < length; along += 38) {
                    var dash = Rect("TrailDash", world.transform, points[i] + direction.normalized * along, new Vector2(16, 5));
                    dash.localRotation = Quaternion.Euler(0, 0, angle); Image(dash, new Color32(218, 200, 132, 240), false);
                }
            }
            var path = AssetDatabase.LoadAssetAtPath<LearningPathAsset>("Assets/PrograMago/Content/LearningPath.asset");
            var battles = path.ToDomain().Battles;
            var buttons = new UnityEngine.UI.Button[9]; var statuses = new TMP_Text[9];
            for (int i = 0; i < 9; i++) {
                var border = Rect("PhaseBorder" + (i + 1), world.transform, points[i], new Vector2(106, 106));
                Image(border, new Color32(30, 40, 36, 255), false);
                var square = Rect("Phase" + (i + 1), world.transform, points[i], new Vector2(92, 92));
                var graphic = Image(square, new Color32(53, 68, 68, 255), true);
                var button = square.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = graphic;
                square.gameObject.AddComponent<ButtonJuice>(); buttons[i] = button;
                Text("PhaseNumber", square, (i + 1).ToString(), font, 44, new Color32(247, 239, 211, 255));
                var statusRect = Rect("PhaseStatus" + (i + 1), world.transform, points[i] + new Vector2(0, -76), new Vector2(280, 35));
                Image(statusRect, new Color32(24, 43, 39, 230), false);
                statuses[i] = Text("Status", statusRect,
                    "BLOQUEADA", font, 18, new Color32(247, 239, 211, 255));
                var titleRect = Rect("PhaseTitle" + (i + 1), world.transform, points[i] + new Vector2(0, -117), new Vector2(280, 46));
                Image(titleRect, new Color32(24, 43, 39, 230), false);
                Text("Title", titleRect,
                    battles[i].Lesson.Title, font, 20, new Color32(244, 233, 200, 255));
            }
            var wizard = Rect("MapWizard", world.transform, points[0] + new Vector2(-140, 140), new Vector2(160, 180));
            var wizardImage = wizard.gameObject.AddComponent<UnityEngine.UI.Image>();
            wizardImage.raycastTarget = false; wizardImage.preserveAspect = true;
            Sprite[] idle = Frames("Idle"); Sprite[] walk = Frames("Walk"); wizardImage.sprite = idle[0];
            var hud = new GameObject("MapHud", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            var hudCanvas = hud.GetComponent<Canvas>(); hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay; hudCanvas.sortingOrder = 20;
            var scaler = hud.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            var header = Anchored("MapHeader", hud.transform, new Vector2(0, 0.88f), Vector2.one);
            Image(header, new Color32(24, 43, 39, 244), true);
            Text("MapTitle", Anchored("TitleArea", header, new Vector2(0.03f, 0.25f), new Vector2(0.55f, 0.90f)),
                "A trilha do mago", font, 38, new Color32(242, 224, 171, 255), TextAlignmentOptions.Left);
            var counter = Text("Progress", Anchored("CounterArea", header, new Vector2(0.03f, 0.01f), new Vector2(0.50f, 0.27f)),
                "0/9 fases concluídas", font, 20, new Color32(193, 211, 178, 255), TextAlignmentOptions.Left);
            var back = Button("CloseMapButton", header, "Voltar à fase [M]", font, new Vector2(0.76f, 0.20f), new Vector2(0.97f, 0.80f));
            var footer = Anchored("MapFooter", hud.transform, Vector2.zero, new Vector2(1, 0.13f));
            Image(footer, new Color32(24, 43, 39, 244), true);
            var message = Text("MapMessage", Anchored("MessageArea", footer, new Vector2(0.03f, 0.49f), new Vector2(0.97f, 0.98f)),
                "Siga a trilha para aprender e avançar.", font, 22, new Color32(240, 230, 197, 255));
            var left = Button("PanMapLeft", footer, "< Explorar", font, new Vector2(0.30f, 0.08f), new Vector2(0.43f, 0.43f));
            var center = Button("CenterOnWizard", footer, "Ver mago", font, new Vector2(0.44f, 0.08f), new Vector2(0.57f, 0.43f));
            var right = Button("PanMapRight", footer, "Explorar >", font, new Vector2(0.58f, 0.08f), new Vector2(0.71f, 0.43f));
            new GameObject("MapEventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var serialized = new SerializedObject(view);
            Set(serialized, "learningPath", path); Set(serialized, "mapCamera", camera); Set(serialized, "wizard", wizard);
            Set(serialized, "wizardImage", wizardImage); Set(serialized, "progressLabel", counter); Set(serialized, "message", message);
            Set(serialized, "closeButton", back); Set(serialized, "previousButton", left); Set(serialized, "nextButton", right); Set(serialized, "centerButton", center);
            SetArray(serialized, "phaseButtons", buttons); SetArray(serialized, "phaseStatus", statuses);
            SetArray(serialized, "idleFrames", idle); SetArray(serialized, "walkFrames", walk);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, scenePath); SceneManager.SetActiveScene(previous); EditorSceneManager.CloseScene(scene, true);
            var scenes = EditorBuildSettings.scenes.Where(item => item.path != scenePath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true)); EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
        }
        private static Sprite[] Frames(string state)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/PrograMago/Art/Characters/Animations/Mago-" + state + ".anim");
            var binding = AnimationUtility.GetObjectReferenceCurveBindings(clip).First(item => item.propertyName == "m_Sprite");
            return AnimationUtility.GetObjectReferenceCurve(clip, binding).Select(key => (Sprite)key.value).ToArray();
        }
        private static void Set(SerializedObject item, string name, Object value) => item.FindProperty(name).objectReferenceValue = value;
        private static void SetArray<T>(SerializedObject item, string name, T[] values) where T : Object
        {
            var property = item.FindProperty(name); property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
        private static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var item = new GameObject(name, typeof(RectTransform)); item.layer = 5; item.transform.SetParent(parent, false);
            var rect = item.GetComponent<RectTransform>(); rect.sizeDelta = size; rect.anchoredPosition = position; return rect;
        }
        private static RectTransform Anchored(string name, Transform parent, Vector2 min, Vector2 max)
        {
            var rect = Rect(name, parent, Vector2.zero, Vector2.zero); rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero; return rect;
        }
        private static UnityEngine.UI.Image Image(RectTransform rect, Color color, bool raycast)
        {
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = color; image.raycastTarget = raycast; return image;
        }
        private static TMP_Text Text(string name, Transform parent, string value, TMP_FontAsset font,
            float size, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var rect = Anchored(name, parent, Vector2.zero, Vector2.one); var text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font; text.text = value; text.fontSize = size; text.color = color; text.alignment = alignment;
            text.enableAutoSizing = true; text.fontSizeMin = size * 0.75f; text.fontSizeMax = size; text.raycastTarget = false; return text;
        }
        private static UnityEngine.UI.Button Button(string name, Transform parent, string label, TMP_FontAsset font, Vector2 min, Vector2 max)
        {
            var rect = Anchored(name, parent, min, max); var image = Image(rect, new Color32(228, 210, 163, 255), true);
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image; rect.gameObject.AddComponent<ButtonJuice>();
            Text("Label", rect, label, font, 23, new Color32(42, 54, 43, 255)); return button;
        }
    }
}
