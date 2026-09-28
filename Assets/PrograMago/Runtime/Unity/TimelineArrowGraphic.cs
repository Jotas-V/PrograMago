using UnityEngine;

namespace PrograMago.UnityIntegration
{
    public sealed class TimelineArrowGraphic : UnityEngine.UI.MaskableGraphic
    {
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper mesh)
        {
            mesh.Clear();
            Rect r = rectTransform.rect;
            float notch = Mathf.Min(13f, r.width * 0.15f);
            Vector2[] points = {
                new Vector2(r.xMin, r.yMin), new Vector2(r.xMax - notch, r.yMin),
                new Vector2(r.xMax, r.center.y), new Vector2(r.xMax - notch, r.yMax),
                new Vector2(r.xMin, r.yMax), new Vector2(r.xMin + notch, r.center.y) };
            mesh.AddVert(r.center, color, Vector2.zero);
            foreach (Vector2 point in points) mesh.AddVert(point, color, Vector2.zero);
            for (int i = 0; i < points.Length; i++) mesh.AddTriangle(0, i + 1, (i + 1) % points.Length + 1);
        }
    }
}
