using System.Collections;
using System.Collections.Generic;
using PrograMago.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace PrograMago.UnityIntegration
{
    public sealed partial class GameplayBootstrapper
    {
        private LevelMapView levelMap;
        private bool mapTransition;
        private bool mapPausedCombat;
        private readonly List<Behaviour> mapHiddenBehaviours = new List<Behaviour>();
        public LevelMapView LevelMap => levelMap;
        public bool IsMapActive { get; private set; }

        private void InitializeLevelMapNavigation()
        {
            var button = CreateButton("OpenLevelMapButton", arenaFrame, "Mapa [M]",
                new Vector2(0.89f, 0.82f), new Vector2(0.995f, 0.98f));
            button.onClick.AddListener(() => ToggleLevelMap());
            var label = nextBattleButton.GetComponentInChildren<TMP_Text>();
            if (label != null && victoryOverlay.activeSelf && learningFlowPresenter.Progress.HasCompletedJourney)
                label.text = "Voltar ao mapa";
            CreateButton("VictoryMapButton", nextBattleButton.transform.parent, "Mapa [M]",
                new Vector2(0.03f, 0.02f), new Vector2(0.24f, 0.10f))
                .onClick.AddListener(() => ToggleLevelMap());
            CreateButton("DefeatMapButton", combatDefeatOverlay.transform, "Mapa [M]",
                new Vector2(0.08f, 0.01f), new Vector2(0.92f, 0.13f))
                .onClick.AddListener(() => ToggleLevelMap());
            // Leave enough room for the new navigation action on the defeat card.
            editAfterDefeatButton.GetComponent<RectTransform>().anchorMin = new Vector2(0.08f, 0.17f);
        }

        public bool HandleMapShortcut(bool pressed)
        {
            if (!pressed || (codeInput != null && codeInput.isFocused)) return false;
            return ToggleLevelMap();
        }

        public bool ToggleLevelMap()
        {
            if (learningFlowPresenter == null || mapTransition || (levelMap != null && levelMap.IsTravelling)) return false;
            if (IsMapActive) {
                levelMap?.RequestClose();
                return true;
            }
            SaveProgress();
            codeInput.DeactivateInputField();
            IsMapActive = true;
            mapTransition = true;
            mapPausedCombat = combat != null && combat.Outcome == CombatOutcome.InProgress && !combat.IsPaused;
            if (mapPausedCombat) combat.Pause();
            HideGameplayForMap();
            StartCoroutine(OpenMapScene());
            return true;
        }

        private void HideGameplayForMap()
        {
            mapHiddenBehaviours.Clear();
            foreach (var root in gameObject.scene.GetRootGameObjects()) {
                foreach (var canvas in root.GetComponentsInChildren<Canvas>(true)) DisableForMap(canvas);
                foreach (var camera in root.GetComponentsInChildren<Camera>(true)) DisableForMap(camera);
                foreach (var events in root.GetComponentsInChildren<EventSystem>(true)) DisableForMap(events);
            }
        }

        private void DisableForMap(Behaviour item)
        {
            if (!item.enabled) return;
            mapHiddenBehaviours.Add(item);
            item.enabled = false;
        }

        private IEnumerator OpenMapScene()
        {
            var loading = SceneManager.LoadSceneAsync(LevelMapView.SceneName, LoadSceneMode.Additive);
            yield return loading;
            Scene scene = SceneManager.GetSceneByName(LevelMapView.SceneName);
            foreach (GameObject root in scene.GetRootGameObjects()) {
                levelMap = root.GetComponentInChildren<LevelMapView>();
                if (levelMap != null) break;
            }
            if (levelMap == null) {
                Debug.LogError("MapScene precisa de LevelMapView configurado.");
                StartCoroutine(CloseMapScene());
                yield break;
            }
            SceneManager.SetActiveScene(scene);
            LearningProgress mapProgress = learningFlowPresenter.Progress;
            if (combat != null && combat.Outcome == CombatOutcome.Defeat) {
                // Preview the retry without changing the live attempt until arrival.
                var retryProgress = new LearningProgress(mapProgress.Path);
                retryProgress.RestorePhaseOne(mapProgress.CapturePhaseOne("", ""));
                mapProgress = retryProgress;
            }
            levelMap.Configure(mapProgress, EnterPhaseFromMap, () => StartCoroutine(CloseMapScene()));
            mapTransition = false;
        }

        public void ReturnToMainMenu()
        {
            if (mapTransition || (levelMap != null && levelMap.IsTravelling)) return;
            SaveProgress();
            SceneManager.LoadScene(MainMenuView.SceneName);
        }

        private bool EnterPhaseFromMap(int index)
        {
            LearningProgress progress = learningFlowPresenter.Progress;
            bool defeated = combat != null && combat.Outcome == CombatOutcome.Defeat;
            bool sameAttempt = progress.CurrentBattleIndex == index && progress.Stage == LearningStage.Editing &&
                !defeated && attemptLife != 0;
            if (defeated) progress.ReportDefeat();
            if (!progress.TrySelectBattle(index)) return false;
            if (!sameAttempt) {
                StoreWorkspaceText();
                ResetFinalAttempt();
                ClearCombatPresentation();
                HideCombatControls();
                combat = null;
                combatAccumulator = 0;
                learningFlowPresenter.Initialize();
            }
            SaveProgress();
            StartCoroutine(CloseMapScene());
            return true;
        }

        private IEnumerator CloseMapScene()
        {
            mapTransition = true;
            SceneManager.SetActiveScene(gameObject.scene);
            Scene scene = SceneManager.GetSceneByName(LevelMapView.SceneName);
            if (scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            foreach (Behaviour item in mapHiddenBehaviours) if (item != null) item.enabled = true;
            mapHiddenBehaviours.Clear();
            if (mapPausedCombat && combat != null && combat.Outcome == CombatOutcome.InProgress) combat.Resume();
            mapPausedCombat = false;
            IsMapActive = false;
            mapTransition = false;
        }
    }
}
