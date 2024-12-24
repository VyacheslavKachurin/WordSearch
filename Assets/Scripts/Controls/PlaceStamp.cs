using UnityEngine.UIElements;
using UnityEngine;

public partial class PlaceStamp : VisualElement
{
    private Label _captionLbl;
    private VisualElement _placePic;
    private VisualElement _lockedDiv;

    public PlaceStamp()
    {
        var template = Resources.Load<VisualTreeAsset>("PlaceStampTemplate");
        var instance = template.Instantiate();
        instance.style.flexGrow = 0;
        Add(template.Instantiate());

        _captionLbl = this.Q<Label>("caption-lbl");
        _placePic = this.Q<VisualElement>("place-pic");
        _lockedDiv = this.Q<VisualElement>("locked-div");
    }

    public void SetCaption(string caption) => _captionLbl.text = caption;
    public void Unlock(bool locked) => _lockedDiv.Toggle(!locked);
    public void SetImage(Texture2D texture) => _placePic.style.backgroundImage = texture;

}