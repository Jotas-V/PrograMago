using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace PrograMago.Tests.UnityIntegration
{
    public sealed class WizardAnimationAssetsTests
    {
        [Test]
        public void WizardFootAnchor_MatchesBottomPivotSprites()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/PrograMago/Prefabs/Mago.prefab");
            Assert.That(prefab.transform.Find("CombatFootAnchor").localPosition.y,
                Is.EqualTo(8f / 300f).Within(0.001f));
        }

        [TestCase("Mago")]
        [TestCase("Piromante")]
        [TestCase("Hidromante")]
        [TestCase("Eletromante")]
        public void WizardCycles_HaveSixResolvedFramesAndKeepOriginalTiming(string form)
        {
            foreach (string state in new[] { "Idle", "Walk", "Attack" })
            {
                string path = "Assets/PrograMago/Art/Characters/Animations/" + form + "-" + state + ".anim";
                AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                Assert.That(clip, Is.Not.Null, path);
                var binding = AnimationUtility.GetObjectReferenceCurveBindings(clip).Single();
                var keys = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                Assert.That(keys.Length, Is.EqualTo(6), path);
                Assert.That(keys.All(key => key.value is Sprite), Is.True, path + " has missing sprites");
                Assert.That(keys.Select(key => key.value).Distinct().Count(), Is.EqualTo(6), path);
                Assert.That(clip.length, Is.EqualTo(0.5f).Within(0.0001f), path);
                Assert.That(AnimationUtility.GetAnimationClipSettings(clip).loopTime,
                    Is.EqualTo(state != "Attack"), path);
                var sprites = keys.Select(key => (Sprite)key.value).ToArray();
                Assert.That(sprites.Select(sprite => sprite.name), Is.EquivalentTo(
                    Enumerable.Range(0, 6).Select(index => form + "_" + state + "_" + index)), path);
                foreach (Sprite sprite in sprites)
                {
                    Assert.That(sprite.bounds.size.x, Is.InRange(0.9f, 1.4f), path);
                    Assert.That(sprite.bounds.size.y, Is.InRange(1f, 1.4f), path);
                    Assert.That(sprite.pivot.x / sprite.rect.width, Is.InRange(0.3f, 0.7f), path);
                    Assert.That(sprite.pivot.y / sprite.rect.height, Is.InRange(0f, 0.15f), path);
                    var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite));
                    Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point), path);
                    Assert.That(importer.mipmapEnabled, Is.False, path);
                    Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed), path);
                }
                string controllerPath = "Assets/PrograMago/Art/Characters/Animations/" +
                    (form == "Mago" ? "Mago.controller" : "Mago-" + form + ".overrideController");
                var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
                Assert.That(controller.animationClips, Does.Contain(clip), controllerPath);
            }
        }
    }
}

