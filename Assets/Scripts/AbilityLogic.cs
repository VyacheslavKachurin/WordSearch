using System;
using UnityEngine;

public class AbilityLogic : MonoBehaviour
{
    [SerializeField] LineProvider _lineProvider;
    private LevelData _data;
    private GameBoard _gameBoard;

    IAdsRequest _adsRequest;

    public static event Action OnCashRequested;

    public static event Action OnFakeLettersRemoved;
    public static event Action OnFreezeRequested;
    private AudioManager _audioManager;

    private void Awake()
    {
        _audioManager = AudioManager.Instance;
        AbilityBtn.OnAbilityClicked += HandleAbility;
    }


    private void OnDestroy()
    {
        AbilityBtn.OnAbilityClicked -= HandleAbility;
    }

    private void HandleAbility(Ability ability, int price, AbilityBtn btn)
    {
        if (!Balance.UseAbility(price))
        {
            _adsRequest.RequestAds();
            return;
        }

        switch (ability)
        {
            case Ability.Magnet:
                HideFakeLetters();
                break;
            case Ability.Hint:
                RevealLetter(1);
                break;
            case Ability.Lighting:
                RevealLetter(3);
                break;
            case Ability.Firework:
                RevealSquare();
                break;

            case Ability.Freeze:
                RequestPauseTimer();
                break;

        }
        AppMetricaService.SendAbilityEvent(ability);
        LevelStateService.SaveState();


    }

    private void RequestPauseTimer()
    {
        OnFreezeRequested?.Invoke();
        AudioManager.Instance.PlaySound(Sound.Freeze);
    }

    public void SetData(LevelData levelData, GameBoard gameBoard, IAdsRequest adsRequest)
    {
        _data = levelData;
        _gameBoard = gameBoard;
        _adsRequest = adsRequest;

    }

    private void RequireAd()
    {
        OnCashRequested?.Invoke();
    }

    private void RevealSquare()
    {
        Debug.Log($"Revealing Square");
    }

    private void RevealLetter(int amount)
    {
        var sound = amount == 1 ? Sound.Lamp : Sound.Light;
        _audioManager.PlaySound(sound);
        for (int i = 0; i < amount; i++)
        {
            var point = LevelStateService.GetFirstLetter();
            var letter = _gameBoard.Letters[point.Y, point.X];
            var position = letter.transform.position;
            var color = _lineProvider.CreateLine(position);
            _lineProvider.FinishDraw(true);
            var revealedLetter= new RevealedLetter(point, color);

            LevelStateService.StoreRevealedLetter(revealedLetter);
        }

    }

    public void HideFakeLetters()
    {
        _audioManager.PlaySound(Sound.Magnet);
        foreach (var point in LevelStateService.GetSomeFakeLetters(_data))
        {
            _gameBoard.Letters[point.Y, point.X].Hide();

        }
        LevelStateService.State.FakeLettersRemoved = true;
        OnFakeLettersRemoved?.Invoke();
    }

    internal void SetState(LevelState levelState)
    {
        if (levelState.FakeLettersRemoved)
            HideFakeLetters();
    }
}