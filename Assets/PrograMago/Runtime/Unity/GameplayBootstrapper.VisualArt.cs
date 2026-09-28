using System.Collections;
using System.Collections.Generic;
using PrograMago.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrograMago.UnityIntegration
{
    public sealed partial class GameplayBootstrapper
    {
        private TCC40VisualCatalog visualCatalog;
        private Transform arenaAtmosphereRoot;
        private SpriteRenderer windRenderer;
        private SpriteRenderer leavesRenderer;
        private SpriteRenderer fireflyRenderer;
        private RuntimeAnimatorController wizardBaseController;
        private Vector3 wizardSpectralAnchor;
        private bool wizardSpectralFormActive;
        private bool wizardSpectralAnchorReady;
        private readonly HashSet<GameObject> movingActors = new HashSet<GameObject>();
        private readonly Dictionary<SpriteRenderer, Coroutine> damageFlashes = new Dictionary<SpriteRenderer, Coroutine>();
        private readonly Dictionary<SpriteRenderer, Color> damageFlashBaseColors = new Dictionary<SpriteRenderer, Color>();
        private Coroutine wizardMoveRoutine;

        private void LoadTCC40Visuals()
        {
            visualCatalog = Resources.Load<TCC40VisualCatalog>("Visuals/TCC40VisualCatalog");
            if (visualCatalog == null)
                Debug.LogWarning("Catálogo visual TCC-40 ausente; a apresentação existente será mantida.");
        }

        private void ApplyTCC40PresentationArt()
        {
            if (visualCatalog == null) return;

            Sprite forestBackground = Resources.Load<Sprite>("Arena/ForestArena");
            if (forestBackground != null && scenery != null)
                foreach (SpriteRenderer backdrop in scenery)
                    if (backdrop != null) backdrop.sprite = forestBackground;

            Image[] images = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Image image in images)
            {
                Sprite panelSprite = image.name switch
                {
                    "CodeEditorPanel" => visualCatalog.workspacePanel,
                    "CodeInput" => visualCatalog.codePanel,
                    "CodeTimeline" => visualCatalog.codeTray,
                    "BottomArea" => visualCatalog.codeTray,
                    "VictoryCard" => visualCatalog.victoryPanel,
                    "MagoStatusCard" => visualCatalog.codePanel,
                    "EnemyStatusCard" => visualCatalog.codePanel,
                    "CombatMessageCard" => visualCatalog.codePanel,
                    "CombatDefeatOverlay" => visualCatalog.codePanel,
                    "EnemyGuidePanel" => visualCatalog.workspacePanel,
                    "TutorialPanel" => visualCatalog.workspacePanel,
                    "RestartProgressPanel" => visualCatalog.workspacePanel,
                    _ => null
                };
                if (panelSprite != null) ApplyPanelSprite(image, panelSprite, false);
            }

            Image inputImage = codeInput == null ? null : codeInput.targetGraphic as Image;
            if (inputImage == null && codeInput != null) inputImage = codeInput.GetComponent<Image>();
            if (inputImage != null) ApplyPanelSprite(inputImage, visualCatalog.codePanel, true);
            if (codeInput != null)
            {
                if (codeInput.textComponent != null)
                    codeInput.textComponent.color = new Color32(232, 238, 222, 255);
                if (codeInput.placeholder != null)
                    codeInput.placeholder.color = new Color32(157, 174, 161, 255);
            }

            Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Button button in buttons)
                ApplyButtonArt(button, button.name == "BattleButton");

            if (victoryTitleText != null)
            {
                victoryTitleText.color = new Color32(96, 61, 26, 255);
                victoryTitleText.fontStyle = FontStyles.Bold;
                victoryTitleText.enableAutoSizing = true;
                victoryTitleText.fontSizeMin = 20;
                victoryTitleText.fontSizeMax = 42;
            }
            if (victoryAchievementText != null) victoryAchievementText.color = new Color32(74, 58, 34, 255);
            if (victoryReviewText != null) victoryReviewText.color = new Color32(58, 54, 43, 255);
            SetChildTextColor("EnemyGuidePanel", new Color32(58, 54, 43, 255));
            SetChildTextColor("TutorialPanel", new Color32(58, 54, 43, 255));
            SetChildTextColor("RestartProgressPanel", new Color32(74, 58, 34, 255));
        }

        private void SetChildTextColor(string panelName, Color color)
        {
            Transform[] objects = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Transform item in objects)
            {
                if (item.name != panelName) continue;
                TMP_Text[] labels = item.GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text label in labels) label.color = color;
            }
        }

        private void ApplyPanelSprite(Image image, Sprite sprite, bool receivesInput)
        {
            if (image == null || sprite == null) return;
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.preserveAspect = false;
            image.raycastTarget = receivesInput;
        }

        private void ApplyCreatedPanelArt(string name, Image image)
        {
            if (visualCatalog == null || image == null) return;
            Sprite sprite = name switch
            {
                "CodeTimeline" => visualCatalog.codeTray,
                "MagoStatusCard" => visualCatalog.codePanel,
                "EnemyStatusCard" => visualCatalog.codePanel,
                "CombatMessageCard" => visualCatalog.codePanel,
                "CombatDefeatOverlay" => visualCatalog.codePanel,
                "EnemyGuidePanel" => visualCatalog.workspacePanel,
                _ => null
            };
            if (sprite != null) ApplyPanelSprite(image, sprite, false);
        }

        private void ApplyButtonArt(Button button, bool battleButtonStyle)
        {
            if (button == null || visualCatalog == null) return;
            Sprite[] states = battleButtonStyle
                ? visualCatalog.battleButtonStates : visualCatalog.actionButtonStates;
            if (states == null || states.Length < 4 || states[0] == null) return;

            Graphic previousGraphic = button.targetGraphic;
            Image image = button.GetComponent<Image>();
            if (image == null) image = button.gameObject.AddComponent<Image>();
            if (previousGraphic != null && previousGraphic != image) previousGraphic.enabled = false;
            TimelineArrowGraphic arrow = button.GetComponent<TimelineArrowGraphic>();
            if (arrow != null) arrow.enabled = false;

            image.sprite = states[0];
            image.type = Image.Type.Sliced;
            image.preserveAspect = false;
            image.color = Color.white;
            image.raycastTarget = true;
            button.targetGraphic = image;
            button.transition = Selectable.Transition.SpriteSwap;
            SpriteState spriteState = button.spriteState;
            spriteState.highlightedSprite = states[1];
            spriteState.pressedSprite = states[2];
            spriteState.disabledSprite = states[3];
            button.spriteState = spriteState;

            if (button.GetComponent<ButtonJuice>() == null) button.gameObject.AddComponent<ButtonJuice>();
        }

        private void EnsureArenaAtmosphere()
        {
            if (visualCatalog == null) return;
            arenaAtmosphereRoot = transform.Find("ArenaAtmosphere");
            if (arenaAtmosphereRoot == null)
            {
                arenaAtmosphereRoot = new GameObject("ArenaAtmosphere").transform;
                arenaAtmosphereRoot.SetParent(transform, false);
            }

            windRenderer = EnsureAmbientSprite("Wind", visualCatalog.arenaWindFrames, 0.14f, -98,
                new Color(0.82f, 0.95f, 0.85f, 0.48f));
            leavesRenderer = EnsureAmbientSprite("Leaves", visualCatalog.arenaLeafFrames, 0.16f, -96,
                new Color(1f, 1f, 1f, 0.9f));
            fireflyRenderer = EnsureAmbientSprite("Firefly", visualCatalog.arenaFireflyFrames, 0.12f, -94,
                new Color(1f, 0.95f, 0.69f, 0.82f));
        }

        private SpriteRenderer EnsureAmbientSprite(string name, Sprite[] frames, float frameDuration,
            int sortingOrder, Color color)
        {
            if (frames == null || frames.Length == 0) return null;
            Transform item = arenaAtmosphereRoot.Find(name);
            if (item == null)
            {
                item = new GameObject(name).transform;
                item.SetParent(arenaAtmosphereRoot, false);
            }
            SpriteRenderer renderer = item.GetComponent<SpriteRenderer>();
            if (renderer == null) renderer = item.gameObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;
            SpriteFrameLoop loop = item.GetComponent<SpriteFrameLoop>();
            if (loop == null) loop = item.gameObject.AddComponent<SpriteFrameLoop>();
            loop.Configure(renderer, frames, frameDuration);
            return renderer;
        }

        private void PlaceAmbientSprite(SpriteRenderer renderer, float horizontal, float vertical,
            float widthFraction, float heightFraction)
        {
            if (renderer == null || renderer.sprite == null || arenaWorldRect.width <= 0 || arenaWorldRect.height <= 0)
                return;
            renderer.transform.position = new Vector3(
                arenaWorldRect.xMin + arenaWorldRect.width * horizontal,
                arenaWorldRect.yMin + arenaWorldRect.height * vertical,
                0f);
            Vector2 spriteSize = renderer.sprite.bounds.size;
            Vector3 parentScale = renderer.transform.parent.lossyScale;
            renderer.transform.localScale = new Vector3(
                arenaWorldRect.width * widthFraction / (spriteSize.x * Mathf.Max(0.0001f, parentScale.x)),
                arenaWorldRect.height * heightFraction / (spriteSize.y * Mathf.Max(0.0001f, parentScale.y)),
                1f / Mathf.Max(0.0001f, parentScale.z));
        }

        private void UpdateArenaAtmosphere()
        {
            PlaceAmbientSprite(windRenderer, 0.43f, 0.68f, 0.24f, 0.12f);
            PlaceAmbientSprite(leavesRenderer, 0.57f, 0.59f, 0.09f, 0.13f);
            PlaceAmbientSprite(fireflyRenderer, 0.72f, 0.74f, 0.045f, 0.08f);
        }

        private void ApplyWizardAppearance(string form)
        {
            if (wizardInstance == null) return;
            SpriteRenderer sprite = wizardInstance.GetComponent<SpriteRenderer>();
            Animator animator = wizardInstance.GetComponent<Animator>();
            if (sprite == null) return;

            if (CurrentMago == null)
            {
                if (!wizardSpectralFormActive)
                {
                    wizardSpectralFormActive = true;
                    wizardSpectralAnchorReady = false;
                }
                if (visualCatalog != null && visualCatalog.spectralWizard != null)
                    sprite.sprite = visualCatalog.spectralWizard;
                if (animator != null) animator.enabled = false;
                Color spectralTint = Color.white;
                spectralTint.a = 0.76f;
                sprite.color = spectralTint;
                return;
            }

            if (wizardSpectralFormActive)
            {
                wizardSpectralFormActive = false;
                wizardSpectralAnchorReady = false;
                wizardInstance.transform.localPosition = Vector3.zero;
            }

            if (animator != null)
            {
                animator.enabled = true;
                RuntimeAnimatorController controller = form switch
                {
                    "piromante" => visualCatalog == null ? null : visualCatalog.pyromancerController,
                    "hidromante" => visualCatalog == null ? null : visualCatalog.hydromancerController,
                    "eletromante" => visualCatalog == null ? null : visualCatalog.electromancerController,
                    _ => wizardBaseController
                };
                if (controller != null && animator.runtimeAnimatorController != controller)
                {
                    animator.SetBool("Walking", false);
                    animator.runtimeAnimatorController = controller;
                    animator.Rebind();
                    animator.Play("Idle", 0, 0f);
                    animator.Update(0f);
                }
            }
            sprite.color = Color.white;
        }

        private void UpdateSpectralWizardIdle()
        {
            if (wizardInstance == null || !wizardInstance.activeInHierarchy || CurrentMago != null || combat != null) return;
            if (!wizardSpectralAnchorReady)
            {
                wizardSpectralAnchor = wizardInstance.transform.localPosition;
                wizardSpectralAnchorReady = true;
            }
            Vector3 bob = wizardSpectralAnchor + Vector3.up * (Mathf.Sin(Time.unscaledTime * 2.4f) * 0.035f);
            wizardInstance.transform.localPosition = bob;
        }

        private void AnimateWizardMove(int destinationCell)
        {
            if (wizardInstance == null) return;
            if (wizardMoveRoutine != null) StopCoroutine(wizardMoveRoutine);
            movingActors.Remove(wizardInstance);
            wizardMoveRoutine = StartCoroutine(MoveWizardToCell(destinationCell));
        }

        private IEnumerator MoveWizardToCell(int destinationCell)
        {
            GameObject actor = wizardInstance;
            if (actor == null) yield break;
            movingActors.Add(actor);
            Animator animator = actor.GetComponent<Animator>();
            if (animator != null && animator.enabled) animator.SetBool("Walking", true);
            Vector3 start = actor.transform.position;
            Vector3 destination = GetActorCellPosition(actor, destinationCell);
            const float duration = 0.24f;
            float elapsed = 0f;
            while (elapsed < duration && actor != null)
            {
                elapsed += Time.unscaledDeltaTime;
                actor.transform.position = Vector3.Lerp(start, destination, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            if (actor != null)
            {
                actor.transform.position = destination;
                if (animator != null && animator.enabled) animator.SetBool("Walking", false);
                movingActors.Remove(actor);
            }
            wizardMoveRoutine = null;
        }

        private Vector3 GetActorCellPosition(GameObject actor, int cell)
        {
            SpriteRenderer sprite = actor.GetComponentInChildren<SpriteRenderer>();
            if (sprite == null) return actor.transform.position;
            Vector3 ground = CellGroundPosition(cell);
            Transform feet = actor.transform.Find("CombatFootAnchor");
            return actor.transform.position + new Vector3(ground.x - sprite.bounds.center.x,
                ground.y - (feet != null ? feet.position.y : sprite.bounds.min.y), 0f);
        }

        private bool IsActorMoving(GameObject actor) => actor != null && movingActors.Contains(actor);

        private void StopWizardMove()
        {
            if (wizardMoveRoutine != null) StopCoroutine(wizardMoveRoutine);
            wizardMoveRoutine = null;
            if (wizardInstance != null)
            {
                movingActors.Remove(wizardInstance);
                Animator animator = wizardInstance.GetComponent<Animator>();
                if (animator != null && animator.enabled) animator.SetBool("Walking", false);
            }
        }

        private void PlayWizardAttack()
        {
            if (wizardInstance == null) return;
            Animator animator = wizardInstance.GetComponent<Animator>();
            if (animator == null || !animator.enabled) return;
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }

        private void FlashCombatTarget(GameObject actor, CombatElement element)
        {
            if (actor == null) return;
            SpriteRenderer sprite = actor.GetComponentInChildren<SpriteRenderer>();
            if (sprite == null) return;
            Color baseColor = damageFlashBaseColors.TryGetValue(sprite, out Color existingBaseColor)
                ? existingBaseColor : sprite.color;
            if (damageFlashes.TryGetValue(sprite, out Coroutine previous) && previous != null)
                StopCoroutine(previous);
            damageFlashBaseColors[sprite] = baseColor;
            damageFlashes[sprite] = StartCoroutine(DamageFlash(sprite, element, baseColor));
        }

        private IEnumerator DamageFlash(SpriteRenderer sprite, CombatElement element, Color baseColor)
        {
            Color elementColor = ElementColor(element);
            sprite.color = Color.Lerp(baseColor, elementColor, 0.72f);
            yield return new WaitForSecondsRealtime(0.14f);
            if (sprite != null)
            {
                if (wizardInstance != null && sprite.gameObject == wizardInstance)
                    ApplyWizardFormTint(combat == null ? "neutro" : combat.WizardForm);
                else
                    sprite.color = baseColor;
                damageFlashes.Remove(sprite);
                damageFlashBaseColors.Remove(sprite);
            }
        }
    }
}
