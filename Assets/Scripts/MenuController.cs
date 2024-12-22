using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    }

    private void HandlePlayClick(bool IsClassicGame)
    {
        Session.IsClassicGame = IsClassicGame;
        SceneManager.LoadScene(1);
    }

    private void Start()
    {
        IShopItems shopItems = _menuView.ShopItems;
        _iapManager.FillUpShopItems(shopItems);

        IAPManager.OnPurchasedCoins += HandleAward;

        Session.AdsRemoved += _menuView.HideAdsBtn;
        if (Session.NoAds) _menuView.HideAdsBtn();

        SetGameData();
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