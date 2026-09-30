using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PrograMago.Tests.UnityIntegration
{
    public sealed class PaperInterfaceTests
    {
        private Scene scene;
        [SetUp] public void Open() => scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Additive);
        [TearDown] public void Close() => EditorSceneManager.CloseScene(scene, true);
        [TestCase(960, 540)]
        [TestCase(960, 720)]
        public void VictoryCardAndContent_FitInsideAvailableViewport(int width, int height)
        {
            var objects = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
            var bootstrap = objects.SelectMany(t => t.GetComponents<MonoBehaviour>()).First(b => b != null && b.GetType().Name == "GameplayBootstrapper");
            bootstrap.GetType().GetMethod("ApplyTCC40PresentationArt", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(bootstrap, null);
            var overlay = (RectTransform)objects.First(t => t.name == "VictoryOverlay");
            var viewport = new GameObject("ReviewViewport", typeof(RectTransform));
            var rect = (RectTransform)viewport.transform;
            rect.sizeDelta = new Vector2(width, height);
            overlay.SetParent(rect, false);
            overlay.anchorMin = Vector2.zero; overlay.anchorMax = Vector2.one;
            overlay.offsetMin = overlay.offsetMax = Vector2.zero;
            try
            {
                var card = (RectTransform)objects.First(t => t.name == "VictoryCard");
                AssertContained(card, rect);
                foreach (RectTransform child in card.GetComponentsInChildren<RectTransform>(true).Where(t => t.name.Contains("Text") || t.name == "NextBattleButton"))
                    AssertContained(child, card);
            }
            finally { Object.DestroyImmediate(viewport); }
        }
        private static void AssertContained(RectTransform item, RectTransform parent)
        {
            var corners = new Vector3[4]; item.GetWorldCorners(corners);
            foreach (var corner in corners)
            {
                var local = parent.InverseTransformPoint(corner);
                Assert.That(local.x, Is.InRange(parent.rect.xMin - .1f, parent.rect.xMax + .1f), item.name);
                Assert.That(local.y, Is.InRange(parent.rect.yMin - .1f, parent.rect.yMax + .1f), item.name);
            }
        }
    }
}