using PrograMago.Application;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrograMago.UnityIntegration
{
    public sealed partial class GameplayBootstrapper
    {
        private void EnsureWorkspaceControls()
        {
            if (definitionStrip == null || editorPanel == null) return;
            LayoutCodeFileButtons();
            EnsureWorkspaceTitles();
            methodsWorkspaceButton = FindOrCreateWorkspaceButton(methodsWorkspaceButton,
                "MethodsWorkspaceButton", "Métodos", 420f);
            preparationWorkspaceButton = FindOrCreateWorkspaceButton(preparationWorkspaceButton,
                "PreparationWorkspaceButton", "Preparação", 546f);
            approveMethodButton = FindOrCreateWorkspaceButton(approveMethodButton,
                "ApproveMethodButton", "Aprovar método", 680f);
            methodsWorkspaceButton.onClick.RemoveAllListeners();
            methodsWorkspaceButton.onClick.AddListener(() => SelectWorkspace(WorkspaceArea.Methods));
            preparationWorkspaceButton.onClick.RemoveAllListeners();
            preparationWorkspaceButton.onClick.AddListener(() => SelectWorkspace(WorkspaceArea.Preparation));
            approveMethodButton.onClick.RemoveAllListeners();
            approveMethodButton.onClick.AddListener(ApproveMethod);
            LayoutWorkspaceButton(methodsWorkspaceButton, 420f, 120f);
            LayoutWorkspaceButton(preparationWorkspaceButton, 546f, 128f);
            LayoutWorkspaceButton(approveMethodButton, 680f, 150f);
            UpdateWorkspaceUi();
        }

        private void LayoutCodeFileButtons()
        {
            for (int index = 0; index < codeBlockButtons.Length; index++)
            {
                if (codeBlockButtons[index] == null) continue;
                RectTransform rect = codeBlockButtons[index].GetComponent<RectTransform>();
                rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
                rect.anchoredPosition = new Vector2(index * 130f, 6f);
                rect.sizeDelta = new Vector2(124f, 40f);
            }
        }

        private void EnsureWorkspaceTitles()
        {
            Transform legacyTitle = definitionStrip.Find("DefinitionBlocksTitle");
            if (legacyTitle != null) legacyTitle.gameObject.SetActive(false);
            LayoutWorkspaceTitle("PhaseCodeGroupTitle", "CÓDIGO DA FASE", 0f, 384f);
            LayoutWorkspaceTitle("WizardConfigGroupTitle", "CONFIGURAÇÃO DO MAGO", 420f, 410f);
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
            label.fontSizeMin = 10f;
            label.fontSizeMax = 12f;
            label.alignment = TextAlignmentOptions.BottomLeft;
            label.raycastTarget = false;
            RectTransform rect = label.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(x + 4f, 50f);
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
            rect.anchoredPosition = new Vector2(x, 9f);
            rect.sizeDelta = new Vector2(width, 41f);
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
            else if (area == WorkspaceArea.Methods)
            {
                codeInput.SetTextWithoutNotify(methodDraft);
                feedbackText.text = "Métodos · escreva um método por vez e clique em Aprovar método.\n" +
                    $"Progresso: {magoMethodBook.ApprovedSources.Count}/{MagoMethodBook.Names.Length}.";
            }
            else
            {
                codeInput.SetTextWithoutNotify(preparationCode);
                feedbackText.text = "Preparação · use apenas setters já aprovados, uma vez antes da batalha.\n" +
                    $"Exemplo: {(declaredMago == null ? "mago" : declaredMago.InstanceName)}.setAlcance(5);";
            }
            UpdateWorkspaceUi();
            RefreshCodeBlockButtons();
            RefreshCombatActionButtons();
        }

        private void StoreWorkspaceText()
        {
            if (codeInput == null) return;
            if (workspaceArea == WorkspaceArea.Classes)
            {
                if (selectedCombatBlock >= 0 && selectedCombatBlock < combatCodeBlocks.Count)
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
                return true;
            }
            PreparationResult result = MagoPreparation.Evaluate(declaredMago, magoMethodBook, preparationCode);
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
            bool classesEditable = editing && (!classesLocked || selectedCombatBlock >= 0);
            codeInput.interactable = editing && (workspaceArea != WorkspaceArea.Classes || classesEditable);
            for (int index = 0; index < codeBlockButtons.Length; index++)
                if (codeBlockButtons[index] != null) codeBlockButtons[index].interactable = classesEditable;
            if (methodsWorkspaceButton != null) methodsWorkspaceButton.interactable = editing;
            if (preparationWorkspaceButton != null) preparationWorkspaceButton.interactable = editing;
            if (approveMethodButton != null)
            {
                approveMethodButton.gameObject.SetActive(workspaceArea == WorkspaceArea.Methods);
                approveMethodButton.interactable = editing && workspaceArea == WorkspaceArea.Methods;
            }
            if (methodsWorkspaceButton != null)
                methodsWorkspaceButton.targetGraphic.color = workspaceArea == WorkspaceArea.Methods
                    ? new Color32(91, 74, 190, 255)
                    : new Color32(39, 77, 92, 255);
            if (preparationWorkspaceButton != null)
                preparationWorkspaceButton.targetGraphic.color = workspaceArea == WorkspaceArea.Preparation
                    ? new Color32(91, 74, 190, 255)
                    : new Color32(39, 77, 92, 255);
            if (workspaceArea == WorkspaceArea.Classes && classesLocked && editing)
            {
                feedbackText.color = new Color32(90, 86, 105, 255);
                feedbackText.text = "Classes protegidas após a fase 3. Use Métodos e Preparação para ajustar o Mago.";
            }
        }

        private void ResetWorkspace()
        {
            magoMethodBook = new MagoMethodBook();
            methodDraft = string.Empty;
            preparationCode = string.Empty;
            declaredMago = null;
            preparedMago = null;
            classesLocked = false;
            interactionEnabled = true;
            workspaceArea = WorkspaceArea.Classes;
        }
    }
}
