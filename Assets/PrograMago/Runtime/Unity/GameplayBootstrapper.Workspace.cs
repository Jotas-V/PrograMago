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
            methodsWorkspaceButton = FindOrCreateWorkspaceButton(methodsWorkspaceButton,
                "MethodsWorkspaceButton", "Métodos", 314f);
            preparationWorkspaceButton = FindOrCreateWorkspaceButton(preparationWorkspaceButton,
                "PreparationWorkspaceButton", "Preparação", 418f);
            approveMethodButton = FindOrCreateWorkspaceButton(approveMethodButton,
                "ApproveMethodButton", "Aprovar método", 526f);
            methodsWorkspaceButton.onClick.RemoveAllListeners();
            methodsWorkspaceButton.onClick.AddListener(() => SelectWorkspace(WorkspaceArea.Methods));
            preparationWorkspaceButton.onClick.RemoveAllListeners();
            preparationWorkspaceButton.onClick.AddListener(() => SelectWorkspace(WorkspaceArea.Preparation));
            approveMethodButton.onClick.RemoveAllListeners();
            approveMethodButton.onClick.AddListener(ApproveMethod);
            LayoutWorkspaceButton(methodsWorkspaceButton, 314f, 100f);
            LayoutWorkspaceButton(preparationWorkspaceButton, 418f, 104f);
            LayoutWorkspaceButton(approveMethodButton, 526f, 132f);
            UpdateWorkspaceUi();
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
                approveMethodButton.interactable = editing && workspaceArea == WorkspaceArea.Methods;
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