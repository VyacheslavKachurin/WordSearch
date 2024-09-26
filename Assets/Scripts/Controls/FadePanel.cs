using UnityEngine.UIElements;
using UnityEngine;

[UxmlElement]
public partial class FadePanel : VisualElement
{

    private VisualElement _bg;
    private const string ANIM_IN = "anim-in";

    public FadePanel()
    {
        var template = Resources.Load<VisualTreeAsset>("FadePanel");
        var instance= template.Instantiate();
        instance.style.flexGrow = 1;
        instance.pickingMode = PickingMode.Ignore;
        Add(instance);
        _bg = this.Q<VisualElement>("bg");
        this.style.position = Position.Absolute;
        this.style.flexGrow = 1;
    }

    public void Toggle(bool value)
    {
        if (value) _bg.AddToClassList(ANIM_IN);
        else _bg.RemoveFromClassList(ANIM_IN);
    }
}

