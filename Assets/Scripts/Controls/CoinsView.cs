using System;
using System.Collections;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class CoinsView : VisualElement
{
    private const string COINS_UP = "coins-up";
    private VisualElement _coinsAnimDiv;
    private Label _coinsAnimLbl;
    public static event Action<Vector2, int> CoinsShown;

    public CoinsView()
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

    private void SetCoinsDivPosition(Vector2 startPos)
    {
        var viewWidth = this.worldBound.width;
        var viewHeight = startPos.y; //this.worldBound.height;

        var toRemoveX = _coinsAnimDiv.worldBound.width / 2;
        var toAddY = _coinsAnimDiv.worldBound.height / 2;
        _coinsAnimDiv.style.left = viewWidth / 2 - toRemoveX;
        _coinsAnimDiv.style.top = viewHeight + toAddY;
    }

    public async void ShowCoinsLbl(Vector2 startPos, int amount, Action callback = null)
    {
        this.Toggle(true);
        //   _coinsAnimDiv.style.opacity = 0;
        await Task.Delay(100);
        SetCoinsDivPosition(startPos);
        _coinsAnimLbl.text = $"+{amount}";

        _coinsAnimDiv.RegisterCallbackOnce<TransitionEndEvent>(e =>
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(_coinsAnimDiv.worldTransform.GetPosition() + new Vector3(_coinsAnimDiv.worldBound.width / 2, _coinsAnimDiv.worldBound.height / 2));

            CoinsShown?.Invoke(worldPos, amount);

            callback?.Invoke();
        });

        _coinsAnimDiv.AddToClassList(COINS_UP);
        //   _coinsAnimDiv.style.opacity = 1;
    }


    public async void HideAsync()
    {
        _coinsAnimDiv.RegisterCallbackOnce<TransitionEndEvent>(async e =>
        {
            await Task.Delay(600);
            this.Toggle(false);
        });
        await Task.Delay(1000);
        _coinsAnimDiv.RemoveFromClassList(COINS_UP);
    }

}