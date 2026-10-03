using PrograMago.Application;
using PrograMago.Domain;
using PrograMago.Language;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrograMago.UnityIntegration
{
    public sealed partial class GameplayBootstrapper
    {
        private const float DefinitionStripWidth = 692f;
        private const float CombatActionStripStartX = 704f;
        private const float TimelineContentWidth = 884f;
        private const float TimelineHeight = 84f;
        private const float TimelineButtonHeight = 48f;
        private const float TimelineButtonBottom = 8f;
        private static readonly float[] CodeFileButtonWidths = { 144f, 160f, 184f };

        private void EnsureWorkspaceControls()
        {
            if (definitionStrip == null || editorPanel == null) return;
            LayoutCodeFileButtons();
            EnsureWorkspaceTitles();
            preparationWorkspaceButton = FindOrCreateWorkspaceButton(preparationWorkspaceButton,
                "PreparationWorkspaceButton", "Ajustes", 536f);
            approveMethodButton = FindOrCreateWorkspaceButton(approveMethodButton,
                "ApproveMethodButton", "Aprovar método", 0f);
            if (methodsWorkspaceButton != null)
            {
                LayoutWorkspaceButton(methodsWorkspaceButton, 536f, 144f);
                methodsWorkspaceButton.onClick.RemoveAllListeners();
                methodsWorkspaceButton.gameObject.SetActive(false);
            }
            preparationWorkspaceButton.onClick.RemoveAllListeners();
            preparationWorkspaceButton.onClick.AddListener(() => SelectWorkspace(WorkspaceArea.Preparation));
            if (approveMethodButton != null)
            {
                approveMethodButton.onClick.RemoveAllListeners();
                approveMethodButton.gameObject.SetActive(false);
            }
            LayoutWorkspaceButton(preparationWorkspaceButton, 536f, 144f);
            RefreshCodeBlockButtons();
            UpdateWorkspaceUi();
        }

        private void LayoutCodeFileButtons()
        {
            float x = 0f;
            for (int index = 0; index < codeBlockButtons.Length; index++)
            {
                if (codeBlockButtons[index] == null) continue;
                RectTransform rect = codeBlockButtons[index].GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
                rect.anchoredPosition = new Vector2(x, TimelineButtonBottom);
                rect.sizeDelta = new Vector2(CodeFileButtonWidths[index], TimelineButtonHeight);
                x += CodeFileButtonWidths[index] + 12f;
            }
        }

        private void EnsureWorkspaceTitles()
        {
            Transform legacyTitle = definitionStrip.Find("DefinitionBlocksTitle");
            if (legacyTitle != null) legacyTitle.gameObject.SetActive(false);
            LayoutWorkspaceTitle("PhaseCodeGroupTitle", "BLOCOS DE CÓDIGO", 0f, 512f);
            LayoutWorkspaceTitle("WizardConfigGroupTitle", "AJUSTES", 536f, 144f);
        }

        private void LayoutWorkspaceTitle(string name, string text, float x, float width)
        {
            Transform existing = definitionStrip.Find(name);
            TMP_Text label = existing == null
                ? CreateLabel(name, definitionStrip, text, Vector2.zero, Vector2.zero)
                : existing.GetComponent<TMP_Text>();
            label.text = text;
            label.color = new Color32(218, 219, 231, 255);
            label.fontStyle = FontStyles.Bold;
            label.fontSizeMin = 12f;
            label.fontSizeMax = 14f;
            label.alignment = TextAlignmentOptions.BottomLeft;
            label.raycastTarget = false;
            RectTransform rect = label.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(x + 4f, 64f);
            rect.sizeDelta = new Vector2(width - 8f, 16f);
        }

        private Button FindOrCreateWorkspaceButton(Button current, string name, string label, float x)
        {
            if (current == null)
            {
                GameObject existing = GameObject.Find(name);
                if (existing != null) current = existing.GetComponent<Button>();
            }
            if (current == null)
            {
                current = CreateArrow(name, definitionStrip, label, x, 100f);
                current.gameObject.name = name;
            }
            TMP_Text text = current.GetComponentInChildren<TMP_Text>();
            if (text != null) text.text = label;
            return current;
        }

        private void LayoutWorkspaceButton(Button button, float x, float width)
        {
            if (button == null) return;
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(x, TimelineButtonBottom);
            rect.sizeDelta = new Vector2(width, TimelineButtonHeight);
        }

        private void SelectWorkspace(WorkspaceArea area)
        {
            StoreWorkspaceText();
            selectedCombatBlock = -1;
            workspaceArea = area;
            if (area == WorkspaceArea.Classes)
            {
                codeBlocks.Select(codeBlocks.ActiveIndex);
                codeInput.SetTextWithoutNotify(codeBlocks.ActiveText);
            }
            else if (area == WorkspaceArea.Preparation)
            {
                codeInput.SetTextWithoutNotify(preparationCode);
                feedbackText.text = "Ajustes · redistribua atributos antes da batalha usando os setters da classe Mago.\n" +
                    $"Exemplo: {(declaredMago == null ? "mago" : declaredMago.InstanceName)}.setAlcance(5);";
            }
            UpdateWorkspaceUi();
            RefreshCodeBlockButtons();
            RefreshCombatActionButtons();
        }

        private void StoreWorkspaceText()
        {
            if (codeInput == null) return;
            if (workspaceArea == WorkspaceArea.Classes || workspaceArea == WorkspaceArea.Strategy)
            {
                if (workspaceArea == WorkspaceArea.Classes && selectedCombatBlock >= 0 && selectedCombatBlock < combatCodeBlocks.Count)
                    combatCodeBlocks[selectedCombatBlock] = codeInput.text;
                else
                    codeBlocks.SetActiveText(codeInput.text);
            }
            else if (workspaceArea == WorkspaceArea.Methods)
            {
                methodDraft = codeInput.text;
            }
            else
            {
                preparationCode = codeInput.text;
            }
        }

        private void ApproveMethod()
        {
            if (workspaceArea != WorkspaceArea.Methods || combat != null) return;
            StoreWorkspaceText();
            if (!magoMethodBook.TryApprove(methodDraft, out string error))
            {
                feedbackText.color = new Color32(156, 39, 49, 255);
                feedbackText.text = error;
                return;
            }
            methodDraft = string.Empty;
            codeInput.SetTextWithoutNotify(string.Empty);
            feedbackText.color = new Color32(40, 82, 100, 255);
            feedbackText.text = $"Método aprovado ({magoMethodBook.ApprovedSources.Count}/{MagoMethodBook.Names.Length}).";
            UpdateWorkspaceUi();
            SaveProgress();
        }

        private bool TryApplyPreparation()
        {
            StoreWorkspaceText();
            if (declaredMago == null)
            {
                feedbackText.color = new Color32(156, 39, 49, 255);
                feedbackText.text = "Conclua a classe e a instância do Mago antes da preparação.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(preparationCode))
            {
                preparedMago = null;
                CurrentMago = declaredMago;
                RenderWizard();
                return true;
            }
            ValidationCriterion preparationCriterion = learningFlowPresenter.Progress.CurrentBattle.Criterion ==
                ValidationCriterion.DefineAndCallSpellMethod
                ? ValidationCriterion.DefineAndCallSpellMethod : ValidationCriterion.AddMagoSetters;
            string setterSource = preparationCriterion == ValidationCriterion.AddMagoSetters
                ? codeBlocks.Snapshot()[0] : SourceCode;
            TokenizationResult setterTokens = new CodeTokenizer().Tokenize(setterSource);
            if (!setterTokens.IsSuccess)
            {
                feedbackText.color = new Color32(156, 39, 49, 255);
                feedbackText.text = "Não foi possível ler a classe Mago para conferir os setters.";
                return false;
            }
            var setterExercise = new ExerciseDefinition("mago-setters", "Mago", "Valide os setters do Mago.");
            ExerciseValidationResult setters = new ExerciseCodeValidator().Validate(
                setterTokens.Tokens, setterExercise, preparationCriterion);
            if (!setters.IsSuccess)
            {
                feedbackText.color = new Color32(156, 39, 49, 255);
                feedbackText.text = "Ajustes indisponíveis: " + setters.Diagnostic.Detail;
                return false;
            }
            var learnedSetters = new MagoMethodBook(new[]
            {
                MagoMethodBook.Examples[0], MagoMethodBook.Examples[1], MagoMethodBook.Examples[2],
                MagoMethodBook.Examples[3], MagoMethodBook.Examples[4]
            });
            PreparationResult result = MagoPreparation.Evaluate(declaredMago, learnedSetters, preparationCode);
            if (!result.IsSuccess)
            {
                feedbackText.color = new Color32(156, 39, 49, 255);
                feedbackText.text = "Preparação inválida: " + result.Error;
                return false;
            }
            preparedMago = result.Mago;
            CurrentMago = preparedMago;
            RenderWizard();
            return true;
        }

        private void UpdateWorkspaceUi()
        {
            if (codeInput == null) return;
            bool editing = combat == null && interactionEnabled;
            bool classesEditable = workspaceArea == WorkspaceArea.Classes && selectedCombatBlock >= 0 ||
                workspaceArea == WorkspaceArea.Classes && IsCodeBlockEditable(codeBlocks.ActiveIndex);
            codeInput.interactable = editing && (workspaceArea == WorkspaceArea.Preparation || classesEditable ||
                workspaceArea == WorkspaceArea.Strategy && IsCodeBlockEditable(2));
            for (int index = 0; index < codeBlockButtons.Length; index++)
                if (codeBlockButtons[index] != null)
                    codeBlockButtons[index].interactable = editing && IsCodeBlockEditable(index);
            bool adjustmentsAvailable = learningFlowPresenter != null &&
                learningFlowPresenter.Progress.CurrentBattleIndex >= 4;
            if (preparationWorkspaceButton != null)
                preparationWorkspaceButton.interactable = editing && adjustmentsAvailable;
            if (approveMethodButton != null)
            {
                approveMethodButton.gameObject.SetActive(false);
                approveMethodButton.interactable = false;
            }
            if (preparationWorkspaceButton != null)
                preparationWorkspaceButton.targetGraphic.color = workspaceArea == WorkspaceArea.Preparation
                    ? new Color32(91, 74, 190, 255)
                    : new Color32(39, 77, 92, 255);
        }

        private void FocusEditableDefinitionBlock(int index)
        {
            if (codeInput == null || index < 0 || index >= codeBlockButtons.Length ||
                !IsCodeBlockEditable(index)) return;

            StoreWorkspaceText();
            workspaceArea = WorkspaceArea.Classes;
            selectedCombatBlock = -1;
            codeBlocks.Select(index);
            codeInput.SetTextWithoutNotify(codeBlocks.ActiveText);
            RefreshCodeBlockButtons();
        }

        private bool IsCodeBlockEditable(int index)
        {
            if (learningFlowPresenter == null) return index < 2;
            if (index == 0) return true;
            if (index == 2)
                return learningFlowPresenter.Progress.CurrentBattle.Criterion ==
                    ValidationCriterion.UsePolymorphicMagoReference;
            return index == 1 &&
                (learningFlowPresenter.Progress.CurrentBattleIndex < 3 ||
                 learningFlowPresenter.Progress.CurrentBattle.Criterion ==
                    ValidationCriterion.ConstructAndInstantiateEnemy);
        }

        private void ResetWorkspace()
        {
            magoMethodBook = new MagoMethodBook();
            methodDraft = string.Empty;
            preparationCode = string.Empty;
            declaredMago = null;
            preparedMago = null;
            interactionEnabled = true;
            workspaceArea = WorkspaceArea.Classes;
        }
    }
}
