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
        private Coroutine windTraverseRoutine;
        private Coroutine leavesTraverseRoutine;
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
            ApplyWorkspaceGround();

            Sprite forestBackground = Resources.Load<Sprite>("Arena/ForestArena");
            if (forestBackground != null && scenery != null)
                foreach (SpriteRenderer backdrop in scenery)
                    if (backdrop != null) backdrop.sprite = forestBackground;

            Sprite codeEditorBoard = Resources.Load<Sprite>("Visuals/CodeEditorBoardFrame");
            Image[] images = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Image image in images)
            {
                if (image == null) continue;
                if (image.name == "BottomArea") continue;
                if (image.name == "CodeInput")
                {
                    SetTransparentPanelImage(image, true);
                    continue;
                }
                if (image.name == "TutorialPanel")
                {
                    image.sprite = null;
                    image.type = Image.Type.Simple;
                    image.color = new Color32(243, 240, 225, 255);
                    image.preserveAspect = false;
                    image.raycastTarget = false;
                    continue;
                }
                if (image.name == "EnemyGuidePanel")
                {
                    SetTransparentPanelImage(image, false);
                    continue;
                }
                if (image.name == "CodeEditorPanel")
                {
                    if (codeEditorBoard != null) ApplyPanelSprite(image, codeEditorBoard, false);
                    else
                    {
                        image.sprite = null;
                        image.type = Image.Type.Simple;
                        image.color = new Color32(47, 38, 30, 255);
                        image.raycastTarget = false;
                    }
                    continue;
                }

                Sprite panelSprite = visualCatalog == null ? null : image.name switch
                {
                    "CodeTimeline" => visualCatalog.codeTray,
                    "VictoryCard" => visualCatalog.victoryPanel,
                    "MagoStatusCard" => visualCatalog.codePanel,
                    "EnemyStatusCard" => visualCatalog.codePanel,
                    "CombatMessageCard" => visualCatalog.codePanel,
                    "CombatDefeatOverlay" => visualCatalog.codePanel,

                    "RestartProgressPanel" => visualCatalog.workspacePanel,
                    _ => null
                };
                if (panelSprite != null) ApplyPanelSprite(image, panelSprite, false);
            }

            Image inputImage = codeInput == null ? null : codeInput.targetGraphic as Image;
            if (inputImage == null && codeInput != null) inputImage = codeInput.GetComponent<Image>();
            if (inputImage != null)
            {
                SetTransparentPanelImage(inputImage, true);
                inputImage.color = new Color32(28, 31, 43, 238);
            }
            if (codeInput != null)
            {
                RectTransform editorTextRect = codeInput.GetComponent<RectTransform>();
                if (editorTextRect != null)
                {
                    // Keep code clear of the thin wood trim while using nearly all of the board.
                    editorTextRect.anchorMin = new Vector2(0.055f, 0.105f);
                    editorTextRect.anchorMax = new Vector2(0.945f, 0.91f);
                    editorTextRect.offsetMin = Vector2.zero;
                    editorTextRect.offsetMax = Vector2.zero;
                }
                // Keep the hit target but prevent Selectable color transitions from tinting the clear editor surface.
                codeInput.targetGraphic = null;
                if (inputImage != null) inputImage.raycastTarget = true;
                if (codeInput.textComponent != null)
                    codeInput.textComponent.color = new Color32(232, 238, 222, 255);
                if (codeInput.placeholder != null)
                    codeInput.placeholder.color = new Color32(157, 174, 161, 255);
            }

            if (visualCatalog == null) return;

            Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Button button in buttons)
            {
                ApplyButtonArt(button, button.name == "BattleButton");
                TMP_Text buttonLabel = button.GetComponentInChildren<TMP_Text>(true);
                if (buttonLabel != null)
                {
                    LayoutButtonLabel(button, buttonLabel);
                    buttonLabel.color = new Color32(255, 246, 222, 255);
                    buttonLabel.fontStyle = FontStyles.Bold;
                }
            }

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
            SetChildTextColor("RestartProgressPanel", new Color32(74, 58, 34, 255));
        }

        private void ApplyWorkspaceGround()
        {
            GameObject bottomArea = GameObject.Find("BottomArea");
            Sprite earthBackground = Resources.Load<Sprite>("Arena/WorkspaceEarthBackground");
            if (bottomArea == null || earthBackground == null) return;

            Image background = bottomArea.GetComponent<Image>();
            if (background == null) background = bottomArea.AddComponent<Image>();
            background.sprite = earthBackground;
            background.type = Image.Type.Simple;
            background.color = Color.white;
            background.preserveAspect = false;
            background.raycastTarget = false;
        }

        private static void SetTransparentPanelImage(Image image, bool receivesInput)
        {
            if (image == null) return;
            image.sprite = null;
            image.type = Image.Type.Simple;
            image.color = Color.clear;
            image.preserveAspect = false;
            image.raycastTarget = receivesInput;
        }

        private void SetChildTextColor(string panelName, Color color)
        {
            Transform[] objects = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Transform item in objects)
            {
                if (item.name != panelName) continue;
                TMP_Text[] labels = item.GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text label in labels)
                {
                    if (label.GetComponentInParent<Button>() != null) continue;
                    label.color = color;
                }
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
                _ => null
            };
            if (sprite != null) ApplyPanelSprite(image, sprite, false);
        }

        private void ApplyButtonArt(Button button, bool battleButtonStyle)
        {
            if (button == null || visualCatalog == null) return;
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            bool squareBattleButton = battleButtonStyle && buttonRect != null &&
                buttonRect.rect.width > 0f && buttonRect.rect.height > 0f &&
                buttonRect.rect.width <= buttonRect.rect.height;
            // The in-game Battle button is horizontal, so reuse the existing horizontal action-button states.
            Sprite[] states = squareBattleButton
                ? visualCatalog.battleButtonStates : visualCatalog.actionButtonStates;
            if (states == null || states.Length < 4 || states[0] == null) return;

            Graphic previousGraphic = button.targetGraphic;
            if (previousGraphic == null) previousGraphic = button.GetComponent<Graphic>();
            Image image = button.GetComponent<Image>();
            if (image == null) image = previousGraphic as Image;
            if (image == null && previousGraphic != null && !(previousGraphic is Image))
            {
                Transform artChild = button.transform.Find("ButtonArt");
                if (artChild == null)
                {
                    var artObject = new GameObject("ButtonArt", typeof(RectTransform),
                        typeof(CanvasRenderer), typeof(Image));
                    artChild = artObject.transform;
                    artChild.SetParent(button.transform, false);
                    RectTransform artRect = artObject.GetComponent<RectTransform>();
                    artRect.anchorMin = Vector2.zero;
                    artRect.anchorMax = Vector2.one;
                    artRect.offsetMin = Vector2.zero;
                    artRect.offsetMax = Vector2.zero;
                    artChild.SetAsFirstSibling();
                }
                image = artChild.GetComponent<Image>();
                if (image == null) image = artChild.gameObject.AddComponent<Image>();
                previousGraphic.enabled = false;
                previousGraphic.raycastTarget = false;
            }
            if (image == null) image = button.gameObject.AddComponent<Image>();
            TimelineArrowGraphic arrow = button.GetComponent<TimelineArrowGraphic>();
            if (arrow != null)
            {
                arrow.enabled = false;
                arrow.raycastTarget = false;
            }

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

        private static void LayoutButtonLabel(Button button, TMP_Text label)
        {
            RectTransform buttonRect = button == null ? null : button.GetComponent<RectTransform>();
            RectTransform labelRect = label == null ? null : label.rectTransform;
            if (buttonRect == null || labelRect == null) return;

            bool squareButton = buttonRect.rect.width > 0f && buttonRect.rect.height > 0f &&
                buttonRect.rect.width <= buttonRect.rect.height * 1.2f;
            bool codeTab = button.name.StartsWith("CodeBlockButton");
            labelRect.anchorMin = squareButton ? new Vector2(0.06f, 0.035f) :
                new Vector2(codeTab ? 0.20f : 0.22f, 0.04f);
            labelRect.anchorMax = squareButton ? new Vector2(0.94f, 0.34f) :
                new Vector2(0.97f, 0.96f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true;
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.fontSizeMin = codeTab ? 10f : 9f;
            label.fontSizeMax = codeTab ? 15f : 18f;
            label.margin = Vector4.zero;
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

            windRenderer = EnsureAmbientSprite("Wind", visualCatalog.arenaWindFrames, 0.28f, -98,
                new Color(0.82f, 0.95f, 0.85f, 0.34f));
            leavesRenderer = EnsureAmbientSprite("Leaves", visualCatalog.arenaLeafFrames, 0.31f, -96,
                new Color(1f, 1f, 1f, 0.72f));
            fireflyRenderer = EnsureAmbientSprite("Firefly", visualCatalog.arenaFireflyFrames, 0.23f, -94,
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
            SizeAmbientSprite(renderer, widthFraction, heightFraction);
            SetAmbientSpritePosition(renderer, horizontal, vertical);
        }

        private void SizeAmbientSprite(SpriteRenderer renderer, float widthFraction, float heightFraction)
        {
            if (renderer == null || renderer.sprite == null || arenaWorldRect.width <= 0 || arenaWorldRect.height <= 0)
                return;
            Vector2 spriteSize = renderer.sprite.bounds.size;
            Vector3 parentScale = renderer.transform.parent.lossyScale;
            renderer.transform.localScale = new Vector3(
                arenaWorldRect.width * widthFraction / (spriteSize.x * Mathf.Max(0.0001f, parentScale.x)),
                arenaWorldRect.height * heightFraction / (spriteSize.y * Mathf.Max(0.0001f, parentScale.y)),
                1f / Mathf.Max(0.0001f, parentScale.z));
        }

        private void SetAmbientSpritePosition(SpriteRenderer renderer, float horizontal, float vertical)
        {
            if (renderer == null || arenaWorldRect.width <= 0 || arenaWorldRect.height <= 0) return;
            renderer.transform.position = new Vector3(
                arenaWorldRect.xMin + arenaWorldRect.width * horizontal,
                arenaWorldRect.yMin + arenaWorldRect.height * vertical,
                0f);
        }

        private void UpdateArenaAtmosphere()
        {
            if (UnityEngine.Application.isPlaying)
            {
                SizeAmbientSprite(windRenderer, 0.24f, 0.12f);
                SizeAmbientSprite(leavesRenderer, 0.09f, 0.13f);
                if (windTraverseRoutine == null && windRenderer != null)
                    windTraverseRoutine = StartCoroutine(DriftAmbientSprite(windRenderer, 0.24f, 0.12f, 0.56f, 0.82f));
                if (leavesTraverseRoutine == null && leavesRenderer != null)
                    leavesTraverseRoutine = StartCoroutine(DriftAmbientSprite(leavesRenderer, 0.09f, 0.13f, 0.55f, 0.8f));
            }
            else
            {
                PlaceAmbientSprite(windRenderer, 0.43f, 0.68f, 0.24f, 0.12f);
                PlaceAmbientSprite(leavesRenderer, 0.57f, 0.59f, 0.09f, 0.13f);
            }
            PlaceAmbientSprite(fireflyRenderer, 0.72f, 0.74f, 0.045f, 0.08f);
        }

        private IEnumerator DriftAmbientSprite(SpriteRenderer renderer, float widthFraction,
            float heightFraction, float minHeight, float maxHeight)
        {
            if (renderer == null) yield break;
            renderer.enabled = false;
            Color baseColor = renderer.color;
            bool leftToRight = Random.value >= 0.5f;

            while (renderer != null)
            {
                while (arenaWorldRect.width <= 0f || arenaWorldRect.height <= 0f)
                    yield return null;

                renderer.enabled = false;
                yield return new WaitForSecondsRealtime(Random.Range(2.2f, 5.2f));
                if (renderer == null) yield break;

                float startX = leftToRight ? -widthFraction * 0.65f : 1f + widthFraction * 0.65f;
                float endX = leftToRight ? 1f + widthFraction * 0.65f : -widthFraction * 0.65f;
                float height = Random.Range(minHeight, maxHeight);
                float phase = Random.Range(0f, Mathf.PI * 2f);
                float duration = Random.Range(5.5f, 7.5f);
                float elapsed = 0f;
                renderer.enabled = true;

                while (elapsed < duration && renderer != null)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    float vertical = height + Mathf.Sin(t * Mathf.PI * 2f + phase) * 0.018f;
                    SetAmbientSpritePosition(renderer, Mathf.Lerp(startX, endX, t), vertical);
                    float fadeIn = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.13f));
                    float fadeOut = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((1f - t) / 0.13f));
                    Color color = baseColor;
                    color.a *= fadeIn * fadeOut;
                    renderer.color = color;
                    yield return null;
                }

                if (renderer != null)
                {
                    renderer.color = baseColor;
                    renderer.enabled = false;
                }
                leftToRight = !leftToRight;
            }
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
                animator.speed = 0.78f;
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
            const float duration = 0.44f;
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
            sprite.color = Color.Lerp(baseColor, elementColor, 0.58f);
            const float duration = 0.26f;
            float elapsed = 0f;
            while (elapsed < duration && sprite != null)
            {
                elapsed += Time.unscaledDeltaTime;
                sprite.color = Color.Lerp(Color.Lerp(baseColor, elementColor, 0.58f), baseColor,
                    Mathf.Clamp01(elapsed / duration));
                yield return null;
            }
            if (sprite != null)
            {
                if (wizardInstance != null && sprite.gameObject == wizardInstance)
                    ApplyWizardFormTint(combat == null ? "neutro" : combat.WizardForm);
                else sprite.color = baseColor;
                damageFlashes.Remove(sprite);
                damageFlashBaseColors.Remove(sprite);
            }
        }
    }
}
