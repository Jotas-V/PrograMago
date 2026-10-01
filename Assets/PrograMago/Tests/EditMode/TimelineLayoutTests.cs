using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PrograMago.Tests.UnityIntegration
{
    public sealed class TimelineLayoutTests
    {
        [Test]
        public void CombatPresentation_RemainsActiveUntilMovementFinishes()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Additive);
            var actor = new GameObject("MovingActor");
            try
            {
                var bootstrap = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<MonoBehaviour>(true)).First(b => b != null && b.GetType().Name == "GameplayBootstrapper");
                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                var moving = (System.Collections.Generic.HashSet<GameObject>)bootstrap.GetType().GetField("movingActors", flags).GetValue(bootstrap);
                var visuals = bootstrap.GetType().GetProperty("HasCombatVisuals", flags);
                Assert.That((bool)visuals.GetValue(bootstrap), Is.False);
                moving.Add(actor);
                Assert.That((bool)visuals.GetValue(bootstrap), Is.True, "Victory must wait for the final movement");
                moving.Remove(actor);
                Assert.That((bool)visuals.GetValue(bootstrap), Is.False);
            }
            finally { Object.DestroyImmediate(actor); EditorSceneManager.CloseScene(scene, true); }
        }
        [Test]
        public void BottomControls_AlignAndLeaveRoomForLongLabels()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity", OpenSceneMode.Additive);
            try
            {
                var objects = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToArray();
                var bootstrap = objects.SelectMany(t => t.GetComponents<MonoBehaviour>()).First(b => b != null && b.GetType().Name == "GameplayBootstrapper");
                var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                bootstrap.GetType().GetMethod("EnsureWorkspaceControls", flags).Invoke(bootstrap, null);
                bootstrap.GetType().GetMethod("RefreshTimelineLayout", flags).Invoke(bootstrap, null);
                var names = new[]{"CodeBlockButton1", "CodeBlockButton2", "CodeBlockButton3", "PreparationWorkspaceButton", "CombatAction1", "BattleButton"};
                float? bottom = null, top = null;
                foreach (var name in names)
                {
                    var rect = (RectTransform)objects.First(t => t.name == name);
                    var corners = new Vector3[4]; rect.GetWorldCorners(corners);
                    Assert.That(rect.rect.width, Is.GreaterThanOrEqualTo(name == "CodeBlockButton3" ? 180 : 140), name);
                    Assert.That(rect.rect.height, Is.GreaterThanOrEqualTo(48), name);
                    if (bottom.HasValue)
                    {
                        Assert.That(corners[0].y, Is.EqualTo(bottom.Value).Within(.1f), name);
                        Assert.That(corners[1].y, Is.EqualTo(top.Value).Within(.1f), name);
                    }
                    else { bottom = corners[0].y; top = corners[1].y; }
                }
                var files = names.Take(3).Select(n => (RectTransform)objects.First(t => t.name == n)).ToArray();
                for (int i = 1; i < files.Length; i++)
                    Assert.That(files[i].anchoredPosition.x, Is.GreaterThanOrEqualTo(files[i-1].anchoredPosition.x + files[i-1].rect.width + 8));
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
    }
}