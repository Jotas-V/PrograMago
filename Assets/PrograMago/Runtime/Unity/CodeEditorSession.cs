using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PrograMago.UnityIntegration
{
    [RequireComponent(typeof(TMP_InputField))]
    public sealed class CodeEditorSession : MonoBehaviour
    {
        private struct Snapshot { public string Text; public int Anchor, Focus; public float Scroll; }
        private sealed class Document {
            public Snapshot Current;
            public readonly List<Snapshot> Undo = new List<Snapshot>();
        }
        private readonly Dictionary<string, Document> documents = new Dictionary<string, Document>();
        private TMP_InputField input;
        private Document document;
        private bool restoring;
        private float lastEdit = float.NegativeInfinity;
        private int lastSimpleEdit;
        private int lastEditEnd;

        private void Awake()
        {
            input = GetComponent<TMP_InputField>();
            input.richText = false;
            input.textComponent.richText = false;
            input.textComponent.enableAutoSizing = false;
            input.textComponent.margin = Vector4.zero;
            input.textComponent.alignment = TextAlignmentOptions.TopLeft;
            input.textComponent.overflowMode = TextOverflowModes.Overflow;
            input.onFocusSelectAll = false;
            input.resetOnDeActivation = false;
            input.restoreOriginalTextOnEscape = false;
            input.scrollSensitivity = 3;
            if (input.textViewport.GetComponent<UnityEngine.UI.RectMask2D>() == null)
                input.textViewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var max = input.textViewport.offsetMax; max.x = Mathf.Min(max.x, -22); input.textViewport.offsetMax = max;
            var textRect = input.textComponent.rectTransform;
            textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
            // TMP computes normalized scroll from both rectangles' local coordinates.
            textRect.pivot = input.textViewport.pivot; textRect.offsetMin = textRect.offsetMax = Vector2.zero;
            input.verticalScrollbar = ThinScrollbar.Create(transform, "CodeScrollbar");
            // TMP's normalized input scroll runs top-to-bottom.
            input.verticalScrollbar.direction = UnityEngine.UI.Scrollbar.Direction.TopToBottom;
            input.onValueChanged.AddListener(Changed);
            LoadDocument("initial", input.text);
        }

        private Snapshot Capture() => new Snapshot {
            Text = input.text, Anchor = input.selectionStringAnchorPosition,
            Focus = input.selectionStringFocusPosition,
            Scroll = input.verticalScrollbar != null ? input.verticalScrollbar.value : 0
        };

        public void LoadDocument(string key, string source)
        {
            if (document != null && input.text == document.Current.Text) document.Current = Capture();
            source ??= string.Empty;
            if (!documents.TryGetValue(key, out document) || document.Current.Text != source) {
                document = new Document { Current = new Snapshot { Text = source } };
                documents[key] = document;
            }
            lastSimpleEdit = 0; lastEdit = float.NegativeInfinity;
            Restore(document.Current, false);
        }

        private void Changed(string text)
        {
            if (restoring || document == null || text == document.Current.Text) return;
            string before = document.Current.Text;
            int prefix = 0, suffix = 0;
            while (prefix < before.Length && prefix < text.Length && before[prefix] == text[prefix]) prefix++;
            while (suffix < before.Length - prefix && suffix < text.Length - prefix &&
                   before[before.Length - 1 - suffix] == text[text.Length - 1 - suffix]) suffix++;
            int added = text.Length - prefix - suffix, removed = before.Length - prefix - suffix;
            int simple = added == 1 && removed == 0 ? 1 : added == 0 && removed == 1 ? -1 : 0;
            bool selected = document.Current.Anchor != document.Current.Focus;
            bool group = !selected && simple != 0 && simple == lastSimpleEdit &&
                Time.unscaledTime - lastEdit < .7f && (prefix == lastEditEnd || simple < 0 && prefix + 1 == lastEditEnd);
            if (!group) {
                document.Undo.Add(document.Current);
                if (document.Undo.Count > 100) document.Undo.RemoveAt(0);
            }
            document.Current = Capture();
            lastEdit = Time.unscaledTime; lastSimpleEdit = simple; lastEditEnd = prefix + added;
        }

        public bool Undo()
        {
            if (document == null || document.Undo.Count == 0 || !input.interactable || input.readOnly) return false;
            int index = document.Undo.Count - 1;
            var snapshot = document.Undo[index]; document.Undo.RemoveAt(index);
            Restore(snapshot, true); document.Current = snapshot;
            lastSimpleEdit = 0; lastEdit = float.NegativeInfinity;
            input.onValueChanged.Invoke(input.text);
            return true;
        }

        public void ScrollToStart()
        {
            input.DeactivateInputField();
            input.stringPosition = 0;
            input.caretPosition = 0;
            input.textComponent.rectTransform.anchoredPosition = Vector2.zero;
            input.verticalScrollbar.value = 0;
            input.ForceLabelUpdate();
            input.textComponent.rectTransform.anchoredPosition = Vector2.zero;
            if (document != null) document.Current = Capture();
            lastSimpleEdit = 0;
        }

        private void Restore(Snapshot snapshot, bool focus)
        {
            restoring = true;
            if (!focus) input.DeactivateInputField();
            input.SetTextWithoutNotify(snapshot.Text);
            input.ForceLabelUpdate(); input.textComponent.ForceMeshUpdate();

            input.stringPosition = Mathf.Clamp(snapshot.Focus, 0, snapshot.Text.Length);
            input.selectionStringAnchorPosition = Mathf.Clamp(snapshot.Anchor, 0, snapshot.Text.Length);
            input.ForceLabelUpdate();
            if (input.verticalScrollbar != null) input.verticalScrollbar.value = snapshot.Scroll;
            restoring = false;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (input.isFocused && keyboard != null && keyboard.zKey.wasPressedThisFrame &&
                (keyboard.ctrlKey.isPressed || keyboard.leftMetaKey.isPressed || keyboard.rightMetaKey.isPressed)) Undo();
        }
        private void LateUpdate()
        {
            if (document != null && input.text == document.Current.Text) {
                var snapshot = Capture();
                if (snapshot.Anchor != document.Current.Anchor || snapshot.Focus != document.Current.Focus) lastSimpleEdit = 0;
                document.Current = snapshot;
            }
        }
        private void OnDestroy() { if (input != null) input.onValueChanged.RemoveListener(Changed); }
    }
}
