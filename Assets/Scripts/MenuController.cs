using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Backtrace.Unity;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

public class MenuController : MonoBehaviour, IPrizeProvider
{

    [SerializeField] MenuView _menuView;
    [SerializeField] private int _minPrize = 20;
    [SerializeField] private int _maxPrize = 50;
    [SerializeField] private CoinsFX_Handler _coinsFX_Handler;
    [SerializeField] IAPManager _iapManager;
    [SerializeField] BacktraceClient _backtraceClient;

    [SerializeField] int _requiredClicks = 5;
    [SerializeField] float timeoutBetweenClicks = 0.5f;

    private int _currentClickCount = 0;
    private float _lastClickTime = 0;


    private async void Awake()
    {
        ShopBtn.ShopCoinGeometryChanged += SetForceFieldPos;
        _menuView.Init();
        await RequireConnection();
        await _iapManager.Create();
        await LoadResources();

        MenuView.AwardRequested += HandleAwardRequest;
        MenuView.PlayClicked += HandlePlayClick;
        MenuView.OnCollectionsClicked += HandleCollectionsClick;
        MenuView.OnBackClicked += HandleBackClick;
        MenuView.CircleClicked += HandleCircleClick;

        Services.InitUserId();
        Debug.Log($"UserId : {Services.UserId}");

        // Balance.OnBalanceAdded += AnimateAward;

        IAPManager.CoinPurchased += AnimateAward;

        
    }

    private async void Start()
    {
        UserProgress progress = ProgressService.Progress;
        progress ??= await ProgressService.LoadProgress();

        if (progress.AdsRemoved)
            _menuView.HideAdsOffer();
        Balance.InitBalance(progress.Coins);
        Debug.Log($"App persistent data: {Application.persistentDataPath}");


        //wait for iap
        _menuView.SetLevelData(progress.Level, progress.Episode, progress.TotalEpisodes);
        _menuView.SetBackPicture(ProgressService.Progress.Season);
        if (progress.AdsRemoved) _menuView.HideAdsBtn();
        //init IAP instead
        IShopItems shopItems = _menuView.ShopItems;

        var seasons = ProgressService.LoadStampData().Seasons;
        _menuView.PopulateCollectionsView(seasons);

        try
        {
            _iapManager.InjectShopItems(shopItems);
        }
        catch (Exception e)
        {
            _backtraceClient?.Send(e);
        }

        var adsController = AdsController.Instance;
        if (adsController != null) adsController.HideBanner();

    }


    private async Task RequireConnection()
    {
        var isConnected = await Services.CheckConnection();
        if (!isConnected) _menuView.RequireConnection(true);
        while (!isConnected)
        {
            await Task.Delay(1000);
            isConnected = await Services.CheckConnection();
            if (isConnected)
            {
                _menuView.RequireConnection(false);
                break;
            }
        }
        return;
    }

    private async Task LoadResources()
    {
        var haveResources = Services.HaveResources();
        if (haveResources) return;

        await ProgressService.LoadLevels();
       // await ProgressService.LoadPictures();
        Services.SetHaveResources();

    }

    private void OnDestroy()
    {
        ShopBtn.ShopCoinGeometryChanged -= SetForceFieldPos;
        MenuView.AwardRequested -= HandleAwardRequest;
        MenuView.OnCollectionsClicked -= HandleCollectionsClick;
        MenuView.OnBackClicked -= HandleBackClick;
        MenuView.PlayClicked -= HandlePlayClick;
        MenuView.CircleClicked -= HandleCircleClick;

        IAPManager.CoinPurchased -= AnimateAward;
        // Balance.OnBalanceAdded -= AnimateAward;

    }

    private void HandleCircleClick()
    {
        // Check time since the last click
        if (Time.time - _lastClickTime > timeoutBetweenClicks)
        {
            _currentClickCount = 0;
        }

        // Increment the click count
        _currentClickCount++;
        _lastClickTime = Time.time;

        // Check if the required number of clicks has been reached
        if (_currentClickCount >= _requiredClicks)
        {
            if (!Services.HasFullAccess())
                Services.ExtendAccess();
            else Services.LowerAccess();
            AudioManager.Instance.PlaySound(Sound.Click);
            _currentClickCount = 0; // Reset the counter
        }
    }

    private void HandleBackClick()
    {
        var currentView = _menuView.GetCurrentView();
        if (currentView == ViewOpen.Shop)
            _menuView.HideShopView();
        else
            _menuView.HideCollectionsView();
        AudioManager.Instance.PlaySound(Sound.WindClose);

    }

    private void HandleCollectionsClick()
    {
        _menuView.ShowMenuView(false);
        _menuView.ShowCollectionsView();
        _menuView.ShowBackBtn();
        AudioManager.Instance.PlaySound(Sound.WindOpen);

    }

    private async void HandlePlayClick(bool IsClassicGame)
    {

        Session.IsClassicGame = IsClassicGame;
        await Resources.UnloadUnusedAssets();
        SceneManager.LoadScene(1);

    }


    private void AnimateAward(int payout)
    {
        _menuView.HideMessage();
        var fxWorldPos = _menuView.ShowCoinsPopup((int)payout);
        PlayCoinsFx(fxWorldPos);
        HideCoinsPopup();

    }


    private void PlayCoinsFx(Vector2 vector)
    {
        _coinsFX_Handler.PlayCoinsFX(vector);
    }


    [ContextMenu("Handle Award")]
    private void HandleAward(int payout)
    {
        var width = Screen.width;
        Debug.Log($"width: {width}");
        var giftPickPos = new Vector2(500, 1184);
        var worldPos = new Vector2(0.01f, 0);
        HandleAwardRequest(giftPickPos, worldPos);
    }

    [ContextMenu("Hide Coins Popup ")]
    private void HideCoinsPopup()
    {
        _menuView.HideCoinsPopup();
    }

    [ContextMenu("Delete Gift Data")]
    private void DeleteGiftData()
    {
        Session.ClearGift();
    }

    private void HandleAwardRequest(Vector2 giftPickPos, Vector2 worldPos)
    {
        // coins anim , balance adding and so on
        var prize = GetRandomPrize();

        _menuView.ShowCoinsPopup(giftPickPos, prize, async () =>
        {
            _coinsFX_Handler.PlayCoinsFX(worldPos);
            HideCoinsPopup();
            await Task.Delay(1200);
            StartCoroutine(_menuView.HidingGiftView(true));
        });

        Balance.AddBalance(prize);
       // AnimateAward(prize);

        AudioManager.Instance.PlaySound(Sound.Coins);

        Session.WasGiftReceived = true;
    }


    public int GetRandomPrize()
    {
        var randomPrize = UnityEngine.Random.Range(_minPrize, _maxPrize + 1);
        return randomPrize;
    }

    private void SetForceFieldPos(Vector2 worldPos)
    {
        _coinsFX_Handler.SetForceField(worldPos);
    }


}

internal interface IPrizeProvider
{
    public int GetRandomPrize();
}

[Preserve]
public class StampItem
{
    [Preserve]
    public int Season { get; set; }
    [Preserve]
    public string Name { get; set; }
    [Preserve]
    public bool IsUnlocked { get; set; }

    [Preserve]
    public StampItem() { }

    [Preserve]
    public StampItem(int season, string name, bool isUnlocked = false)
    {
        Season = season;
        Name = name;
        IsUnlocked = isUnlocked;
    }
}

[Preserve]
public class StampsData
{
    [Preserve]
    public List<StampItem> Seasons { get; set; }

    [Preserve]
    public StampsData(List<StampItem> seasons)
    {
        Seasons = seasons;
    }

    [Preserve]
    public StampsData() { }
}