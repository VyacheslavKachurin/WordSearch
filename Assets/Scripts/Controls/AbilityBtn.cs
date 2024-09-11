
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class AbilityBtn : Button
{
    public static event Action<Ability, int, AbilityBtn> OnAbilityClicked;
    private Label _priceLbl;
    private VisualElement _priceDiv;
    private VisualElement _abilityIcon;
    private Dictionary<Ability, string> _abilityClasses = new()
    {
        {Ability.Lighting, "lighting-icon"},
        {Ability.Hint, "hint-icon"},
        {Ability.Magnet, "magnet-icon"},
        {Ability.Firework, "firework-icon"},
        {Ability.Ads, "ads-icon"}
    };

    private int _priceInt;

    [UxmlAttribute]
    public string Price
    {
        get => _priceLbl.text;
        set
        {
            //convert if price bigger than 1000
            _priceLbl.text = value;
        }
    }
    [UxmlAttribute]
    public AbilityPriceData PriceData;

    [UxmlAttribute]
    public Ability AbilityValue
    {
        get => _ability;
        set
        {
            _abilityIcon.ClearClassList();
            _abilityIcon.AddToClassList(_abilityClasses[value]);
            _ability = value;
            var price = PriceData?.GetPrice(value);
            Price = price?.ToString();
            _priceInt = price ?? 0;
        }
    }

    private Ability _ability;

    [UxmlAttribute]
    public bool IsFree
    {
        get => _priceDiv.style.display == DisplayStyle.None;
        set => _priceDiv.Toggle(!value);

    }


    public AbilityBtn()
    {
        var template = Resources.Load<VisualTreeAsset>("AbilityBtnTemplate");
        Add(template.Instantiate());
        _priceLbl = this.Q<Label>("price-lbl");
        _priceDiv = this.Q<VisualElement>("price-div");
        _abilityIcon = this.Q<VisualElement>("ability-icon");

        RegisterCallback<ClickEvent>(OnClicked);
    }

    private void OnClicked(ClickEvent evt)
    {
        OnAbilityClicked?.Invoke(_ability, _priceInt, this);
    }
}
public enum Ability
{
    Lighting,
    Hint,
    Magnet,
    Firework,
    Ads
}
