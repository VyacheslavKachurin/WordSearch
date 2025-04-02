using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class PlateView : VisualElement
{
    public static event Action<Vector2, int, Action> OnAnimateCoinsRequested;

    private VisualElement _settingsDiv;
    private VisualElement _adsDiv;
    private VisualElement _timeoutDiv;
    private const string SWITCH_OFF = "switch-off";
    private VisualElement _currentDiv;

    private Button _closeBtn;
    private Button _adBtn;
    private Button _replayBtn;

    private Button _musicBtn;
    private Button _soundBtn;

    private VisualElement _plate;
    private VisualElement _blurPnl;

    public VisualElement BlurPnl
    {
        get => _blurPnl;
        set
        {
            _blurPnl = value;
        }

    }

    public static event Action OnCloseClicked;
    public static event Action OnReplayRequested;
    const string PLATE_SHOW = "plate-show";


    public PlateView()
    {
        var template = Resources.Load<VisualTreeAsset>("PlateView");
        var instance = template.Instantiate();
        instance.style.flexGrow = 1;
        instance.style.position = Position.Absolute;
        instance.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
        instance.style.height = new StyleLength(new Length(100, LengthUnit.Percent));
        instance.style.justifyContent = Justify.Center;
        instance.style.alignItems = Align.Center;

        instance.pickingMode = PickingMode.Ignore;
        Add(instance);

        _settingsDiv = this.Q<VisualElement>("settings-div");
        _adsDiv = this.Q<VisualElement>("ads-div");
        _timeoutDiv = this.Q<VisualElement>("timeout-div");

        _closeBtn = this.Q<Button>("close-btn");
        _closeBtn.clicked += ClosePlate;

        _adBtn = this.Q<Button>("watch-ad-btn");
        _adBtn.clicked += HandleAdBtn;

        _plate = this.Q<VisualElement>("plate");
        _plate.RemoveFromClassList(PLATE_SHOW);

        _musicBtn = this.Q<Button>("music-btn");
        _musicBtn.clicked += HandleMusicBtn;

        _soundBtn = this.Q<Button>("sound-btn");
        _soundBtn.clicked += HandleSoundBtn;

        _replayBtn = this.Q<Button>("replay-btn");
        _replayBtn.clicked += HandleReplayBtn;

        SetBtns();

        AdsEventManager.RewardedAdWatched += HandleReward;

        this.RegisterCallback<DetachFromPanelEvent>((evt) =>
        {
            Unsubscribe();
        });

    }

    private void HandleReplayBtn()
    {
        OnReplayRequested?.Invoke();
        ClosePlate();
    }

    private void Unsubscribe()
    {
        AdsEventManager.RewardedAdWatched -= HandleReward;
        _adBtn.clicked -= HandleAdBtn;
    }

    private void HandleReward(IAdsProvider provider)
    {
        Debug.Log($"Handle reward");
        HideAdsOffer();
    }


    private void HideAdsOffer()
    {
        ClosePlate();
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
        AdsController.Instance.ShowRewardedAd();
    }

    private void ClosePlate()
    {
        _plate.RegisterCallbackOnce<TransitionEndEvent>(e =>
        {
            _currentDiv.Toggle(false);
            _currentDiv = null;
            this.Toggle(false);
            BlurPnl.Toggle(false);
        });
        Session.IsSelecting = true;
        AudioManager.Instance.PlaySound(Sound.WindClose);
        _plate.RemoveFromClassList(PLATE_SHOW);
        _closeBtn.Toggle(true);
        OnCloseClicked?.Invoke();
    }

    public void ShowPlate(Plate plate)
    {

        this.Toggle(true);
        //  await Task.Delay(100);
        BlurPnl.Toggle(true);
        BlurPnl.PlaceBehind(this);

        this.pickingMode = PickingMode.Position;

        _currentDiv = GetDiv(plate);

        if (plate == Plate.Timeout)
            _closeBtn.Toggle(false);

        _currentDiv.Toggle(true);
        _plate.AddToClassList(PLATE_SHOW);
        AudioManager.Instance.PlaySound(Sound.WindOpen);
    }

    private VisualElement GetDiv(Plate plate)
    {
        switch (plate)
        {
            case Plate.Ads:
                return _adsDiv;
            case Plate.Settings:
                return _settingsDiv;
            case Plate.Timeout:
                return _timeoutDiv;
            default:
                return _adsDiv;
        }
    }

    internal void SetBlur(VisualElement blurPnl)
    {
        BlurPnl = blurPnl;
    }
}


public enum Plate
{
    Settings, Ads,
    Timeout
}