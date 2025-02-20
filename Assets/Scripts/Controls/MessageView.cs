using System;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class MessageView : VisualElement
{

    private VisualElement _messageDiv;
    private Label _messageLbl;

    public MessageView()
    {
        var template = Resources.Load<VisualTreeAsset>("message-view");
        var instance = template.Instantiate();
        this.style.position = Position.Absolute;
        Add(instance);

        instance.style.position = Position.Absolute;
        instance.style.flexGrow = 1;
        instance.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        instance.style.height = new StyleLength(new Length(100, LengthUnit.Percent));

        _messageDiv = this.Q<VisualElement>("message-div");
        _messageLbl = this.Q<Label>("message-lbl");

        EventManager.PurchaseFailed += HideMessage;
        EventManager.AdsRemoved += HideMessage;

        this.RegisterCallback<DetachFromPanelEvent>(Unsubscribe);
    }

    private void Unsubscribe(DetachFromPanelEvent evt)
    {
        EventManager.PurchaseFailed -= HideMessage;
        EventManager.AdsRemoved -= HideMessage;
    }


    public void ShowMessage(string message)
    {
        _messageLbl.text = message;
        this.Toggle(true);
    }

    public void HideMessage()
    {
        this.Toggle(false);
        _messageLbl.text = "";
    }

}