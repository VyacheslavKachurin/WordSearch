using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelView : MonoBehaviour
{
    private VisualElement _root;
    private Label _levelTheme;
    private VisualElement _wordsHolder;
    private VisualElement _targetWord;
    private Label _targetLetters;
    private Label _levelLbl;
    private const string WORD_STYLE = "word";
    private const string WORD_DIV_STYLE = "word-div";

    private const string WORD_BIG = "word-big";
    const string WORD_GRAY = "word-gray";
    [SerializeField] private int _removeWordStyleDelay = 300;
    private Dictionary<string, VisualElement> _words = new();
    private Dictionary<Ability, VisualElement> _abilityBtns;

    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _levelTheme = _root.Q<Label>("level-theme");
        _wordsHolder = _root.Q<VisualElement>("words-holder");

        _targetWord = _root.Q<VisualElement>("target-word");
        _targetLetters = _targetWord.Q<Label>("target-letters");

        _levelLbl = _root.Q<Label>("level-lbl");


        InitButtons();

        AbilityLogic.OnFakeLettersRemoved += HandleFakeLettersRemoved;
        LevelStateService.OnActiveFirstLetterRemoved += HandleFirstLettersRemoved;

    }

    private void HandleFirstLettersRemoved(int lettersLeft)
    {
        if (lettersLeft < 3 && _abilityBtns[Ability.Lighting].enabledInHierarchy)
            _abilityBtns[Ability.Lighting].SetEnabled(false);
        if (lettersLeft == 0 && _abilityBtns[Ability.Hint].enabledInHierarchy)
            _abilityBtns[Ability.Hint].SetEnabled(false);


    }

    private void HandleFakeLettersRemoved()
    {
        _abilityBtns[Ability.Magnet].SetEnabled(false);
    }

    public void SetLevelData(LevelData data)
    {
        _levelTheme.text = data.Theme;
        FillWords(data.Words);
        _levelLbl.text = $"Level: {data.Level}";

        // country
        // level
        // words


    }

    private void InitButtons()
    {
        _abilityBtns = new()
        {
            {Ability.Lighting, _root.Q<AbilityBtn>("lighting-btn")},
            {Ability.Hint, _root.Q<AbilityBtn>("hint-btn")},
            {Ability.Magnet, _root.Q<AbilityBtn>("magnet-btn")},
            {Ability.Firework, _root.Q<AbilityBtn>("firework-btn")},
            {Ability.Ads, _root.Q<AbilityBtn>("ads-btn")}
        };
    }


    private void FillWords(List<string> words)
    {
        foreach (var word in words)
        {
            var div = new VisualElement();
            div.AddToClassList(WORD_DIV_STYLE);
            var label = new Label(word);
            label.AddToClassList(WORD_STYLE);
            div.Add(label);
            _wordsHolder.Add(div);
            _words[word] = label;
        }
    }

    public void AddLetter(char letter)
    {
        _targetLetters.text += letter;
    }

    public void RemoveLetter(char letter)
    {
        _targetLetters.text = _targetLetters.text.Remove(_targetLetters.text.Length - 1);
    }

    public void ToggleWord(bool show, Color color = default)
    {
        _targetWord.Toggle(show);
        if (!show) _targetLetters.text = string.Empty;
        _targetWord.style.backgroundColor = color;
    }

    public async void HideWord(string word)
    {
        var label = _words[word];
        label.AddToClassList(WORD_BIG);
        label.AddToClassList(WORD_GRAY);

        await Task.Delay(_removeWordStyleDelay);
        label.RemoveFromClassList(WORD_BIG);
    }

    internal void SetState(LevelState levelState)
    {
        if (levelState.FoundWords.Count > 0)
            foreach (var word in levelState.FoundWords)
                HideWord(word);

        HandleFirstLettersRemoved(levelState.ActiveFirstLetters.Count);
    }
}

public static class Extensions
{
    public static void Toggle(this VisualElement element, bool value)
    {
        element.style.display = value ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public static Vector2 Pos(this Transform trans)
    {
        return trans.position;
    }
}
