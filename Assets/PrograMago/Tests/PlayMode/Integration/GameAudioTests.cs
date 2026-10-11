using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PrograMago.UnityIntegration;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PrograMago.Tests.Integration
{
    public sealed class GameAudioTests
    {
        private float master, music, effects;
        private Type AudioType => typeof(GameAudioSettings).Assembly.GetType("PrograMago.UnityIntegration.GameAudio");
        private Component Service
        {
            get {
                Assert.That(AudioType, Is.Not.Null, "O serviço persistente de áudio deve existir.");
                return (Component)AudioType.GetProperty("Instance").GetValue(null);
            }
        }
        private bool Play(string name)
        {
            var method = AudioType.GetMethod("Play", BindingFlags.Public | BindingFlags.Static);
            return (bool)method.Invoke(null, new[] { Enum.Parse(method.GetParameters()[0].ParameterType, name) });
        }
        private AudioSource Music => Service.GetComponentsInChildren<AudioSource>().Single(s => s.loop);
        private AudioSource[] Voices => Service.GetComponentsInChildren<AudioSource>().Where(s => !s.loop).ToArray();

        [SetUp] public void SetUp()
        {
            master = GameAudioSettings.Master; music = GameAudioSettings.Music; effects = GameAudioSettings.Effects;
            GameAudioSettings.Set(GameAudioSettings.MasterKey, 1);
            GameAudioSettings.Set(GameAudioSettings.MusicKey, 1);
            GameAudioSettings.Set(GameAudioSettings.EffectsKey, 1);
        }
        [TearDown] public void TearDown()
        {
            if (AudioType != null) foreach (var voice in Voices) voice.Stop();
            GameAudioSettings.Set(GameAudioSettings.MasterKey, master);
            GameAudioSettings.Set(GameAudioSettings.MusicKey, music);
            GameAudioSettings.Set(GameAudioSettings.EffectsKey, effects);
        }

        [TestCase("Music", 1f, 650f)]
        [TestCase("Hover", .01f, 2f)]
        [TestCase("MapOpen", .01f, 3f)]
        [TestCase("GrimoireOpen", .01f, 3f)]
        [TestCase("PageTurn", .01f, 3f)]
        [TestCase("NormalSpell", .01f, .2f)]
        [TestCase("ElectricSpell", .1f, .3f)]
        [TestCase("FireSpell", .1f, 2f)]
        [TestCase("WaterSpell", .01f, 4f)]
        [TestCase("Damage", .01f, 3f)]
        [TestCase("MapStep", .01f, 2f)]
        [TestCase("Victory", .01f, 8f)]
        public void ApprovedClipIsPlayableAndHasExpectedDuration(string name, float min, float max)
        {
            var clip = Resources.Load<AudioClip>("Audio/" + name);
            Assert.That(clip, Is.Not.Null, name + " deve acompanhar a build.");
            Assert.That(clip.length, Is.InRange(min, max));
            Assert.That(clip.samples, Is.GreaterThan(0));
        }

        [UnityTest] public IEnumerator MusicSurvivesSceneTransitionsWithoutRestarting()
        {
            var original = Service; var clip = Music.clip;
            yield return new WaitForSecondsRealtime(.15f);
            float time = Music.time;
            SceneManager.LoadScene("MenuScene"); yield return null; yield return null;
            Assert.That(Service, Is.SameAs(original));
            Assert.That(Music.clip, Is.SameAs(clip));
            Assert.That(Music.time, Is.GreaterThanOrEqualTo(time));
            Assert.That(UnityEngine.Object.FindObjectsByType(AudioType, FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(Music.isPlaying, Is.True);
        }

        [UnityTest] public IEnumerator VolumesUpdateLiveAndMasterIsAppliedOnlyOnce()
        {
            Play("MapOpen");
            GameAudioSettings.Set(GameAudioSettings.MasterKey, .5f);
            GameAudioSettings.Set(GameAudioSettings.MusicKey, .4f);
            GameAudioSettings.Set(GameAudioSettings.EffectsKey, .3f);
            yield return null;
            Assert.That(AudioListener.volume, Is.EqualTo(.5f));
            Assert.That(Music.volume, Is.EqualTo(.4f * .3f).Within(.001f));
            Assert.That(Voices.Where(s => s.clip != null).All(s => s.volume <= .3f), Is.True);
            GameAudioSettings.Set(GameAudioSettings.EffectsKey, 0);
            yield return null;
            Assert.That(Voices.All(s => s.volume == 0), Is.True);
            Assert.That(Music.isPlaying, Is.True);
        }

        [UnityTest] public IEnumerator RepeatedDamageDoesNotAccumulateUnboundedSources()
        {
            for (int i = 0; i < 100; i++) Play("Damage");
            Assert.That(Voices.Length, Is.EqualTo(8));
            Assert.That(Voices.Count(s => s.isPlaying), Is.LessThanOrEqualTo(1));
            yield return null;
        }

        [UnityTest] public IEnumerator HoverPlaysOncePerEntryAndDisabledButtonsStaySilent()
        {
            var item = new GameObject("AudioHoverTest", typeof(RectTransform), typeof(Button), typeof(ButtonJuice));
            var juice = item.GetComponent<ButtonJuice>();
            var data = new PointerEventData(null);
            juice.OnPointerEnter(data);
            Assert.That(Voices.Any(s => s.clip != null && s.clip.name == "Hover"), Is.True);
            foreach (var voice in Voices) voice.Stop();
            yield return new WaitForSecondsRealtime(.1f);
            juice.OnPointerEnter(data);
            Assert.That(Voices.Any(s => s.isPlaying), Is.False);
            juice.OnPointerExit(data); item.GetComponent<Button>().interactable = false;
            juice.OnPointerEnter(data);
            Assert.That(Voices.Any(s => s.isPlaying), Is.False);
            UnityEngine.Object.Destroy(item);
        }

        [UnityTest] public IEnumerator ImpactCallbackRunsAtArrivalOnlyOnceAndNotOnCancellation()
        {
            var method = typeof(CombatProjectileView).GetMethod("Launch");
            Assert.That(method.GetParameters().Length, Is.EqualTo(5), "Projétil precisa de callback de chegada separado da destruição.");
            var item = new GameObject("AudioProjectileTest", typeof(SpriteRenderer), typeof(CombatProjectileView));
            int arrivals = 0;
            method.Invoke(item.GetComponent<CombatProjectileView>(), new object[] { Vector3.zero, Vector3.right, (Func<bool>)(() => false), null, (Action)(() => arrivals++) });
            Assert.That(arrivals, Is.Zero);
            yield return new WaitForSecondsRealtime(.5f);
            Assert.That(arrivals, Is.EqualTo(1));
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(arrivals, Is.EqualTo(1));
            item = new GameObject("CancelledAudioProjectile", typeof(SpriteRenderer), typeof(CombatProjectileView));
            method.Invoke(item.GetComponent<CombatProjectileView>(), new object[] { Vector3.zero, Vector3.right, (Func<bool>)(() => false), null, (Action)(() => arrivals++) });
            UnityEngine.Object.Destroy(item); yield return null;
            Assert.That(arrivals, Is.EqualTo(1));
        }
    }
}
