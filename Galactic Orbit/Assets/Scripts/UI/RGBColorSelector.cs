using UnityEngine;
using UnityEngine.UIElements;

public class RGBColorSelector : VisualElement
{
    public Color SelectedColor { get; private set; } = Color.white;

    public Slider rSlider, gSlider, bSlider;
    private VisualElement colorPreview;

    public RGBColorSelector(Color StartColor)
    {
        AddToClassList("rgb-color-selector");
        var sliderContainer = new VisualElement();
        sliderContainer.AddToClassList("slider-container");
        Add(sliderContainer);
        // Build layout in code (you could also load from UXML instead)

        // Container layout
        /*
        style.width = new StyleLength(Length.Percent(100));
        style.flexDirection = FlexDirection.Column;
        style.paddingTop = 4;
        style.paddingBottom = 4;
        style.paddingLeft = 6;
        style.paddingRight = 6;
        style.borderBottomWidth = 1;
        style.borderTopWidth = 1;
        style.borderLeftWidth = 1;
        style.borderRightWidth = 1;
        style.borderBottomColor = Color.gray;
        style.borderTopColor = Color.gray;
        style.borderLeftColor = Color.gray;
        style.borderRightColor = Color.gray;
        style.marginBottom = 6;*/

        // Sliders
        rSlider = MakeSlider("R", Color.red);
        gSlider = MakeSlider("G", Color.green);
        bSlider = MakeSlider("B", Color.blue);

        // Preview box
        /*colorPreview = new VisualElement
        {
            style =
            {
                width = 100,
                height = 100,
                marginTop = 10,
                borderBottomLeftRadius = 6,
                borderBottomRightRadius = 6,
                borderTopLeftRadius = 6,
                borderTopRightRadius = 6,
                backgroundColor = Color.white
            }
        };*/
        colorPreview = new VisualElement();
        colorPreview.AddToClassList("color-preview");
        Add(colorPreview);

        sliderContainer.Add(rSlider);
        sliderContainer.Add(gSlider);
        sliderContainer.Add(bSlider);

        // Set defaults
        rSlider.value = StartColor.r;
        gSlider.value = StartColor.g;
        bSlider.value = StartColor.b;

        // Register callbacks
        rSlider.RegisterValueChangedCallback(evt => UpdateColor());
        gSlider.RegisterValueChangedCallback(evt => UpdateColor());
        bSlider.RegisterValueChangedCallback(evt => UpdateColor());

        // Initialize
        UpdateColor();
    }

    private Slider MakeSlider(string label, Color tint)
    {
        var slider = new Slider(label, 0f, 1f)
        {
            value = 1f,
            style =
            {
                flexGrow = 1,
                unityTextAlign = TextAnchor.MiddleLeft,
            }
        };
        slider.AddToClassList("rgb-slider");
        // Tint the label text (optional)
        var labelElement = slider.Q<Label>();
        if (labelElement != null)
            labelElement.style.color = tint;

        return slider;
    }

    private void UpdateColor()
    {
        SelectedColor = new Color(rSlider.value, gSlider.value, bSlider.value);
        
        colorPreview.style.backgroundColor = new StyleColor(SelectedColor);
    }
}
