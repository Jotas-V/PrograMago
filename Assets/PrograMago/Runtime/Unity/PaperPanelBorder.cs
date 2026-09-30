using UnityEngine;
using UnityEngine.UI;

namespace PrograMago.UnityIntegration
{
    // Small geometry-based trim keeps the paper area usable at every panel size.
    public sealed class PaperPanelBorder : MaskableGraphic
    {
        protected override void Awake() { base.Awake(); raycastTarget = false; }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var r = rectTransform.rect;
            DrawFrame(mesh, r, 3, new Color32(75, 57, 37, 255));
            r.xMin += 3; r.xMax -= 3; r.yMin += 3; r.yMax -= 3;
            DrawFrame(mesh, r, 2, new Color32(181, 151, 101, 255));
            if (r.height > 180 && r.width > 200)
            {
                Quad(mesh, new Rect(r.xMin+5, r.yMin+7, 3, r.height-14), new Color32(199, 176, 130, 255));
                Quad(mesh, new Rect(r.xMin+10, r.yMin+5, r.width-20, 2), new Color32(199, 176, 130, 255));
                Quad(mesh, new Rect(r.xMax-15, r.yMax-9, 8, 3), new Color32(199, 176, 130, 255));
                Quad(mesh, new Rect(r.xMax-10, r.yMax-14, 3, 8), new Color32(199, 176, 130, 255));
            }
        }
        private static void DrawFrame(VertexHelper mesh, Rect r, float width, Color32 color)
        {
            Quad(mesh, new Rect(r.xMin, r.yMin, r.width, width), color);
            Quad(mesh, new Rect(r.xMin, r.yMax-width, r.width, width), color);
            Quad(mesh, new Rect(r.xMin, r.yMin+width, width, r.height-2*width), color);
            Quad(mesh, new Rect(r.xMax-width, r.yMin+width, width, r.height-2*width), color);
        }
        private static void Quad(VertexHelper mesh, Rect r, Color32 color)
        {
            if (r.width <= 0 || r.height <= 0) return;
            int start = mesh.currentVertCount;
            mesh.AddVert(new Vector3(r.xMin,r.yMin),color,Vector2.zero);
            mesh.AddVert(new Vector3(r.xMin,r.yMax),color,Vector2.zero);
            mesh.AddVert(new Vector3(r.xMax,r.yMax),color,Vector2.zero);
            mesh.AddVert(new Vector3(r.xMax,r.yMin),color,Vector2.zero);
            mesh.AddTriangle(start,start+1,start+2); mesh.AddTriangle(start,start+2,start+3);
        }
    }
}