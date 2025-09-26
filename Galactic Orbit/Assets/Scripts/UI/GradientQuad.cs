using UnityEngine;
using UnityEngine.UIElements;

public class GradientQuad : VisualElement
{
    // Public properties (accessible from code) reference
    public Color TopColor { get; set; } = Color.white;
    public Color BottomColor { get; set; } = Color.black;

    [Tooltip("Enable/disable click to change colors")]
    public bool ChangeOnClick = false;

    // Change colors (may be changed on click)
    public Color SecondTopColor { get; set; } = Color.black;
    public Color SecondBottomColor { get; set; } = Color.white;

    private Color CurrentTopColor = Color.white;
    private Color CurrentBottomColor = Color.black;

    // Vertex data reused each frame
    static readonly Vertex[] k_Vertices = new Vertex[4];
    static readonly ushort[] k_Indices = { 0, 1, 2, 2, 3, 0 };

    public GradientQuad()
    {
        generateVisualContent += OnGenerateVisualContent;
        CurrentTopColor = TopColor;
        CurrentBottomColor = BottomColor;

        RegisterCallback<PointerDownEvent>(evt =>
        {
            if (!ChangeOnClick) return;

            // Change to the alternate colors on press
            CurrentTopColor = SecondTopColor;
            CurrentBottomColor = SecondBottomColor;
            MarkDirtyRepaint();
        });

        RegisterCallback<PointerUpEvent>(evt =>
        {
            if (!ChangeOnClick) return;

            // Revert to the original colors on release
            CurrentTopColor = TopColor;
            CurrentBottomColor = BottomColor;
            MarkDirtyRepaint();
        });
    }

    void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        Rect r = contentRect;
        if (r.width < 0.01f || r.height < 0.01f)
            return;

        // Assign per-corner tint for interpolation
        k_Vertices[0].tint = CurrentBottomColor; // bottom-left
        k_Vertices[1].tint = CurrentTopColor;    // top-left
        k_Vertices[2].tint = CurrentTopColor;    // top-right
        k_Vertices[3].tint = CurrentBottomColor; // bottom-right

        float left = 0, right = r.width, top = 0, bottom = r.height;
        k_Vertices[0].position = new Vector3(left, bottom, Vertex.nearZ);
        k_Vertices[1].position = new Vector3(left, top, Vertex.nearZ);
        k_Vertices[2].position = new Vector3(right, top, Vertex.nearZ);
        k_Vertices[3].position = new Vector3(right, bottom, Vertex.nearZ);

        MeshWriteData mwd = mgc.Allocate(k_Vertices.Length, k_Indices.Length);
        mwd.SetAllVertices(k_Vertices);
        mwd.SetAllIndices(k_Indices);
    }

    // --- UXML support: factory & traits ---
    public new class UxmlFactory : UxmlFactory<GradientQuad, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        readonly UxmlColorAttributeDescription topColor = new UxmlColorAttributeDescription
        {
            name = "top-color",
            defaultValue = Color.white
        };

        readonly UxmlColorAttributeDescription bottomColor = new UxmlColorAttributeDescription
        {
            name = "bottom-color",
            defaultValue = Color.black
        };
        
        readonly UxmlBoolAttributeDescription changeOnClick = new UxmlBoolAttributeDescription
        {
            name = "change-on-click",
            defaultValue = false
        };

        readonly UxmlColorAttributeDescription secondTopColor = new UxmlColorAttributeDescription
        {
            name = "second-top-color",
            defaultValue = Color.black
        };
        readonly UxmlColorAttributeDescription secondBottomColor = new UxmlColorAttributeDescription
        {
            name = "second-bottom-color",
            defaultValue = Color.white
        };

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            var gv = (GradientQuad)ve;

            gv.TopColor = topColor.GetValueFromBag(bag, cc);
            gv.BottomColor = bottomColor.GetValueFromBag(bag, cc);

            // initialize current with starting values
            gv.CurrentTopColor = gv.TopColor;
            gv.CurrentBottomColor = gv.BottomColor;

            gv.SecondTopColor = secondTopColor.GetValueFromBag(bag, cc);
            gv.SecondBottomColor = secondBottomColor.GetValueFromBag(bag, cc);

            gv.ChangeOnClick = changeOnClick.GetValueFromBag(bag, cc);
        }
    }
}
