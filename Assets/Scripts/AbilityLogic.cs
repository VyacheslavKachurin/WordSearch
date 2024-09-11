using UnityEngine;

public class AbilityLogic : MonoBehaviour
{
    private LevelData _data;
    private GameBoard _gameBoard;

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

        btn.SetEnabled(false);
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
        Debug.Log($"Revealing Letter: {amount}");
    }

    private void HideFakeLetters()
    {
        Debug.Log($"Hiding Fake Letters");
        foreach (var point in _data.FakeLetters)
        {
            _gameBoard.Letters[point.Y, point.X].Hide();
        }
        LevelStateService.State.FakeLettersRemoved = true;
    }

}