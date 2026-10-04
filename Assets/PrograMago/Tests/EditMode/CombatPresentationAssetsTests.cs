using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace PrograMago.Tests.UnityIntegration
{
    public sealed class CombatPresentationAssetsTests
    {
        [TestCase("Neutral")]
        [TestCase("Fire")]
        [TestCase("Water")]
        [TestCase("Electric")]
        public void SpellPrefab_ContainsReusableProjectileAndVisibleSprite(string element)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/PrograMago/Resources/Combat/" + element + "Projectile.prefab");
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponent("CombatProjectileView"), Is.Not.Null);
            var renderer = prefab.GetComponentInChildren<SpriteRenderer>();
            Assert.That(renderer.sprite, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(renderer.sprite),
                Is.EqualTo("Assets/PrograMago/Art/Combat/" + element + "Projectile-PixelArt.png"));
            Assert.That(renderer.color, Is.EqualTo(Color.white), "A pixel art deve preservar suas cores.");
            Assert.That(renderer.sprite.texture.filterMode, Is.EqualTo(FilterMode.Point));
        }
    }
}
