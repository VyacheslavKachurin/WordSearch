using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelView : MonoBehaviour
{
    private VisualElement _root;
    private Label _levelTheme;
    private VisualElement _wordsHolder;
    private VisualElement _targetWord;
    private Label _targetLetters;
    private const string WORD_STYLE = "word";
    private const string WORD_DIV_STYLE = "word-div";

    private const string WORD_BIG = "word-big";
    const string WORD_GRAY = "word-gray";
    [SerializeField] private int _removeWordStyleDelay = 300;
    private Dictionary<string, VisualElement> _words = new();


    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _levelTheme = _root.Q<Label>("level-theme");
        _wordsHolder = _root.Q<VisualElement>("words-holder");

        _targetWord = _root.Q<VisualElement>("target-word");
        _targetLetters = _targetWord.Q<Label>("target-letters");

    }

    public void SetLevelData(LevelData data)
    {
        _levelTheme.text = data.Theme;
        FillWords(data.Words);

        // country
        // level
        // words


    }


    private void FillWords(string[] words)
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

    public void ToggleWord(bool show)
    {
        _targetWord.Toggle(show);
        if (!show) _targetLetters.text = string.Empty;
    }

    public async void HideWord(string word)
    {
        var label = _words[word];
        label.AddToClassList(WORD_BIG);
        label.AddToClassList(WORD_GRAY);

        await Task.Delay(_removeWordStyleDelay);
        label.RemoveFromClassList(WORD_BIG);

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
