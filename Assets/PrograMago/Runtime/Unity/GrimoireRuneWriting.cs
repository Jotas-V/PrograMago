using UnityEngine;
using UnityEngine.UI;

namespace PrograMago.UnityIntegration
{
    // Fictional glyphs drawn as geometry, independent of UI fonts and real alphabets.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class GrimoireRuneWriting : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var rect = rectTransform.rect;
            for (int row = 0; row < 5; row++) for (int column = 0; column < 8; column++) {
                int glyph = (row * 7 + column * 3) % 9;
                var center = new Vector2(rect.xMin + rect.width * (column + .5f) / 8,
                    rect.yMax - rect.height * (row + .5f) / 5);
                float unit = Mathf.Min(rect.width / 8, rect.height / 5) * .28f;
                Stroke(mesh, center, unit, new Vector2(0,-1), new Vector2(0,1));
                Stroke(mesh, center, unit, new Vector2(0, .7f), new Vector2(glyph % 2 == 0 ? -.7f : .7f, .15f));
                Stroke(mesh, center, unit, new Vector2(0,-.25f), new Vector2(glyph % 3 == 0 ? -.7f : .7f,-.8f));
                if (glyph % 3 != 0) {
                    Stroke(mesh, center, unit, new Vector2(-.55f,.3f), new Vector2(0,1.2f));
                    Stroke(mesh, center, unit, new Vector2(0,1.2f), new Vector2(.55f,.3f));
                }
                if (glyph >= 4) Stroke(mesh, center, unit, new Vector2(-.65f,-.9f), new Vector2(.65f,-.9f));
                if (glyph >= 6) Stroke(mesh, center, unit, new Vector2(.7f,-.2f), new Vector2(.7f,.9f));
            }
        }
        private void Stroke(VertexHelper mesh, Vector2 center, float unit, Vector2 from, Vector2 to)
        {
            Vector2 a = center + from * unit, b = center + to * unit;
            Vector2 direction = (b-a).normalized;
            Vector2 normal = new Vector2(-direction.y,direction.x) * Mathf.Max(1,unit * .055f);
            int index = mesh.currentVertCount;
            mesh.AddVert(a-normal,color,Vector2.zero); mesh.AddVert(a+normal,color,Vector2.zero);
            mesh.AddVert(b+normal,color,Vector2.zero); mesh.AddVert(b-normal,color,Vector2.zero);
            mesh.AddTriangle(index,index+1,index+2); mesh.AddTriangle(index,index+2,index+3);
        }
    }
}
