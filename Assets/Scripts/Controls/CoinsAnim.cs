using System;
using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class CoinsAnim : VisualElement
{

    private const string COINS_HIDE = "coins-hide";
    private const string COINS_UP = "coins-up";
    private VisualElement _coinsAnimDiv;
    private Label _coinsAnimLbl;

    public static event Action<Vector2, int> CoinsShown;

    public CoinsAnim()
    {
        var template = Resources.Load<VisualTreeAsset>("CoinsAnimTemplate");
        var instance = template.Instantiate();
        instance.style.position = Position.Absolute;
        instance.style.flexGrow = 1;
        instance.pickingMode = PickingMode.Ignore;
        Add(instance);

        _coinsAnimDiv = this.Q<VisualElement>("coins-anim-div");
        _coinsAnimLbl = this.Q<Label>("coins-anim-lbl");

    }

    public void ShowCoinsLbl(Vector2 startPos, int amount, Action callback)
    {
        _coinsAnimDiv.style.left = startPos.x;
        _coinsAnimDiv.style.top = startPos.y;

        _coinsAnimLbl.text = $"+{amount}";


        _coinsAnimDiv.RegisterCallbackOnce<TransitionEndEvent>(e =>
        {
            var worldPos = Camera.main.ScreenToWorldPoint(_coinsAnimDiv.worldTransform.GetPosition());
            CoinsShown?.Invoke(worldPos, amount);
            HideLbl();
            callback?.Invoke();

        });

        _coinsAnimDiv.AddToClassList(COINS_UP);
    }

    private async void HideLbl()
    {
        _coinsAnimDiv.AddToClassList(COINS_HIDE);
        await Task.Delay(1500);
        _coinsAnimDiv.RemoveFromClassList(COINS_UP);
        _coinsAnimDiv.RemoveFromClassList(COINS_HIDE);
    }

}