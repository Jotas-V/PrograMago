using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace PrograMago.Tests.UnityIntegration
{
    public sealed class GrimoireArtTests
    {
        [TestCase("MainScene", "GrimoireBookIcon", "Closed")]
        [TestCase("GrimoireScene", "OpenBook", "Open")]
        public void Book_UsesOriginalSpriteWithPointFiltering(string sceneName, string objectName, string art)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity", OpenSceneMode.Additive);
            try {
                var item = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .Single(t => t.name == objectName);
                var image = item.GetComponent<Image>();
                Assert.That(image, Is.Not.Null, "O livro precisa do sprite criado para o grimório.");
                Assert.That(image.sprite, Is.Not.Null);
                Assert.That(AssetDatabase.GetAssetPath(image.sprite), Does.EndWith("Grimoire-" + art + "-PixelArt.png"));
                Assert.That(image.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
