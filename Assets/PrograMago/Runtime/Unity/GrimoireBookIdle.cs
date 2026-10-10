using UnityEngine;

namespace PrograMago.UnityIntegration
{
    public sealed class GrimoireBookIdle : MonoBehaviour
    {
        private RectTransform book;
        private Vector2 restingPosition;
        private void Awake() { book = (RectTransform)transform; restingPosition = book.anchoredPosition; }
        private void Update()
        {
            book.anchoredPosition = restingPosition + Vector2.up * Mathf.Round(Mathf.Sin(Time.unscaledTime * 1.8f) * 4);
            book.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.unscaledTime) * 1.5f);
        }
    }
}
