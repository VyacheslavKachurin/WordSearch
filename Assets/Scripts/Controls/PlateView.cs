using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class PlateView : VisualElement
{
    public static event Action<Vector2, int, Action> OnAnimateCoinsRequested;
    private VisualElement _settingsPlate;
    private VisualElement _adsPlate;
    private FadePanel _fadePanel;
    private const string PLATE_HIDE = "plate-show-off";
    private const string SWITCH_OFF = "switch-off";
    private VisualElement _currentPlate;
    private List<Button> _closeBtns;
    private Button _adBtn;
    private Button _musicBtn;
    private Button _soundBtn;
    private VisualElement _main;

    public PlateView()
    {
        var template = Resources.Load<VisualTreeAsset>("PlateView");
        var instance = template.Instantiate();
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


        AbilityLogic.OnCashRequested += () => ShowPlate(Plate.Ads);


        _musicBtn = this.Q<Button>("music-btn");
        _musicBtn.clicked += HandleMusicBtn;

        _soundBtn = this.Q<Button>("sound-btn");
        _soundBtn.clicked += HandleSoundBtn;

        SetBtns();

        _main = this.Q<VisualElement>("main");

        AdsController.RewardedAdWatched += HandleReward;

        this.RegisterCallback<DetachFromPanelEvent>((evt) =>
        {
            Unsubscribe();
        });

    }

    private void Unsubscribe()
    {
        AdsController.RewardedAdWatched -= HandleReward;
    }

    private void HandleReward()
    {
        Debug.Log($"Handle reward");
        HideAdsOffer();

        var pos = _adBtn.worldBound.position;
        var reward = Session.RewardAmount;

        OnAnimateCoinsRequested(pos, reward, null);
        Balance.AddBalance(reward, 100);
        AudioManager.Instance.PlaySound(Sound.Coins);
    }

    private void HideAdsOffer()
    {
        HandleCloseBtn();
    }

    private void SetBtns()
    {
        Session.OnMusicChange += (value) =>
        {
            if (value) _musicBtn.RemoveFromClassList(SWITCH_OFF);
            else _musicBtn.AddToClassList(SWITCH_OFF);
        };

        Session.OnSoundChange += (value) =>
        {
            if (value) _soundBtn.RemoveFromClassList(SWITCH_OFF);
            else _soundBtn.AddToClassList(SWITCH_OFF);
        };

        if (!Session.IsMusicOn) _musicBtn.AddToClassList(SWITCH_OFF);
        if (!Session.IsSoundOn) _soundBtn.AddToClassList(SWITCH_OFF);
    }

    private void HandleSoundBtn()
    {
        Session.IsSoundOn = !Session.IsSoundOn;

        var sound = Session.IsMusicOn ? Sound.WindOpen : Sound.WindClose;
        AudioManager.Instance.PlaySound(sound);

    }

    private void HandleMusicBtn()
    {
        Session.IsMusicOn = !Session.IsMusicOn;
        var sound = Session.IsMusicOn ? Sound.WindOpen : Sound.WindClose;
        AudioManager.Instance.PlaySound(sound);

    }

    private void HandleAdBtn()
    {
        Debug.Log($"Ads controller is null : {AdsController.Instance == null}");
        var AdController = AdsController.Instance;
        AdController = AdController != null ? AdController : GameObject.Find("AdsController").GetComponent<AdsController>();
        AdController.ShowRewardedAd();

    }

    private void HandleCloseBtn()
    {
        _fadePanel.Toggle(false);
        _currentPlate.Toggle(false);
        _currentPlate.AddToClassList(PLATE_HIDE);
        AudioManager.Instance.PlaySound(Sound.WindClose);
        Session.IsSelecting = true;
        _main.pickingMode = PickingMode.Ignore;
    }

    public void ShowPlate(Plate plate)
    {
        Debug.Log($"Showing plate: {plate}");
        _main.pickingMode = PickingMode.Position;
        _currentPlate = plate == Plate.Ads ? _adsPlate : _settingsPlate;
        _fadePanel.Toggle(true);
        _currentPlate.Toggle(true);
        _currentPlate.RemoveFromClassList(PLATE_HIDE);
        Session.IsSelecting = false;
        AudioManager.Instance.PlaySound(Sound.WindOpen);
    }

    public void SubscribeToSettingsClick()
    {
        NavigationRow.OnSettingsClicked += () => ShowPlate(Plate.Settings);
    }
}


public enum Plate { Settings, Ads }