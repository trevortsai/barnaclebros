using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Tints a UI Graphic's vertices into a two-stop gradient along one axis.
// Multiplies the existing vertex colour, so the host Image's colour still applies.
[RequireComponent(typeof(Graphic))]
public class UIGradient : BaseMeshEffect
{
    public enum Direction { Vertical, Horizontal }

    public Direction direction = Direction.Vertical;

    [Tooltip("Colour at the min edge (bottom for Vertical, left for Horizontal).")]
    public Color startColor = Color.white;

    [Tooltip("Colour at the max edge (top for Vertical, right for Horizontal).")]
    public Color endColor = new Color(1f, 1f, 1f, 0f);

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0) return;

        var verts = new List<UIVertex>();
        vh.GetUIVertexStream(verts);

        float min = float.MaxValue, max = float.MinValue;
        for (int i = 0; i < verts.Count; i++)
        {
            float v = direction == Direction.Vertical ? verts[i].position.y : verts[i].position.x;
            if (v < min) min = v;
            if (v > max) max = v;
        }
        float range = Mathf.Max(0.0001f, max - min);

        for (int i = 0; i < verts.Count; i++)
        {
            var vert = verts[i];
            float v = direction == Direction.Vertical ? vert.position.y : vert.position.x;
            float t = (v - min) / range;
            vert.color = Color.Lerp(startColor, endColor, t) * vert.color;
            verts[i] = vert;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);
    }
}
