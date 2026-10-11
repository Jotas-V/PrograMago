using System;
using System.Collections;
using System.Text;
using PrograMago.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PrograMago.UnityIntegration
{
    public sealed class GrimoireView : MonoBehaviour
    {
        public const string SceneName = "GrimoireScene";
        [SerializeField] private RectTransform book;
        [SerializeField] private TMP_Text pageTitle, leftText, rightText, pageNumber;
        [SerializeField] private Button previousButton, nextButton, closeButton, hintButton;
        [SerializeField] private Button[] pageButtons;
        [SerializeField] private ScrollRect leftScroll, rightScroll;
        private LearningProgress progress;
        private Action close, save;
        private bool closing;
        private bool pageSelected;
        public int CurrentPage { get; private set; }
        public bool IsPageReadable => progress != null && progress.CanReadGrimoirePage(CurrentPage);

        private void Awake()
        {
            foreach (var scroll in new[] { leftScroll, rightScroll }) {
                if (scroll == null || scroll.verticalScrollbar != null) continue;
                scroll.verticalScrollbar = ThinScrollbar.Create(scroll.transform, "PageScrollbar");
                var max = scroll.viewport.offsetMax; max.x -= 20; scroll.viewport.offsetMax = max;
            }
        }

        public void Configure(LearningProgress value, Action onClose, Action onSave)
        {
            progress = value ?? throw new ArgumentNullException(nameof(value));
            close = onClose; save = onSave;
            previousButton.onClick.AddListener(() => SelectPage(CurrentPage - 1));
            nextButton.onClick.AddListener(() => SelectPage(CurrentPage + 1));
            closeButton.onClick.AddListener(RequestClose);
            hintButton.onClick.AddListener(RevealHint);
            for (int i = 0; i < pageButtons.Length; i++) {
                int page = i; pageButtons[i].onClick.AddListener(() => SelectPage(page));
            }
            SelectPage(progress.CurrentBattleIndex);
            GameAudio.Play(GameSound.GrimoireOpen);
            StartCoroutine(AnimateOpening());
        }

        private IEnumerator AnimateOpening()
        {
            float elapsed = 0;
            while (elapsed < .35f) {
                elapsed += Time.unscaledDeltaTime;
                float fraction = Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / .35f));
                book.localScale = new Vector3(Mathf.Lerp(.06f, 1, fraction), Mathf.Lerp(.92f, 1, fraction), 1);
                yield return null;
            }
            book.localScale = Vector3.one;
        }

        public void SelectPage(int index)
        {
            if (progress == null || closing || index < 0 || index >= progress.Path.Battles.Count) return;
            if (pageSelected && CurrentPage != index) GameAudio.Play(GameSound.PageTurn);
            pageSelected = true;
            CurrentPage = index;
            previousButton.interactable = index > 0;
            nextButton.interactable = index < progress.Path.Battles.Count - 1;
            pageNumber.text = $"Página {index + 1} de {progress.Path.Battles.Count}";
            for (int i = 0; i < pageButtons.Length; i++)
                pageButtons[i].GetComponentInChildren<TMP_Text>().text = i == index ? $"<b>[{i + 1}]</b>" : (i + 1).ToString();
            Render();
            Canvas.ForceUpdateCanvases();
            leftScroll.verticalNormalizedPosition = rightScroll.verticalNormalizedPosition = 1;
        }

        private void Render()
        {
            foreach (var runes in book.GetComponentsInChildren<GrimoireRuneWriting>(true))
                runes.gameObject.SetActive(!IsPageReadable);
            hintButton.interactable = IsPageReadable && progress.GetRevealedHintCount(CurrentPage) < progress.Path.Battles[CurrentPage].Hints.Count;
            if (!IsPageReadable) {
                pageTitle.text = $"Conhecimento selado · Fase {CurrentPage + 1}";
                leftText.text = "<b>Runas desconhecidas</b>\n\nVocê ainda não aprendeu a decifrar esta escrita mágica.";
                rightText.text = $"<b>Conhecimento selado</b>\n\nConclua a fase {CurrentPage} para compreender estas runas.";
                hintButton.GetComponentInChildren<TMP_Text>().text = "Conhecimento selado";
                return;
            }
            var battle = progress.Path.Battles[CurrentPage];
            var lesson = battle.Lesson;
            pageTitle.text = $"Fase {CurrentPage + 1} · {lesson.Title}";
            leftText.text = $"<b>O QUE VAMOS APRENDER</b>\n{lesson.Concept}\n\n<b>ENTENDA</b>\n{lesson.WhatItIs}\n\n<b>POR QUE USAR?</b>\n{lesson.Purpose}\n\n<b>NO JOGO</b>\n{lesson.GameEffect}";
            var text = new StringBuilder($"<b>SUA TAREFA</b>\n{lesson.Task}\n\n<b>EXEMPLO DE OUTRO CONTEXTO</b>\n{lesson.UsageExample}\n\n<b>DICAS</b>\n");
            int count = progress.GetRevealedHintCount(CurrentPage);
            if (count == 0) text.Append("Tente aplicar o conceito. Se precisar de orientação, use o botão abaixo.");
            for (int i = 0; i < count; i++) text.Append($"\n<b>{i + 1}/{battle.Hints.Count}</b> · {battle.Hints[i]}\n");
            rightText.text = text.ToString();
            hintButton.GetComponentInChildren<TMP_Text>().text = count < battle.Hints.Count
                ? $"Preciso de uma dica ({count}/{battle.Hints.Count})" : "Todas as dicas disponíveis";
        }

        public void RevealHint()
        {
            if (!IsPageReadable || closing) return;
            progress.RevealGrimoireHint(CurrentPage);
            save?.Invoke(); Render(); Canvas.ForceUpdateCanvases();
            rightScroll.verticalNormalizedPosition = 0;
        }
        public void RequestClose()
        {
            if (closing) return;
            closing = true; close?.Invoke();
        }
        private void Update()
        {
            if (progress != null && Keyboard.current?.escapeKey.wasPressedThisFrame == true) RequestClose();
        }
    }
}
