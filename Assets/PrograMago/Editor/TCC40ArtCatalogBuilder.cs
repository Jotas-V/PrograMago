using System;
using System.Linq;
using PrograMago.UnityIntegration;
using UnityEditor;
using UnityEngine;

namespace PrograMago.Editor
{
    public static class TCC40ArtCatalogBuilder
    {
        private const string CatalogPath = "Assets/PrograMago/Resources/Visuals/TCC40VisualCatalog.asset";
        private const string UiSheetPath = "Assets/PrograMago/Art/UI/UI-Panel-Button-Sheet.png";

        public static void Build()
        {
            EnsureFolder("Assets/PrograMago/Resources/Visuals");
            TCC40VisualCatalog catalog = AssetDatabase.LoadAssetAtPath<TCC40VisualCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<TCC40VisualCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            catalog.spectralWizard = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/PrograMago/Art/Characters/Mago-Espectral.png");
            catalog.pyromancerController = Load<RuntimeAnimatorController>(
                "Assets/PrograMago/Art/Characters/Animations/Mago-Piromante.overrideController");
            catalog.hydromancerController = Load<RuntimeAnimatorController>(
                "Assets/PrograMago/Art/Characters/Animations/Mago-Hidromante.overrideController");
            catalog.electromancerController = Load<RuntimeAnimatorController>(
                "Assets/PrograMago/Art/Characters/Animations/Mago-Eletromante.overrideController");

            catalog.workspacePanel = FindSprite("UI_WorkspacePanel");
            catalog.codePanel = FindSprite("UI_CodePanel");
            catalog.codeTray = FindSprite("UI_CodeTray");
            catalog.victoryPanel = FindSprite("UI_VictoryPanel");
            catalog.battleButtonStates = new[]
            {
                FindSprite("UI_Battle_Idle"), FindSprite("UI_Battle_Hover"),
                FindSprite("UI_Battle_Pressed"), FindSprite("UI_Battle_Disabled")
            };
            catalog.actionButtonStates = new[]
            {
                FindSprite("UI_Action_Idle"), FindSprite("UI_Action_Hover"),
                FindSprite("UI_Action_Pressed"), FindSprite("UI_Action_Disabled")
            };
            catalog.arenaWindFrames = ReadFrames("Assets/PrograMago/Art/Environment/Animations/Arena-Wind.anim");
            catalog.arenaLeafFrames = ReadFrames("Assets/PrograMago/Art/Environment/Animations/Arena-Leaves.anim");
            catalog.arenaFireflyFrames = ReadFrames("Assets/PrograMago/Art/Environment/Animations/Arena-Firefly.anim");

            Validate(catalog);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("TCC-40: catálogo de sprites e animações atualizado em " + CatalogPath);
        }

        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new InvalidOperationException("Asset não encontrado: " + path);
            return asset;
        }

        private static Sprite FindSprite(string name)
        {
            Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(UiSheetPath)
                .OfType<Sprite>().FirstOrDefault(candidate => candidate.name == name);
            if (sprite == null) throw new InvalidOperationException("Sprite não encontrado: " + name);
            return sprite;
        }

        private static Sprite[] ReadFrames(string path)
        {
            AnimationClip clip = Load<AnimationClip>(path);
            EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
            foreach (EditorCurveBinding binding in bindings)
            {
                Sprite[] frames = AnimationUtility.GetObjectReferenceCurve(clip, binding)
                    .Select(key => key.value as Sprite).Where(sprite => sprite != null).ToArray();
                if (frames.Length > 0) return frames;
            }
            throw new InvalidOperationException("O clip não contém frames de sprite: " + path);
        }

        private static void Validate(TCC40VisualCatalog catalog)
        {
            if (catalog.spectralWizard == null || catalog.pyromancerController == null ||
                catalog.hydromancerController == null || catalog.electromancerController == null ||
                catalog.workspacePanel == null || catalog.codePanel == null || catalog.codeTray == null ||
                catalog.victoryPanel == null || catalog.battleButtonStates.Any(sprite => sprite == null) ||
                catalog.actionButtonStates.Any(sprite => sprite == null) ||
                catalog.arenaWindFrames == null || catalog.arenaLeafFrames == null ||
                catalog.arenaFireflyFrames == null || catalog.arenaWindFrames.Length == 0 ||
                catalog.arenaLeafFrames.Length == 0 || catalog.arenaFireflyFrames.Length == 0)
                throw new InvalidOperationException("O catálogo TCC-40 ficou incompleto.");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string[] parts = path.Split('/');
            string parent = parts[0];
            for (int index = 1; index < parts.Length; index++)
            {
                string child = parent + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(child)) AssetDatabase.CreateFolder(parent, parts[index]);
                parent = child;
            }
        }
    }
}
