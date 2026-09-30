using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PrograMago.Editor
{
    public static class TCC42WizardAnimationBuilder
    {
        private const string ArtRoot = "Assets/PrograMago/Art/Characters/";

        [MenuItem("PrograMago/Art/Integrate TCC-42 Wizard Animations")]
        public static void Build()
        {
            foreach (string form in new[] { "Mago", "Piromante", "Hidromante", "Eletromante" })
            {
                string sheetName = form == "Mago" ? "Mago-Base" : form;
                string originalPath = ArtRoot + sheetName + "-AnimSheet.png";
                string additionalPath = ArtRoot + sheetName + "-AdditionalFrames.png";
                var originalImporter = (TextureImporter)AssetImporter.GetAtPath(originalPath);
                var additionalImporter = (TextureImporter)AssetImporter.GetAtPath(additionalPath);
                // Keep the same world-space canvas as the original 362px cells.
                additionalImporter.spritePixelsPerUnit = originalImporter.spritePixelsPerUnit * 512f / 362f;
                additionalImporter.filterMode = FilterMode.Point;
                additionalImporter.mipmapEnabled = false;
                additionalImporter.textureCompression = TextureImporterCompression.Uncompressed;
                additionalImporter.SaveAndReimport();
                Sprite[] originals = AssetDatabase.LoadAllAssetsAtPath(originalPath).OfType<Sprite>().ToArray();
                Sprite[] additions = AssetDatabase.LoadAllAssetsAtPath(additionalPath).OfType<Sprite>().ToArray();
                foreach (string state in new[] { "Idle", "Walk", "Attack" })
                {
                    string path = ArtRoot + "Animations/" + form + "-" + state + ".anim";
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                    if (clip == null) throw new InvalidOperationException("Missing clip: " + path);
                    // Insert the new poses within the cycle, preserving original relative order.
                    int[] order = state == "Attack" ? new[] { 0, 4, 1, 5, 2, 3 } : new[] { 0, 4, 1, 2, 5, 3 };
                    var keys = order.Select((index, position) => new ObjectReferenceKeyframe
                    {
                        time = position / 12f,
                        value = (index < 4 ? originals : additions).Single(sprite =>
                            sprite.name == form + "_" + state + "_" + index)
                    }).ToArray();
                    clip.frameRate = 12f;
                    AnimationUtility.SetObjectReferenceCurve(clip,
                        EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), keys);
                    var settings = AnimationUtility.GetAnimationClipSettings(clip);
                    settings.startTime = 0f;
                    settings.stopTime = 0.5f;
                    settings.loopTime = state != "Attack";
                    AnimationUtility.SetAnimationClipSettings(clip, settings);
                    EditorUtility.SetDirty(clip);
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("TCC-42: twelve wizard clips integrated with six frames and unchanged 0.5s cycles.");
        }
    }
}
