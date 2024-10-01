using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class MenuView : MonoBehaviour
{
    [SerializeField] private float _hideBoxDelay = 0.005f;
    private VisualElement _root;
    private FadePanel _fadePnl;
    private VisualElement _giftDiv;

    private List<Button> _giftBtns;

    private const string GIFT_ANIM_IN = "gift-anim-in";
    private const string GIFT_ANIM_OUT = "gift-anim-out";

    private const string BOX_HIDE = "box-hide";
    private const string BOX_WIN = "box-win";

    private NavigationRow _navRow;

    [SerializeField] private int _minPrize = 20;
    [SerializeField] private int _maxPrize = 50;

    private void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _fadePnl = _root.Q<FadePanel>();
        _giftDiv = _root.Q<VisualElement>("gift-div");
        _navRow = _root.Q<NavigationRow>();

        SetGiftBtns();
        _navRow.SetCoinsAnim(_root.Q<CoinsAnim>());
    }

    private void SetGiftBtns()
    {
        _giftBtns = _giftDiv.Query<Button>().ToList();

        foreach (var btn in _giftBtns)
        {
            btn.RegisterCallback<ClickEvent>(HandleGiftBtn);
            btn.AddToClassList(BOX_HIDE);
        }

    }

    private void HandleGiftBtn(ClickEvent evt)
    {
        var btn = evt.currentTarget as Button;

        var otherBtns = _giftBtns.FindAll(x => x != btn);
        foreach (var b in otherBtns)
        {
            b.AddToClassList(BOX_HIDE);
        }
        btn.RegisterCallbackOnce<TransitionEndEvent>(UnpackGift);
        btn.AddToClassList(BOX_WIN);
    }

    private void UnpackGift(TransitionEndEvent evt)
    {
        var randomPrize = UnityEngine.Random.Range(_minPrize, _maxPrize + 1);
        var element = evt.target as VisualElement;
        var pos = element.worldBound.position;//+ new Vector2(element.worldBound.width / 2, element.worldBound.height / 2);

        _navRow.AnimateAddCoins(pos, randomPrize, () => HideGift(element));
    }


    [ContextMenu("Show gift panel")]
    public void ShowGift()
    {
        _fadePnl.Toggle(true);
        _giftDiv.Toggle(true);
        _giftDiv.AddToClassList(GIFT_ANIM_IN);

        foreach (var btn in _giftBtns)
            btn.RemoveFromClassList(BOX_HIDE);
    }

    public void HideGift(VisualElement element)
    {

        _giftDiv.RegisterCallbackOnce<TransitionEndEvent>(e =>
        {
            _giftDiv.Toggle(false);
            //_giftDiv.RemoveFromClassList(GIFT_ANIM_IN);
            //_giftDiv.RemoveFromClassList(GIFT_ANIM_OUT);
            _fadePnl.Toggle(false);
            element.RemoveFromClassList(BOX_WIN);
           
        });

        _giftDiv.AddToClassList(GIFT_ANIM_OUT);



        foreach (var btn in _giftBtns)
        {
            btn.style.opacity = 1;
            btn.style.scale = new StyleScale(Vector3.one);
            btn.Toggle(true);
        }
    }
}