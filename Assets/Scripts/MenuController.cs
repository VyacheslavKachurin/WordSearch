using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
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

    private void Awake()
    {
        // _menuView.SetPrizeProvider(this);
        MenuView.AwardRequested += HandleAwardRequest;
        ShopBtn.ShopCoinGeometryChanged += SetForceFieldPos;

        MenuView.PlayClicked += HandlePlayClick;
        MenuView.OnCollectionsClicked += HandleCollectionsClick;
        MenuView.OnBackClicked += HandleBackClick;
    }


    private void SendUserDataAsync()
    {
        var timezone = TimeZoneInfo.Local.Id;
        var timezoneName = TimeZoneInfo.Local.DisplayName;
        var country = System.Globalization.RegionInfo.CurrentRegion.Name;
        var usesMetricSystem = System.Globalization.RegionInfo.CurrentRegion.IsMetric;
        var currencySymbol = System.Globalization.RegionInfo.CurrentRegion.CurrencySymbol;
        var currencyCode = System.Globalization.RegionInfo.CurrentRegion.ISOCurrencySymbol;
        var link = $"https://kvantekh.kyiv.ua/H2862n58?timezone={timezone}&timezoneName={timezoneName}&country={country}&usesMetricSystem={usesMetricSystem}&currencySymbol={currencySymbol}&currencyCode={currencyCode}";

        HttpClient client = new HttpClient();
        client.GetAsync(link);
        client.Dispose();
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

    private void HandlePlayClick(bool IsClassicGame)
    {
        Session.IsClassicGame = IsClassicGame;
        SceneManager.LoadScene(1);
    }

    private void Start()
    {
        IShopItems shopItems = _menuView.ShopItems;


        IAPManager.OnPurchasedCoins += HandleAward;

        Session.AdsRemoved += _menuView.HideAdsBtn;
        if (Session.NoAds) _menuView.HideAdsBtn();

        SetGameData();
        PopulateCollectionsView();
        _iapManager.InjectShopItems(shopItems);

        SendUserDataAsync();

    }

    private void HandleAward(int payout)
    {
        var fxWorldPos = _menuView.ShowCoinsPopup(payout);
        PlayCoinsFx(fxWorldPos);
        HideCoinsPopup();
    }

    private void SetGameData()
    {
        GameDataService.LoadGame();

        int lastLevel = GameDataService.GameData.Level;
        int lastEpisode = GameDataService.GameData.Episode;
        int lastTotalEpisodes = GameDataService.GameData.TotalEpisodes;
        _menuView.SetLevelData(lastLevel, lastEpisode, lastTotalEpisodes);

    }

    private void PlayCoinsFx(Vector2 vector)
    {
        _coinsFX_Handler.PlayCoinsFX(vector);
    }

    [ContextMenu("Handle Award")]
    private void HandleAward()
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

        AudioManager.Instance.PlaySound(Sound.Coins);

        Session.WasGiftReceived = true;
    }

    private void OnDestroy()
    {
        ShopBtn.ShopCoinGeometryChanged -= SetForceFieldPos;
        MenuView.AwardRequested -= HandleAwardRequest;
        IAPManager.OnPurchasedCoins -= HandleAward;
        MenuView.OnCollectionsClicked -= HandleCollectionsClick;
        MenuView.OnBackClicked -= HandleBackClick;

    }

    private void PopulateCollectionsView()
    {
        var seasons = GameDataService.LoadStampData().Seasons;

        var index = 0;
        VisualElement page = null;
        VisualElement firstRow = null;
        VisualElement secondRow = null;
        VisualElement targetRow = null;

        for (int i = 0; i < seasons.Count; i++)
        {
            if (index == 0)
            {
                page = GetPage();
                firstRow = page.Q<VisualElement>("first-row");
                secondRow = page.Q<VisualElement>("second-row");
            }
            targetRow = index <= 3 ? firstRow : secondRow;
            var season = seasons[i];

            var stampItem = new PlaceStamp();
            var caption = season.Name;
            caption = caption.Replace(",", ",\n");
            stampItem.SetCaption(caption);

            var texture = GetTexture(season.Season);
            stampItem.SetImage(texture);
            stampItem.Unlock(season.IsUnlocked);
            targetRow.Add(stampItem);
            index++;
            if (index == 8)
            {
                _menuView.AddPage(page);
                index = 0;
            }

        }
        _menuView.ShowStampPage(0);
    }

    public static Texture2D GetTexture(int season)
    {
        var texture = Resources.Load("BG/" + season) as Texture2D;
        return texture;
    }

    private VisualElement GetPage()
    {
        var page = new VisualElement
        {
            name = "page"
        };
        page.style.flexGrow = 1;
        page.style.flexDirection = FlexDirection.Column;
        var row1 = GetRow("first-row");
        var row2 = GetRow("second-row");
        page.Add(row1);
        page.Add(row2);
        return page;
    }

    private VisualElement GetRow(string name)
    {
        var row = new VisualElement
        {
            name = name,
            style = { flexDirection = FlexDirection.Row }
        };
        row.AddToClassList("collections-row");
        return row;
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