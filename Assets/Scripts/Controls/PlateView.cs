using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class PlateView : VisualElement
{
    public static event Action OnAdclicked;

    private VisualElement _settingsPlate;
    private VisualElement _adsPlate;
    private FadePanel _fadePanel;
    private const string PLATE_HIDE = "plate-show-off";
    private VisualElement _currentPlate;
    private List<Button> _closeBtns;
    private Button _adBtn;

    public PlateView()
    {
        var template = Resources.Load<VisualTreeAsset>("PlateView");
        var instance= template.Instantiate();
        instance.style.flexGrow = 1;
        instance.pickingMode = PickingMode.Ignore;
        Add(instance);
       
        _settingsPlate = this.Q<VisualElement>("settings-plate");
        _adsPlate = this.Q<VisualElement>("ads-plate");
        _fadePanel = this.Q<FadePanel>();
        _closeBtns = this.Query<Button>("close-btn").ToList();

        foreach (var btn in _closeBtns)
            btn.clicked += HandleCloseBtn;

        _adBtn = this.Q<Button>("watch-ad-btn");
        _adBtn.clicked += HandleAdBtn;

        NavigationRow.OnSettingsClicked += () => ShowPlate(Plate.Settings);
        AbilityLogic.OnCashRequested += () => ShowPlate(Plate.Ads);
    }


    private void HandleAdBtn()
    {
        OnAdclicked?.Invoke();
    }

    private void HandleCloseBtn()
    {
        _fadePanel.Toggle(false);
        _currentPlate.Toggle(false);
        _currentPlate.AddToClassList(PLATE_HIDE);
    }

    public void ShowPlate(Plate plate)
    {
        _currentPlate = plate == Plate.Ads ? _adsPlate : _settingsPlate;
        _fadePanel.Toggle(true);
        _currentPlate.Toggle(true);
        _currentPlate.RemoveFromClassList(PLATE_HIDE);
    }
}

public enum Plate { Settings, Ads }