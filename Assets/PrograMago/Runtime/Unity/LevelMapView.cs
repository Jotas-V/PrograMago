using System;
using System.Collections.Generic;
using System.IO;
using PrograMago.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PrograMago.UnityIntegration
{
    public sealed class LevelMapView : MonoBehaviour
    {
        [SerializeField] private LearningPathAsset learningPath;
        [SerializeField] private Camera mapCamera;
        [SerializeField] private RectTransform wizard;
        [SerializeField] private UnityEngine.UI.Image wizardImage;
        [SerializeField] private Sprite[] idleFrames;
        [SerializeField] private Sprite[] walkFrames;
        [SerializeField] private UnityEngine.UI.Button[] phaseButtons;
        [SerializeField] private TMP_Text[] phaseStatus;
        [SerializeField] private TMP_Text progressLabel;
        [SerializeField] private TMP_Text message;
        [SerializeField] private UnityEngine.UI.Button closeButton;
        [SerializeField] private UnityEngine.UI.Button previousButton;
        [SerializeField] private UnityEngine.UI.Button nextButton;
        [SerializeField] private UnityEngine.UI.Button centerButton;
        [SerializeField] private UnityEngine.UI.Button menuButton;

        public const string SceneName = "MapScene";
        public const float WorldWidth = 36f;
        public static readonly Vector2[] PhasePositions = {
            new Vector2(-1550, -40), new Vector2(-1170, 100), new Vector2(-790, -80),
            new Vector2(-410, 100), new Vector2(-30, -70), new Vector2(350, 100),
            new Vector2(730, -60), new Vector2(1110, 80), new Vector2(1490, -20)
        };
        private static readonly Vector2 WizardOffset = new Vector2(0, 140);
        private readonly Queue<Vector2> waypoints = new Queue<Vector2>();
        private LearningProgress progress;
        private Func<int, bool> enterPhase;
        private Action close;
        private PhaseOneSaveStore standaloneStore;
        private PhaseOneSaveData standaloneSave;
        private int currentNode;
        private int targetNode = -1;
        private float animationTime;
        private float cameraTargetX;
        private bool followWizard = true;

        public bool IsVisible { get; private set; }
        public bool IsTravelling => targetNode >= 0;
        public Vector2 WizardPosition => wizard.anchoredPosition;
        public bool CameraWithinBounds => Mathf.Abs(mapCamera.transform.position.x) <= CameraLimit + 0.01f;
        private float CameraLimit => Mathf.Max(0, WorldWidth / 2f - mapCamera.orthographicSize * mapCamera.aspect);

        private void Start()
        {
            for (int i = 0; i < phaseButtons.Length; i++) {
                int index = i;
                phaseButtons[i].onClick.AddListener(() => SelectPhase(index));
            }
            closeButton.onClick.AddListener(RequestClose);
            previousButton.onClick.AddListener(() => PanCamera(-1));
            nextButton.onClick.AddListener(() => PanCamera(1));
            centerButton.onClick.AddListener(() => { followWizard = true; });
            if (menuButton != null) menuButton.onClick.AddListener(ReturnToMainMenu);
            var gameplay = FindFirstObjectByType<GameplayBootstrapper>();
            if (gameplay != null && gameplay.IsMapActive) return;

            // Opening the map directly restores the same local journey as the menu.
            try {
                progress = new LearningProgress(learningPath.ToDomain());
                standaloneStore = new PhaseOneSaveStore(Path.Combine(
                    UnityEngine.Application.persistentDataPath, "programago-phase1.json"));
                standaloneSave = standaloneStore.Load();
                if (standaloneSave != null) progress.RestorePhaseOne(standaloneSave);
                Configure(progress, EnterStandalonePhase, null);
            } catch (Exception exception) {
                Debug.LogError("Não foi possível abrir o mapa: " + exception.Message);
                message.text = "Não foi possível carregar seu progresso. Volte ao jogo para revisar o registro.";
            }
        }

        public void ReturnToMainMenu()
        {
            if (IsTravelling) return;
            var gameplay = FindFirstObjectByType<GameplayBootstrapper>();
            if (gameplay != null) gameplay.ReturnToMainMenu();
            else SceneManager.LoadScene(MainMenuView.SceneName);
        }

        public void Configure(LearningProgress value, Func<int, bool> onEnter, Action onClose)
        {
            progress = value ?? throw new ArgumentNullException(nameof(value));
            enterPhase = onEnter ?? throw new ArgumentNullException(nameof(onEnter));
            close = onClose;
            IsVisible = true;
            currentNode = progress.CurrentBattleIndex;
            wizard.anchoredPosition = PhasePositions[currentNode] + WizardOffset;
            if (currentNode == 0 && !progress.IsCompleted(progress.CurrentBattle.Id))
                wizard.anchoredPosition += new Vector2(-140, 0);
            targetNode = -1;
            followWizard = true;
            cameraTargetX = wizard.position.x;
            FollowCamera(1f, true);
            RefreshState();
        }

        public bool SelectPhase(int index)
        {
            if (!IsVisible || IsTravelling || progress == null || !progress.CanSelectBattle(index)) return false;
            waypoints.Clear();
            int direction = index >= currentNode ? 1 : -1;
            for (int i = currentNode; ; i += direction) {
                waypoints.Enqueue(PhasePositions[i] + WizardOffset);
                if (i == index) break;
            }
            targetNode = index;
            followWizard = true;
            message.text = $"Caminhando até a fase {index + 1}… A fase começa ao chegar.";
            RefreshButtons();
            return true;
        }

        public void AdvanceTravel(float deltaTime)
        {
            if (!IsVisible || !IsTravelling) return;
            float remaining = Mathf.Max(0, deltaTime) * 560f;
            while (waypoints.Count > 0 && remaining > 0) {
                Vector2 target = waypoints.Peek();
                float distance = Vector2.Distance(wizard.anchoredPosition, target);
                float direction = target.x - wizard.anchoredPosition.x;
                if (Mathf.Abs(direction) > 0.01f)
                    wizard.localScale = new Vector3(direction < 0 ? -1 : 1, 1, 1);
                wizard.anchoredPosition = Vector2.MoveTowards(wizard.anchoredPosition, target, remaining);
                if (remaining < distance) break;
                remaining -= distance;
                waypoints.Dequeue();
            }
            FollowCamera(deltaTime, false);
            if (waypoints.Count != 0) return;
            int arrived = targetNode;
            targetNode = -1;
            currentNode = arrived;
            if (enterPhase(arrived)) {
                IsVisible = false;
            } else {
                message.text = "Esta fase não está disponível agora. Seu progresso foi preservado.";
                RefreshButtons();
            }
        }

        private void Update()
        {
            if (!IsVisible) return;
            if (Keyboard.current?.mKey.wasPressedThisFrame == true) RequestClose();
            if (!IsVisible) return;
            AdvanceTravel(Time.unscaledDeltaTime);
            if (!IsTravelling) FollowCamera(Time.unscaledDeltaTime, false);
            Sprite[] frames = IsTravelling ? walkFrames : idleFrames;
            if (frames != null && frames.Length > 0) {
                animationTime += Time.unscaledDeltaTime;
                wizardImage.sprite = frames[(int)(animationTime / 0.16f) % frames.Length];
            }
        }

        public void RequestClose()
        {
            if (!IsVisible || IsTravelling || close == null) return;
            IsVisible = false;
            close();
        }

        private void OnDestroy() { IsVisible = false; }

        private void PanCamera(int direction)
        {
            if (IsTravelling) return;
            followWizard = false;
            cameraTargetX = Mathf.Clamp(mapCamera.transform.position.x + direction * 5f, -CameraLimit, CameraLimit);
        }

        private void FollowCamera(float deltaTime, bool snap)
        {
            if (followWizard) cameraTargetX = wizard.position.x;
            cameraTargetX = Mathf.Clamp(cameraTargetX, -CameraLimit, CameraLimit);
            Vector3 position = mapCamera.transform.position;
            position.x = snap ? cameraTargetX : Mathf.Lerp(position.x, cameraTargetX, 1f - Mathf.Exp(-7f * deltaTime));
            position.x = Mathf.Clamp(position.x, -CameraLimit, CameraLimit);
            mapCamera.transform.position = position;
        }

        private void RefreshState()
        {
            int completed = 0;
            for (int i = 0; i < phaseButtons.Length; i++) {
                bool done = progress.IsCompleted(progress.Path.Battles[i].Id);
                if (done) completed++;
                bool available = progress.CanSelectBattle(i);
                phaseStatus[i].text = available ? (done ? "REJOGAR" : "JOGAR") : done ? "CONCLUÍDA" : "BLOQUEADA";
                phaseButtons[i].GetComponent<UnityEngine.UI.Image>().color = available
                    ? new Color32(238, 207, 129, 255) : done ? new Color32(83, 133, 111, 255) : new Color32(53, 68, 68, 255);
                phaseButtons[i].GetComponentInChildren<TMP_Text>().color = available
                    ? new Color32(40, 52, 40, 255) : new Color32(247, 239, 211, 255);
                var colors = phaseButtons[i].colors;
                colors.disabledColor = Color.white;
                colors.highlightedColor = new Color32(255, 236, 180, 255);
                phaseButtons[i].colors = colors;
            }
            progressLabel.text = $"{completed}/{phaseButtons.Length} fases concluídas";
            message.text = progress.Stage == LearningStage.BattleInProgress
                ? "Combate pausado. Volte à fase para continuar a tentativa."
                : progress.HasCompletedJourney ? "Jornada concluída! Escolha qualquer fase para jogar novamente."
                : "Siga a trilha. A próxima fase abre após a vitória; replay livre ao concluir a jornada.";
            RefreshButtons();
        }

        private void RefreshButtons()
        {
            if (menuButton != null) menuButton.interactable = !IsTravelling;
            for (int i = 0; i < phaseButtons.Length; i++)
                phaseButtons[i].interactable = !IsTravelling && progress.CanSelectBattle(i);
            closeButton.interactable = !IsTravelling && close != null;
            closeButton.gameObject.SetActive(close != null);
            previousButton.interactable = nextButton.interactable = centerButton.interactable = !IsTravelling;
        }

        private bool EnterStandalonePhase(int index)
        {
            if (!progress.TrySelectBattle(index)) return false;
            PhaseOneSaveData data = progress.CapturePhaseOne(
                standaloneSave?.sourceCode ?? "", standaloneSave?.approvedCode ?? "",
                standaloneSave?.sourceBlocks ?? new[] { standaloneSave?.sourceCode ?? "", "", "" },
                standaloneSave?.activeBlock ?? 0);
            data.combatBlocks = standaloneSave?.combatBlocks;
            data.timelineOrder = standaloneSave?.timelineOrder;
            data.approvedMethods = standaloneSave?.approvedMethods;
            data.methodDraft = standaloneSave?.methodDraft;
            data.preparationCode = standaloneSave?.preparationCode;
            try { standaloneStore.Save(data); }
            catch (Exception exception) { Debug.LogError("Não foi possível salvar a fase: " + exception.Message); return false; }
            SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
            return true;
        }
    }
}
