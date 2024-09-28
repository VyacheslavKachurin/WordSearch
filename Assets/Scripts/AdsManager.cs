using System;
using Unity.VisualScripting;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    private bool _isPaused;
    [SerializeField] private string _appKey;

    private void Start()
    {
        Init();
    }

    void OnApplicationPause(bool isPaused)
    {
        _isPaused = isPaused;
    }
    public void ShowBanner(){
        IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, IronSourceBannerPosition.BOTTOM);
    
    }

    public void Init()
    {
        IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitializationCompletedEvent;
        IronSource.Agent.init(_appKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
        IronSource.Agent.validateIntegration();
    }

    private void SdkInitializationCompletedEvent()
    {
        Debug.Log($"SdkInitializationCompletedEvent");
    }
}