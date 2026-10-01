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
        private const float CharacterAnimationSpeed = 0.30f;
        private TCC40VisualCatalog visualCatalog;
        private Transform arenaAtmosphereRoot;
        private SpriteRenderer windRenderer;
        private SpriteRenderer leavesRenderer;
        private SpriteRenderer fireflyRenderer;
        private Coroutine windTraverseRoutine;
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
            foreach (Image image in Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                switch (image.name)
                {
                    case "CodeEditorPanel": case "TutorialPanel": case "VictoryCard":
                    case "RestartProgressPanel":
                        ApplyPaperPanel(image, new Color32(231, 216, 177, 255)); break;
                    case "MagoStatusCard": case "EnemyStatusCard":
                        ApplyPaperPanel(image, new Color32(233, 220, 184, 245)); break;
                    case "CodeTimeline":
                        ApplyPaperPanel(image, new Color32(193, 169, 126, 255)); break;
                    case "CombatMessageCard": case "CombatDefeatOverlay":
                        ApplyPaperPanel(image, new Color32(231, 216, 177, 255)); break;
                    case "CodeInput": case "EnemyGuidePanel":
                        SetTransparentPanelImage(image, image.name == "CodeInput"); break;
                    case "Viewport":
                        if (image.transform.parent.name == "CodeTimeline") { image.sprite = null; image.color = Color.white; image.raycastTarget = true; var mask = image.GetComponent<Mask>(); if (mask != null) mask.showMaskGraphic = false; }
                        break;
                }
            }
            if (codeInput != null)
            {
                var rect = codeInput.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(.025f, .13f);
                rect.anchorMax = new Vector2(.975f, .97f);
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                codeInput.targetGraphic = null;
                if (codeInput.textComponent != null) codeInput.textComponent.color = new Color32(47, 39, 30, 255);
                if (codeInput.placeholder != null) codeInput.placeholder.color = new Color32(103, 87, 66, 255);
            }
            foreach (string name in new[]{"BottomArea", "MagoStatusCard", "EnemyStatusCard", "CombatMessageCard", "VictoryCard", "RestartProgressPanel"})
                SetChildTextColor(name, new Color32(47, 39, 30, 255));
            foreach (Button button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                ApplyButtonArt(button, button.name == "BattleButton");
                var label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null) { LayoutButtonLabel(button, label); label.color = new Color32(255, 245, 214, 255); label.fontStyle = FontStyles.Bold; }
            }
            foreach (var panel in Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (panel.name != "VictoryCard") continue;
                panel.anchorMin = new Vector2(.15f, .19f);
                panel.anchorMax = new Vector2(.85f, .81f);
                panel.pivot = new Vector2(.5f, .5f);
                panel.offsetMin = panel.offsetMax = Vector2.zero;
                LayoutPaperText(victoryTitleText, new Vector2(.07f,.73f), new Vector2(.93f,.91f), 38);
                LayoutPaperText(victoryAchievementText, new Vector2(.07f,.49f), new Vector2(.93f,.70f), 24);
                LayoutPaperText(victoryReviewText, new Vector2(.07f,.25f), new Vector2(.93f,.47f), 22);
                if (nextBattleButton != null)
                {
                    var rect = nextBattleButton.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(.27f,.07f); rect.anchorMax = new Vector2(.73f,.19f);
                    rect.offsetMin = rect.offsetMax = Vector2.zero;
                }
                panel.SetAsLastSibling();
            }
            if (victoryOverlay != null)
            {
                var shade = victoryOverlay.GetComponent<Image>();
                if (shade != null) { shade.sprite = null; shade.color = new Color32(0, 0, 0, 196); shade.raycastTarget = true; }
            }
        }

        private static void LayoutPaperText(TMP_Text text, Vector2 min, Vector2 max, float size)
        {
            if (text == null) return;
            var rect = text.rectTransform;
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            text.alignment = TextAlignmentOptions.Center;
            text.enableAutoSizing = true; text.fontSizeMin = 16; text.fontSizeMax = size;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Truncate;
            text.color = new Color32(47, 39, 30, 255);
        }

        private void ApplyWorkspaceGround()
        {
            var area = GameObject.Find("BottomArea");
            if (area == null) return;
            var image = area.GetComponent<Image>();
            if (image == null) image = area.AddComponent<Image>();
            image.sprite = null; image.type = Image.Type.Simple;
            image.color = new Color32(45, 48, 35, 255); image.raycastTarget = false;
        }

        private static void ApplyPaperPanel(Image image, Color color)
        {
            image.sprite = null; image.type = Image.Type.Simple;
            image.color = color; image.preserveAspect = false; image.raycastTarget = false;
            var border = image.transform.Find("PaperBorder");
            if (border == null)
            {
                var go = new GameObject("PaperBorder", typeof(RectTransform), typeof(CanvasRenderer), typeof(PaperPanelBorder));
                border = go.transform; border.SetParent(image.transform, false);
            }
            var rect = border.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            border.SetAsFirstSibling();
        }

        private static void SetTransparentPanelImage(Image image, bool receivesInput)
        {
            if (image == null) return;
            image.sprite = null; image.type = Image.Type.Simple;
            image.color = Color.clear; image.preserveAspect = false; image.raycastTarget = receivesInput;
        }

        private void SetChildTextColor(string panelName, Color color)
        {
            foreach (Transform item in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (item.name == panelName)
                    foreach (TMP_Text label in item.GetComponentsInChildren<TMP_Text>(true))
                        if (label.GetComponentInParent<Button>() == null) label.color = color;
        }

        private void ApplyCreatedPanelArt(string name, Image image)
        {
            if (name == "CodeTimeline") ApplyPaperPanel(image, new Color32(193, 169, 126, 255));
            else if (name == "MagoStatusCard" || name == "EnemyStatusCard" || name == "CombatMessageCard" || name == "CombatDefeatOverlay")
                ApplyPaperPanel(image, new Color32(231, 216, 177, 255));
        }

        private void ApplyButtonArt(Button button, bool battleButtonStyle)
        {
            if (button == null) return;
            var image = button.GetComponent<Image>();
            if (image == null)
            {
                var previous = button.targetGraphic;
                var art = button.transform.Find("ButtonArt");
                if (art == null)
                {
                    var go = new GameObject("ButtonArt", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    art = go.transform; art.SetParent(button.transform, false);
                    var rect = go.GetComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                    rect.offsetMin = rect.offsetMax = Vector2.zero;
                }
                image = art.GetComponent<Image>(); art.SetAsFirstSibling();
                if (previous != null && previous != image) { previous.enabled = false; previous.raycastTarget = false; }
            }
            var arrow = button.GetComponent<TimelineArrowGraphic>();
            if (arrow != null) { arrow.enabled = false; arrow.raycastTarget = false; }
            ApplyPaperPanel(image, Color.white);
            image.raycastTarget = true; button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = battleButtonStyle ? new Color32(65, 88, 53, 255) : new Color32(105, 77, 47, 255);
            colors.highlightedColor = new Color32(128, 101, 63, 255);
            colors.pressedColor = new Color32(66, 51, 34, 255);
            colors.selectedColor = colors.normalColor;
            colors.disabledColor = new Color32(102, 97, 79, 255);
            colors.fadeDuration = .12f; button.colors = colors;
            if (button.GetComponent<ButtonJuice>() == null) button.gameObject.AddComponent<ButtonJuice>();
        }

        private static void LayoutButtonLabel(Button button, TMP_Text label)
        {
            var rect = label.rectTransform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(7, 4); rect.offsetMax = new Vector2(-7, -4);
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true; label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.fontSizeMin = 10; label.fontSizeMax = button.name == "NextBattleButton" ? 24 : 18;
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
            float scale = Mathf.Min(arenaWorldRect.width * widthFraction / spriteSize.x,
                arenaWorldRect.height * heightFraction / spriteSize.y);
            renderer.transform.localScale = new Vector3(
                scale / Mathf.Max(0.0001f, parentScale.x),
                scale / Mathf.Max(0.0001f, parentScale.y),
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
                SizeAmbientSprite(windRenderer, 0.14f, 0.23f);
                SizeAmbientSprite(leavesRenderer, 0.06f, 0.16f);
                if (windTraverseRoutine == null && windRenderer != null)
                    windTraverseRoutine = StartCoroutine(DriftAmbientSprite(windRenderer, 0.14f, 0.23f, 0.56f, 0.72f));
            }
            else
            {
                PlaceAmbientSprite(windRenderer, 0.43f, 0.68f, 0.14f, 0.23f);
                PlaceAmbientSprite(leavesRenderer, 0.39f, 0.655f, 0.06f, 0.16f);
            }
            PlaceAmbientSprite(fireflyRenderer, 0.72f, 0.74f, 0.045f, 0.08f);
        }

        private IEnumerator DriftAmbientSprite(SpriteRenderer renderer, float widthFraction,
            float heightFraction, float minHeight, float maxHeight)
        {
            if (renderer == null) yield break;
            renderer.enabled = false;
            Color baseColor = renderer.color;
            SpriteRenderer follower = renderer == windRenderer ? leavesRenderer : null;
            Color followerBaseColor = follower == null ? Color.white : follower.color;
            if (follower != null) follower.enabled = false;

            while (renderer != null)
            {
                while (arenaWorldRect.width <= 0f || arenaWorldRect.height <= 0f)
                    yield return null;

                renderer.enabled = false;
                if (follower != null) follower.enabled = false;
                yield return new WaitForSecondsRealtime(Random.Range(1.8f, 3.5f));
                if (renderer == null) yield break;

                float startX = -widthFraction * 0.65f;
                float endX = 1f + widthFraction * 0.65f + 0.04f;
                float height = Random.Range(minHeight, maxHeight);
                float phase = Random.Range(0f, Mathf.PI * 2f);
                float duration = Random.Range(6.5f, 8.5f);
                float elapsed = 0f;
                renderer.enabled = true;
                if (follower != null) follower.enabled = true;

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
                    if (follower != null)
                    {
                        SetAmbientSpritePosition(follower, Mathf.Lerp(startX, endX, t) - 0.04f,
                            vertical - 0.025f);
                        Color followerColor = followerBaseColor;
                        followerColor.a *= fadeIn * fadeOut;
                        follower.color = followerColor;
                    }
                    yield return null;
                }

                if (renderer != null)
                {
                    renderer.color = baseColor;
                    renderer.enabled = false;
                }
                if (follower != null)
                {
                    follower.color = followerBaseColor;
                    follower.enabled = false;
                }
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
                animator.speed = CharacterAnimationSpeed;
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
