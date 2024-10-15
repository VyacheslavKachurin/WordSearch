using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class ShopView : VisualElement
{
    private List<Button> _buyBtns;
    private Button _noAdsBtn;

    public static event Action OnRemoveAdsClicked;
    public static event Action<int> OnShopBtnClicked;


    public ShopView()
    {
        var template = Resources.Load<VisualTreeAsset>("ShopView");
        var instance = template.Instantiate();
        instance.style.flexGrow = 1;
        instance.pickingMode = PickingMode.Ignore;
        Add(instance);
        style.position = Position.Absolute;
        style.flexGrow = 1;
        style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        style.width = new StyleLength(new Length(100, LengthUnit.Percent));

        NavigationRow.OnShopBtnClicked += HandleShopBtn;
        _noAdsBtn = this.Q<Button>("no-ads-btn");
        _noAdsBtn.clicked += OnRemoveAdsClicked;
        _buyBtns = this.Query<Button>("shop-item").ToList();

        for (int i = 0; i < _buyBtns.Count; i++)
        {
            var index = i;
            _buyBtns[i].clicked += () => OnShopBtnClicked?.Invoke(index);
        }



    }

    private void HandleShopBtn()
    {
        this.Toggle(style.display != DisplayStyle.Flex);
    }
}