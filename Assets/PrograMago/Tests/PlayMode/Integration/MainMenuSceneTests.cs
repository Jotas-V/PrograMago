using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using PrograMago.Domain;
using PrograMago.UnityIntegration;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PrograMago.Tests.Integration
{
    public sealed class MainMenuSceneTests
    {
        private string savePath;
        private byte[] previousSave;
        private float previousVolume;
        private readonly string[] keys = { "PrograMago.Volume.Master", "PrograMago.Volume.Music", "PrograMago.Volume.Effects" };
        private bool[] hadKeys;
        private float[] values;
        private object Menu => GameObject.Find("MainMenu").GetComponent(Type.GetType("PrograMago.UnityIntegration.MainMenuView, PrograMago.Unity"));
        private static object Call(object obj, string name, params object[] args)
        {
            var method = obj.GetType().GetMethod(name);
            Assert.That(method, Is.Not.Null, name);
            return method.Invoke(obj, args);
        }
        private void SaveDraft()
        {
            // Read the authored content through the map, rather than inventing battle IDs.
            var authored = UnityEngine.Object.FindFirstObjectByType<LevelMapView>();
            var learningPath = (LearningPathAsset)typeof(LevelMapView).GetField("learningPath", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(authored);
            var progress = new LearningProgress(learningPath.ToDomain());
            var data = progress.CapturePhaseOne("public class Mago { // meu código", "");
            data.sourceBlocks = new[] { data.sourceCode, "", "" };
            new PhaseOneSaveStore(savePath).Save(data);
        }
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            savePath = Path.Combine(UnityEngine.Application.persistentDataPath, "programago-phase1.json");
            previousSave = File.Exists(savePath) ? File.ReadAllBytes(savePath) : null;
            if (File.Exists(savePath)) File.Delete(savePath);
            previousVolume = AudioListener.volume;
            hadKeys = new bool[3]; values = new float[3];
            for (int i = 0; i < 3; i++) { hadKeys[i] = PlayerPrefs.HasKey(keys[i]); values[i] = PlayerPrefs.GetFloat(keys[i]); }
            Assert.That(UnityEngine.Application.CanStreamedLevelBeLoaded("MenuScene"), Is.True, "MenuScene deve existir e estar na build.");
            yield return SceneManager.LoadSceneAsync("MapScene");
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var gameplay = UnityEngine.Object.FindFirstObjectByType<GameplayBootstrapper>();
            if (gameplay != null) UnityEngine.Object.Destroy(gameplay.gameObject);
            yield return null;
            if (previousSave == null) { if (File.Exists(savePath)) File.Delete(savePath); }
            else File.WriteAllBytes(savePath, previousSave);
            if (hadKeys != null) for (int i = 0; i < 3; i++) {
                if (hadKeys[i]) PlayerPrefs.SetFloat(keys[i], values[i]); else PlayerPrefs.DeleteKey(keys[i]);
            }
            PlayerPrefs.Save(); AudioListener.volume = previousVolume;
            yield return null;
        }
        [UnityTest]
        public IEnumerator MainMenu_JogarWithoutSaveOpensMapAtFirstPhase()
        {
            yield return SceneManager.LoadSceneAsync("MenuScene"); yield return null;
            Call(Menu, "Play"); yield return null; yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MapScene"));
            Assert.That(new PhaseOneSaveStore(savePath).Load().currentBattleIndex, Is.Zero);
        }
        [UnityTest]
        public IEnumerator MainMenu_ContinuePreservesSavedCode()
        {
            SaveDraft(); var before = File.ReadAllBytes(savePath);
            yield return SceneManager.LoadSceneAsync("MenuScene"); yield return null;
            Call(Menu, "Play");
            Assert.That(GameObject.Find("JourneyPanel"), Is.Not.Null);
            Call(Menu, "ContinueJourney"); yield return null; yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MapScene"));
            Assert.That(File.ReadAllBytes(savePath), Is.EqualTo(before));
        }
        [UnityTest]
        public IEnumerator MainMenu_CancellingResetKeepsSaveAndConfirmingStartsFresh()
        {
            SaveDraft(); var before = File.ReadAllBytes(savePath);
            yield return SceneManager.LoadSceneAsync("MenuScene"); yield return null;
            Call(Menu, "RequestNewJourney");
            Assert.That(GameObject.Find("ResetJourneyPanel"), Is.Not.Null);
            Call(Menu, "ClosePanels");
            Assert.That(File.ReadAllBytes(savePath), Is.EqualTo(before));
            Call(Menu, "RequestNewJourney"); Call(Menu, "ConfirmNewJourney");
            yield return null; yield return null;
            var saved = new PhaseOneSaveStore(savePath).Load();
            Assert.That(saved.currentBattleIndex, Is.Zero);
            Assert.That(saved.sourceCode ?? "", Is.Empty);
            Assert.That(saved.completed, Is.All.False);
        }
        [UnityTest]
        public IEnumerator MainMenu_VolumeSlidersPersistAndMasterApplies()
        {
            yield return SceneManager.LoadSceneAsync("MenuScene"); yield return null;
            Call(Menu, "ShowOptions");
            GameObject.Find("MasterVolume").GetComponent<UnityEngine.UI.Slider>().value = .4f;
            GameObject.Find("MusicVolume").GetComponent<UnityEngine.UI.Slider>().value = .2f;
            GameObject.Find("EffectsVolume").GetComponent<UnityEngine.UI.Slider>().value = .7f;
            Assert.That(AudioListener.volume, Is.EqualTo(.4f).Within(.001));
            yield return SceneManager.LoadSceneAsync("MenuScene"); yield return null;
            Call(Menu, "ShowOptions");
            Assert.That(GameObject.Find("MusicVolume").GetComponent<UnityEngine.UI.Slider>().value, Is.EqualTo(.2f).Within(.001));
            Assert.That(GameObject.Find("EffectsVolume").GetComponent<UnityEngine.UI.Slider>().value, Is.EqualTo(.7f).Within(.001));
        }
        [UnityTest]
        public IEnumerator MainMenu_ControlsDescribeRealShortcutsAndPanelCloses()
        {
            yield return SceneManager.LoadSceneAsync("MenuScene"); yield return null;
            Call(Menu, "ShowControls");
            string text = GameObject.Find("ControlsText").GetComponent<TMP_Text>().text;
            Assert.That(text, Does.Contain("M").And.Contain("R").And.Contain("5 segundos").And.Contain("digitação"));
            Call(Menu, "ClosePanels");
            Assert.That(GameObject.Find("ControlsPanel"), Is.Null);
        }
        [UnityTest]
        public IEnumerator MainMenu_MapReturnPreservesSaveAndUnloadsMap()
        {
            SaveDraft(); var before = File.ReadAllBytes(savePath);
            Call(UnityEngine.Object.FindFirstObjectByType<LevelMapView>(), "ReturnToMainMenu");
            yield return null; yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MenuScene"));
            Assert.That(SceneManager.GetSceneByName("MapScene").isLoaded, Is.False);
            Assert.That(File.ReadAllBytes(savePath), Is.EqualTo(before));
        }
        [UnityTest]
        public IEnumerator MainMenu_InvalidSaveCannotContinueAndIsNotOverwritten()
        {
            File.WriteAllText(savePath, "registro incompatível");
            yield return SceneManager.LoadSceneAsync("MenuScene"); yield return null;
            Call(Menu, "Play");
            Assert.That(GameObject.Find("ContinueJourneyButton").GetComponent<UnityEngine.UI.Button>().interactable, Is.False);
            Call(Menu, "ContinueJourney"); yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MenuScene"));
            Assert.That(File.ReadAllText(savePath), Is.EqualTo("registro incompatível"));
        }
        [UnityTest]
        public IEnumerator MainMenu_ReturnFromGameplayPreservesDraftAndUnloadsBothScenes()
        {
            yield return SceneManager.LoadSceneAsync("MainScene"); yield return null;
            var gameplay = UnityEngine.Object.FindFirstObjectByType<GameplayBootstrapper>();
            GameObject.Find("CodeInput").GetComponent<TMP_InputField>().text = "public class Mago { // rascunho de retorno";
            gameplay.ToggleLevelMap();
            for (int i = 0; i < 60 && !SceneManager.GetSceneByName("MapScene").isLoaded; i++) yield return null;
            yield return null;
            Call(UnityEngine.Object.FindFirstObjectByType<LevelMapView>(), "ReturnToMainMenu");
            yield return null; yield return null;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("MenuScene"));
            Assert.That(SceneManager.GetSceneByName("MainScene").isLoaded, Is.False);
            Assert.That(SceneManager.GetSceneByName("MapScene").isLoaded, Is.False);
            Assert.That(new PhaseOneSaveStore(savePath).Load().sourceBlocks[0], Does.Contain("rascunho de retorno"));
        }
    }
}
