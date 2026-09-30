using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PrograMago.Tests.UnityIntegration
{
    public sealed class ArenaPresentationTests
    {
        private Scene scene;
        private MonoBehaviour bootstrapper;
        private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;

        [SetUp]
        public void OpenScene()
        {
            scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Additive);
            bootstrapper = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<MonoBehaviour>(true))
                .First(item => item != null && item.GetType().Name == "GameplayBootstrapper");
        }

        [TearDown]
        public void CloseScene()
        {
            if (scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
        }

        [Test]
        public void ForestBackdrops_PreserveAspectAndCoverArenaWithoutGaps()
        {
            var type = bootstrapper.GetType();
            type.GetMethod("UpdateArenaPresentation", PrivateInstance).Invoke(bootstrapper, null);
            var arena = (Rect)type.GetField("arenaWorldRect", PrivateInstance).GetValue(bootstrapper);
            var scenery = ((SpriteRenderer[])type.GetField("scenery", PrivateInstance).GetValue(bootstrapper))
                .Where(item => item != null && item.enabled).OrderBy(item => item.bounds.min.x).ToArray();
            Assert.That(scenery.Length, Is.GreaterThanOrEqualTo(2));
            Assert.That(scenery[0].bounds.min.x, Is.LessThanOrEqualTo(arena.xMin + 0.001f));
            Assert.That(scenery.Last().bounds.max.x, Is.GreaterThanOrEqualTo(arena.xMax - 0.001f));
            foreach (var backdrop in scenery)
            {
                Assert.That(backdrop.transform.lossyScale.x, Is.EqualTo(backdrop.transform.lossyScale.y).Within(0.001f),
                    "Forest trees must keep the source proportions");
                Assert.That(backdrop.bounds.min.y, Is.EqualTo(arena.yMin).Within(0.001f));
                Assert.That(backdrop.bounds.max.y, Is.EqualTo(arena.yMax).Within(0.001f));
            }
            for (int i = 1; i < scenery.Length; i++)
                Assert.That(scenery[i].bounds.min.x, Is.EqualTo(scenery[i - 1].bounds.max.x).Within(0.001f));
            int count = scene.GetRootGameObjects().Sum(root => root.GetComponentsInChildren<Transform>(true).Length);
            type.GetMethod("UpdateArenaPresentation", PrivateInstance).Invoke(bootstrapper, null);
            Assert.That(scene.GetRootGameObjects().Sum(root => root.GetComponentsInChildren<Transform>(true).Length), Is.EqualTo(count));
        }

        [Test]
        public void AmbientSprite_PreservesAspectUnderScaledCanvas()
        {
            var root = new GameObject("AmbientSizeTest");
            root.transform.localScale = new Vector3(2f, 3f, 1f);
            var renderer = new GameObject("Wind").AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(root.transform, false);
            var sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            renderer.sprite = sprite;
            try
            {
                var type = bootstrapper.GetType();
                type.GetField("arenaWorldRect", PrivateInstance).SetValue(bootstrapper, new Rect(0, 0, 16, 3));
                type.GetMethod("SizeAmbientSprite", PrivateInstance)
                    .Invoke(bootstrapper, new object[] { renderer, 0.24f, 0.12f });
                Assert.That(renderer.transform.lossyScale.x, Is.EqualTo(renderer.transform.lossyScale.y).Within(0.001f));
                Assert.That(renderer.bounds.size.x, Is.LessThanOrEqualTo(16f * 0.24f + 0.001f));
                Assert.That(renderer.bounds.size.y, Is.LessThanOrEqualTo(3f * 0.12f + 0.001f));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(sprite); }
        }
        [Test]
        public void WindAndLeaves_TravelTogetherFromLeftToRight()
        {
            var root = new GameObject("AmbientTest");
            var wind = new GameObject("Wind").AddComponent<SpriteRenderer>();
            var leaves = new GameObject("Leaves").AddComponent<SpriteRenderer>();
            wind.transform.SetParent(root.transform); leaves.transform.SetParent(root.transform);
            leaves.enabled = false;
            IEnumerator drift = null;
            try
            {
                var type = bootstrapper.GetType();
                type.GetField("windRenderer", PrivateInstance).SetValue(bootstrapper, wind);
                type.GetField("leavesRenderer", PrivateInstance).SetValue(bootstrapper, leaves);
                type.GetField("arenaWorldRect", PrivateInstance).SetValue(bootstrapper, new Rect(0, 0, 16, 3));
                drift = (IEnumerator)type.GetMethod("DriftAmbientSprite", PrivateInstance)
                    .Invoke(bootstrapper, new object[] { wind, 0.24f, 0.12f, 0.56f, 0.82f });
                Assert.That(drift.MoveNext(), Is.True); // wait between groups
                Assert.That(drift.MoveNext(), Is.True); // first movement frame
                Assert.That(wind.enabled, Is.True);
                Assert.That(leaves.enabled, Is.True, "Leaves must follow the same wind passage");
                Assert.That(wind.transform.position.x, Is.LessThan(0f));
                Assert.That(leaves.transform.position.x, Is.LessThan(wind.transform.position.x));
                for (int sample = 0; sample < 12; sample++)
                {
                    float previousWind = wind.transform.position.x;
                    float previousLeaves = leaves.transform.position.x;
                    Assert.That(drift.MoveNext(), Is.True);
                    Assert.That(wind.transform.position.x, Is.GreaterThanOrEqualTo(previousWind));
                    Assert.That(leaves.transform.position.x, Is.GreaterThanOrEqualTo(previousLeaves));
                    Assert.That(leaves.transform.position.x, Is.LessThan(wind.transform.position.x));
                }
            }
            finally
            {
                (drift as IDisposable)?.Dispose();
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}

