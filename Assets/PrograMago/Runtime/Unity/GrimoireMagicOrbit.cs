using UnityEngine;
using UnityEngine.UI;

namespace PrograMago.UnityIntegration
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class GrimoireMagicOrbit : MaskableGraphic
    {
        private void Update() => SetVerticesDirty();

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var bounds = rectTransform.rect;
            float radius = Mathf.Min(bounds.width, bounds.height) * .43f;
            float rotation = Time.unscaledTime * .45f;
            for (int ring = 0; ring < 2; ring++) {
                float r = radius * (1 - ring * .13f);
                var tint = ring == 0 ? new Color(.65f, .25f, 1, .7f) : new Color(.25f, .85f, 1, .5f);
                for (int i = 0; i < 96; i++) {
                    float a = i * Mathf.PI * 2 / 96 + rotation;
                    float b = (i + 1) * Mathf.PI * 2 / 96 + rotation;
                    Quad(mesh, Point(bounds.center, a, r - 1.5f), Point(bounds.center, a, r + 1.5f),
                        Point(bounds.center, b, r + 1.5f), Point(bounds.center, b, r - 1.5f), tint);
                }
            }
            for (int i = 0; i < 12; i++) {
                float angle = i * Mathf.PI * 2 / 12 + rotation;
                var p = Point(bounds.center, angle, radius * (1.05f + .07f * Mathf.Sin(Time.unscaledTime * 2 + i)));
                float size = i % 3 == 0 ? 5 : 3;
                Quad(mesh, p + Vector2.left * size, p + Vector2.up * size,
                    p + Vector2.right * size, p + Vector2.down * size,
                    new Color(.7f, .5f, 1, .65f + .3f * Mathf.Sin(Time.unscaledTime * 3 + i)));
            }
        }

        private static Vector2 Point(Vector2 center, float angle, float radius) => center +
            new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius * .72f);

        private static void Quad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color tint)
        {
            int index = mesh.currentVertCount;
            mesh.AddVert(a, tint, Vector2.zero); mesh.AddVert(b, tint, Vector2.zero);
            mesh.AddVert(c, tint, Vector2.zero); mesh.AddVert(d, tint, Vector2.zero);
            mesh.AddTriangle(index, index + 1, index + 2); mesh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
