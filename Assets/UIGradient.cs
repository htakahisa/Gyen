using UnityEngine;
using UnityEngine.UI;

public class UIGradient : BaseMeshEffect
{
    [SerializeField] private Color _topColor = Color.white;
    [SerializeField] private Color _bottomColor = Color.black;

    public Color topColor
    {
        get => _topColor;
        set { _topColor = value; graphic.SetVerticesDirty(); }
    }

    public Color bottomColor
    {
        get => _bottomColor;
        set { _bottomColor = value; graphic.SetVerticesDirty(); }
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0)
            return;

        UIVertex vertex = new UIVertex();

        float topY = float.MinValue;
        float bottomY = float.MaxValue;
        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            if (vertex.position.y > topY) topY = vertex.position.y;
            if (vertex.position.y < bottomY) bottomY = vertex.position.y;
        }

        float height = topY - bottomY;

        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            float t = (vertex.position.y - bottomY) / height;
            vertex.color = Color.Lerp(_bottomColor, _topColor, t);
            vh.SetUIVertex(vertex, i);
        }
    }
}
