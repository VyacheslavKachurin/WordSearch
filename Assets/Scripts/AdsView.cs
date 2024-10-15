using System;
using UnityEngine;
using UnityEngine.UIElements;

public class AdsView : MonoBehaviour
{
    private VisualElement _root;
    private Button _loadBannerBtn;
    [SerializeField] private AdsController _adsController;
    private Button _loadInterstitialBtn;
    private Button _showInterstitialBtn;
    private Button _loadRewardedBtn;
    private Button _showRewardedBtn;

    private void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;


        _loadBannerBtn = _root.Q<Button>("LoadBannerBtn");
        _loadBannerBtn.clicked += LoadBannerBtn_OnClicked;

        _loadInterstitialBtn = _root.Q<Button>("LoadInterstitialBtn");
        _loadInterstitialBtn.clicked += LoadInterstitialBtn_OnClicked;

        _showInterstitialBtn = _root.Q<Button>("ShowInterstitialBtn");
        _showInterstitialBtn.clicked += ShowInterstitialBtn_OnClicked;

        _loadRewardedBtn = _root.Q<Button>("LoadRewardedBtn");
        _loadRewardedBtn.clicked += LoadRewardedBtn_OnClicked;

        _showRewardedBtn = _root.Q<Button>("ShowRewardedBtn");
        _showRewardedBtn.clicked += ShowRewardedBtn_OnClicked;



    }

    private void ShowRewardedBtn_OnClicked()
    {
        _adsController.ShowRewardedAd();
    }

    private void LoadRewardedBtn_OnClicked()
    {
        _adsController.LoadRewardedAd();
    }

    private void ShowInterstitialBtn_OnClicked()
    {
        _adsController.ShowInterstitialAd();
    }

    private void LoadInterstitialBtn_OnClicked()
    {
        _adsController.LoadInterstitialAd();
    }

    private void LoadBannerBtn_OnClicked()
    {
        _adsController.LoadBanner();
    }
}