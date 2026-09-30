using PrograMago.Domain;
using TMPro;
using UnityEngine;

namespace PrograMago.UnityIntegration
{
    public sealed partial class GameplayBootstrapper
    {
        [SerializeField] private UnityEngine.UI.Image[] arenaCells = new UnityEngine.UI.Image[CombatEngine.CellCount];
        [SerializeField] private SpriteRenderer[] scenery = new SpriteRenderer[2];
        private readonly Vector3[] arenaCorners = new Vector3[4];
        private Rect arenaWorldRect;
        private const float GroundHeight = 0.30f;

        private void CreateArenaPresentation()
        {
            var frameImage = arenaFrame.GetComponent<UnityEngine.UI.Image>();
            if (frameImage != null) { frameImage.color = Color.clear; frameImage.raycastTarget = false; }
            Sprite background = Resources.Load<Sprite>("Arena/ForestArena");
            for (int i = 0; i < scenery.Length; i++)
            {
                var item = new GameObject("ForestBackdrop" + i, typeof(SpriteRenderer));
                item.transform.SetParent(transform, false);
                scenery[i] = item.GetComponent<SpriteRenderer>();
                scenery[i].sprite = background;
                scenery[i].sortingOrder = -100;
                scenery[i].flipX = i == 1;
            }

            RectTransform cells = CreatePanel("ArenaCells", arenaFrame, Vector2.zero, Vector2.one, Color.clear);
            cells.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            for (int i = 0; i < arenaCells.Length; i++)
            {
                RectTransform cell = CreatePanel("ArenaCell" + (i + 1), cells,
                    new Vector2((float)i / arenaCells.Length, 0.09f),
                    new Vector2((float)(i + 1) / arenaCells.Length, 0.29f), Color.clear);
                cell.offsetMin = new Vector2(1, 1);
                cell.offsetMax = new Vector2(-1, -1);
                arenaCells[i] = cell.GetComponent<UnityEngine.UI.Image>();
                arenaCells[i].raycastTarget = false;
                TMP_Text number = CreateLabel("CellNumber", cell, (i + 1).ToString(), Vector2.zero, Vector2.one);
                number.fontSizeMax = 18;
            }

            RectTransform mageCard = CreatePanel("MagoStatusCard", arenaFrame,
                new Vector2(0.01f, 0.73f), new Vector2(0.36f, 0.98f), new Color32(22, 30, 40, 225));
            RectTransform enemyCard = CreatePanel("EnemyStatusCard", arenaFrame,
                new Vector2(0.40f, 0.73f), new Vector2(0.80f, 0.98f), new Color32(22, 30, 40, 225));
            RectTransform messageCard = CreatePanel("CombatMessageCard", arenaFrame,
                Vector2.zero, new Vector2(1, 0.085f), new Color32(18, 24, 30, 245));
            PlaceArenaLabel(magoStatsText, mageCard);
            PlaceArenaLabel(enemyStatsText, enemyCard);
            PlaceArenaLabel(combatStatusText, messageCard);
            magoStatsText.fontSizeMax = 20;
            enemyStatsText.fontSizeMax = 19;
            combatStatusText.fontSizeMax = 18;
            combatStatusText.alignment = TextAlignmentOptions.Center;
            mageCard.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            enemyCard.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            messageCard.GetComponent<UnityEngine.UI.Image>().raycastTarget = false;
            combatDefeatOverlay.transform.SetAsLastSibling();
            UpdateArenaPresentation();
        }

        private static void PlaceArenaLabel(TMP_Text label, Transform parent)
        {
            label.transform.SetParent(parent, false);
            label.rectTransform.anchorMin = new Vector2(0.02f, 0.03f);
            label.rectTransform.anchorMax = new Vector2(0.98f, 0.97f);
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
            label.color = new Color32(236, 241, 225, 255);
        }

        private void UpdateArenaPresentation()
        {
            if (arenaCamera == null || arenaFrame == null) return;
            arenaFrame.GetWorldCorners(arenaCorners);
            float depth = -arenaCamera.transform.position.z;
            Vector3 bottom = arenaCamera.ScreenToWorldPoint(new Vector3(arenaCorners[0].x, arenaCorners[0].y, depth));
            Vector3 top = arenaCamera.ScreenToWorldPoint(new Vector3(arenaCorners[2].x, arenaCorners[2].y, depth));
            arenaWorldRect = Rect.MinMaxRect(bottom.x, bottom.y, top.x, top.y);
            UpdateArenaScenery();
            UpdateReachIndicators();
            UpdateArenaAtmosphere();
        }

        private void UpdateArenaScenery()
        {
            if (arenaWorldRect.width <= 0f || arenaWorldRect.height <= 0f) return;
            Sprite background = scenery.Length > 0 && scenery[0] != null
                ? scenery[0].sprite : Resources.Load<Sprite>("Arena/ForestArena");
            if (background == null) return;
            Vector2 size = background.bounds.size;
            if (size.x <= 0f || size.y <= 0f) return;
            float scale = arenaWorldRect.height / size.y;
            float tileWidth = size.x * scale;
            int count = Mathf.Max(1, Mathf.CeilToInt(arenaWorldRect.width / tileWidth));
            if (scenery.Length < count) System.Array.Resize(ref scenery, count);
            for (int i = 0; i < scenery.Length; i++)
            {
                if (scenery[i] == null && i < count)
                {
                    var item = new GameObject("ForestBackdrop" + i, typeof(SpriteRenderer));
                    item.transform.SetParent(transform, false);
                    scenery[i] = item.GetComponent<SpriteRenderer>();
                }
                if (scenery[i] == null) continue;
                scenery[i].enabled = i < count;
                if (i >= count) continue;
                scenery[i].sprite = background;
                scenery[i].sortingOrder = -100;
                scenery[i].flipX = i % 2 == 1;
                scenery[i].transform.position = new Vector3(
                    arenaWorldRect.xMin + tileWidth * (i + 0.5f), arenaWorldRect.center.y, 0f);
                // Preserve the source proportions and repeat only as far as the viewport needs.
                Vector3 parentScale = scenery[i].transform.parent.lossyScale;
                scenery[i].transform.localScale = new Vector3(
                    scale / parentScale.x, scale / parentScale.y, 1f / parentScale.z);
            }
        }

        private void UpdateReachIndicators()
        {
            int magePosition = combat == null ? 0 : combat.WizardPosition;
            for (int i = 0; i < arenaCells.Length; i++)
            {
                if (arenaCells[i] == null) continue;
                bool mageCell = i == magePosition;
                bool reached = CurrentMago != null && Mathf.Abs(i - magePosition) <= CurrentMago.Alcance;
                arenaCells[i].color = mageCell ? new Color32(121, 94, 192, 180) : reached
                    ? new Color32(71, 161, 158, 135) : new Color32(23, 34, 34, 135);
            }
        }

        private Vector3 CellGroundPosition(int index) => new Vector3(
            arenaWorldRect.xMin + arenaWorldRect.width * (Mathf.Clamp(index, 0, 15) + 0.5f) / CombatEngine.CellCount,
            arenaWorldRect.yMin + arenaWorldRect.height * GroundHeight, 0);

        private void StandInCell(GameObject actor, int cell)
        {
            if (actor == null || IsActorMoving(actor)) return;
            SpriteRenderer sprite = actor.GetComponentInChildren<SpriteRenderer>();
            Vector3 ground = CellGroundPosition(cell);
            Transform feet = actor.transform.Find("CombatFootAnchor");
            actor.transform.position += new Vector3(ground.x - sprite.bounds.center.x, ground.y - (feet != null ? feet.position.y : sprite.bounds.min.y), 0);
        }

        private int TargetDistance(string variableName)
        {
            if (combat != null)
                foreach (CombatEnemy enemy in combat.Enemies)
                    if (enemy.Source.VariableName == variableName) return Mathf.Abs(enemy.Position - combat.WizardPosition);
            return 0;
        }
    }
}
