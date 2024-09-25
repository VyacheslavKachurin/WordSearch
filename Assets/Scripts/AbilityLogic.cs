using System;
using UnityEngine;

public class AbilityLogic : MonoBehaviour
{
    [SerializeField] LineProvider _lineProvider;
    private LevelData _data;
    private GameBoard _gameBoard;

    public static event Action OnFakeLettersRemoved;

    private void Start()
    {
        AbilityBtn.OnAbilityClicked += HandleAbility;
    }

    private void HandleAbility(Ability ability, int price, AbilityBtn btn)
    {
        if (!Balance.UseAbility(price))
        {
            Debug.Log($"Not enough money: {price}");
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
            case Ability.Ads:
                ShowAd();
                break;
        }

       
    }

    public void SetData(LevelData levelData, GameBoard gameBoard)
    {
        _data = levelData;
        _gameBoard = gameBoard;

    }

    private void ShowAd()
    {
        Debug.Log($"Showing Ad");
    }

    private void RevealSquare()
    {
        Debug.Log($"Revealing Square");
    }

    private void RevealLetter(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var point = LevelStateService.GetFirstPoint();
            var color = _gameBoard.Letters[point.Y, point.X].GetColor();
            var position = _gameBoard.Letters[point.Y, point.X].transform.position;
            _lineProvider.CreateLine(position, color);
        }
    }

    public void HideFakeLetters()
    {
        foreach (var point in _data.FakeLetters)
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