using System;
using System.IO;
using PrograMago.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
namespace PrograMago.UnityIntegration
{
    public sealed class MainMenuView : MonoBehaviour
    {
        public const string SceneName = "MenuScene";
        [SerializeField] private LearningPathAsset learningPath;
        [SerializeField] private GameObject journeyPanel, resetPanel, optionsPanel, controlsPanel;
        [SerializeField] private UnityEngine.UI.Button playButton, optionsButton, controlsButton, quitButton;
        [SerializeField] private UnityEngine.UI.Button continueButton, newButton, confirmButton;
        [SerializeField] private UnityEngine.UI.Button[] closeButtons;
        [SerializeField] private UnityEngine.UI.Slider masterVolume, musicVolume, effectsVolume;
        [SerializeField] private TMP_Text notice;
        private PhaseOneSaveStore store;
        private bool validSave, resetRequested, navigating, saveExists;
        private void Start()
        {
            var file = Path.Combine(UnityEngine.Application.persistentDataPath, "programago-phase1.json");
            store = new PhaseOneSaveStore(file); saveExists = File.Exists(file);
            try {
                var saved = store.Load();
                if (saved != null) { new LearningProgress(learningPath.ToDomain()).RestorePhaseOne(saved); validSave = true; }
            } catch (Exception) {
                notice.text = "Não foi possível carregar o progresso. Você pode iniciar uma nova jornada com confirmação.";
            }
            continueButton.interactable = validSave;
            playButton.onClick.AddListener(Play); optionsButton.onClick.AddListener(ShowOptions);
            controlsButton.onClick.AddListener(ShowControls); quitButton.onClick.AddListener(Quit);
            continueButton.onClick.AddListener(ContinueJourney); newButton.onClick.AddListener(RequestNewJourney);
            confirmButton.onClick.AddListener(ConfirmNewJourney);
            foreach (var button in closeButtons) button.onClick.AddListener(ClosePanels);
            BindVolume(masterVolume, GameAudioSettings.MasterKey, GameAudioSettings.Master);
            BindVolume(musicVolume, GameAudioSettings.MusicKey, GameAudioSettings.Music);
            BindVolume(effectsVolume, GameAudioSettings.EffectsKey, GameAudioSettings.Effects);
            ClosePanels();
        }
        private static void BindVolume(UnityEngine.UI.Slider slider, string key, float value)
        {
            slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(volume => GameAudioSettings.Set(key, volume));
        }
        private void Update()
        {
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true) ClosePanels();
        }
        public void Play()
        {
            if (navigating) return;
            if (saveExists) { Show(journeyPanel); return; }
            StartNewJourney();
        }
        public void ContinueJourney()
        {
            if (!validSave || navigating) return;
            OpenMap();
        }
        public void RequestNewJourney()
        {
            if (navigating) return;
            if (saveExists) { Show(resetPanel); resetRequested = true; }
            else StartNewJourney();
        }
        public void ConfirmNewJourney()
        {
            if (!resetRequested || navigating) return;
            StartNewJourney();
        }
        private void StartNewJourney()
        {
            try {
                var progress = new LearningProgress(learningPath.ToDomain());
                var initial = progress.CapturePhaseOne("", ""); initial.sourceBlocks = new[] { "", "", "" };
                store.Save(initial); OpenMap();
            } catch (Exception) { notice.text = "Não foi possível salvar a nova jornada. Tente novamente."; }
        }
        private void OpenMap() { navigating = true; SceneManager.LoadScene(LevelMapView.SceneName); }
        private void Show(GameObject panel) { ClosePanels(); panel.SetActive(true); }
        public void ShowOptions() => Show(optionsPanel);
        public void ShowControls() => Show(controlsPanel);
        public void ClosePanels()
        {
            journeyPanel.SetActive(false); resetPanel.SetActive(false);
            optionsPanel.SetActive(false); controlsPanel.SetActive(false); resetRequested = false;
        }
        public void Quit()
        {
            if (UnityEngine.Application.isEditor) { notice.text = "O botão Sair fecha o jogo na versão executável."; return; }
            UnityEngine.Application.Quit();
        }
    }
}
