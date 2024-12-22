using System;
using UnityEngine.UIElements;

public static class ViewUtils
{

    public static void RegisterShopBtn(VisualElement root, Action clickCallback)
    {
        var shopBtn = root.Q<Button>("shop-btn");
        shopBtn.clicked += clickCallback;
        var balanceLbl = root.Q<Label>("balance-lbl");
        balanceLbl.text = Balance.GetBalance().ToString("F0");
    }

    public static void UnregisterShopBtn(VisualElement root, Action clickCallback)
    {
        var shopBtn = root.Q<Button>("shop-btn");
        shopBtn.clicked -= clickCallback;
    }
}