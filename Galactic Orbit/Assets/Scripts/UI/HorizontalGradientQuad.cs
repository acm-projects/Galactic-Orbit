using UnityEngine;
using UnityEngine.UIElements;

public class HorizontalGradientQuad : VisualElement
{
    public new class UxmlFactory : UxmlFactory<HorizontalGradientQuad, UxmlTraits> { }

    public Color leftColor = Color.white;
    public Color rightColor = Color.black;

    static readonly Vertex[] k_Vertices = new Vertex[4];
    static readonly ushort[] k_Indices = { 0, 1, 2, 2, 3, 0 };

    public HorizontalGradientQuad()
    {
        generateVisualContent += OnGenerateVisualContent;
    }

    void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        Rect r = contentRect;
        if (r.width < 0.01f || r.height < 0.01f)
            return;

        // Assign colors for left → right
        k_Vertices[0].tint = leftColor;  // bottom-left
        k_Vertices[1].tint = leftColor;  // top-left
        k_Vertices[2].tint = rightColor; // top-right
        k_Vertices[3].tint = rightColor; // bottom-right

        float left = 0;
        float right = r.width;
        float top = 0;
        float bottom = r.height;

        k_Vertices[0].position = new Vector3(left, bottom, Vertex.nearZ);
        k_Vertices[1].position = new Vector3(left, top, Vertex.nearZ);
        k_Vertices[2].position = new Vector3(right, top, Vertex.nearZ);
        k_Vertices[3].position = new Vector3(right, bottom, Vertex.nearZ);

        MeshWriteData mwd = mgc.Allocate(k_Vertices.Length, k_Indices.Length);
        mwd.SetAllVertices(k_Vertices);
        mwd.SetAllIndices(k_Indices);
    }
}
