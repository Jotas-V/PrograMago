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
            Assert.That(prefab.GetComponentInChildren<SpriteRenderer>().sprite, Is.Not.Null);
        }
    }
}
