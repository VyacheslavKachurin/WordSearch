using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;
using System.Collections;
using UnityEngine.SceneManagement;

public class LevelView : MonoBehaviour
{
    public static event Action NextLevelClicked;

    private VisualElement _root;
    private Label _levelTheme;
    private VisualElement _wordsHolder;
    private VisualElement _targetWord;
    private Label _targetLetters;
    private Label _levelLbl;
    private VisualElement _visualsDiv;
    private VisualElement _fxDiv;
    private const string WORD_STYLE = "word";
    private const string WORD_DIV_STYLE = "word-div";
    [SerializeField] float _letterAnimStyle = 70;

    private const string WORD_BIG = "word-big";
    const string WORD_GRAY = "word-gray";
    [SerializeField] private int _removeWordStyleDelay = 300;
    private Dictionary<string, Label> _words = new();
    private Dictionary<Ability, VisualElement> _abilityBtns;

    [SerializeField] float _letterAnimDuration = 0.5f;

    [SerializeField] private ParticleSystem _wordFoundFX;
    [SerializeField] private RenderTexture _renderTexture;
    private float _rootHeight;
    private NavigationRow _navRow;
    private VisualElement _finishView;
    private Button _nextLvlBtn;
    private VisualElement _progressFill;
    private Label _progressLbl;
    private VisualElement _progressBg;
    [SerializeField] private float _fillDelay = 0.01f;
    private Coroutine _progressBarCoroutine;
    private const string NEXT_LVL_BTN_ON = "next-lvl-btn-on";

    private void Awake()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _levelTheme = _root.Q<Label>("level-theme");
        _wordsHolder = _root.Q<VisualElement>("words-holder");

        _targetWord = _root.Q<VisualElement>("target-word");
        _targetLetters = _targetWord.Q<Label>("target-letters");

        _levelLbl = _root.Q<Label>("level-lbl");
        _visualsDiv = _root.Q<VisualElement>("visuals-div");
        _fxDiv = _root.Q<VisualElement>("fx-div");

        InitRenderTexture();


        InitButtons();

        AbilityLogic.OnFakeLettersRemoved += DisableMagnet;
        LevelStateService.OnActiveFirstLetterRemoved += HandleFirstLettersRemoved;

        _root.RegisterCallbackOnce<GeometryChangedEvent>((e) =>
        {
            _rootHeight = _root.layout.height;
        });

        _finishView = _root.Q<VisualElement>("finish-view");
        _nextLvlBtn = _finishView.Q<Button>("next-lvl-btn");
        _nextLvlBtn.clicked += HandleNextLvlBtn;

        _progressFill = _root.Q<VisualElement>("progress-fill");
        _progressLbl = _root.Q<Label>("progress-lbl");
        _progressBg = _root.Q<VisualElement>("progress-bg");


        _navRow = _root.Q<NavigationRow>();
        _navRow.InitBalance(AudioManager.Instance);

    }

    public void ShowFinishView(int levelPassed)
    {
        _progressLbl.text = $"{levelPassed}/5";
        _finishView.Toggle(true);
        if (_progressBarCoroutine != null) return;
        _progressBarCoroutine = StartCoroutine(FillProgressBar(levelPassed));
    }

    [ContextMenu("Test level fill")]
    public void TestFill()
    {

        ShowFinishView(1);
    }



    private IEnumerator FillProgressBar(int levelPassed)
    {
        var targetFill = 100 * (levelPassed / 5f);
        var currentFill = 0f;
        Debug.Log($"targetFill: {targetFill}");
        while (currentFill < targetFill)
        {
            currentFill += 1f;
            _progressFill.style.width = new StyleLength(new Length(currentFill, LengthUnit.Percent));
            yield return new WaitForSeconds(_fillDelay);
        }
        ShowNextLvlBtn();
    }

    private void ShowNextLvlBtn()
    {
        _nextLvlBtn.AddToClassList(NEXT_LVL_BTN_ON);

    }

    private void HandleNextLvlBtn()
    {
        var nextLvl = Session.GetLastLevel() + 1;
        Session.SetLastLevel(nextLvl);
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        NextLevelClicked?.Invoke();
        _finishView.Toggle(false);
        _progressBarCoroutine = null;
    }

    private void InitRenderTexture()
    {
        _fxDiv.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(_renderTexture));

    }

    private void HandleFirstLettersRemoved(int lettersLeft)
    {
        if (lettersLeft < 3 && _abilityBtns[Ability.Lighting].enabledInHierarchy)
            _abilityBtns[Ability.Lighting].SetEnabled(false);
        if (lettersLeft == 0 && _abilityBtns[Ability.Hint].enabledInHierarchy)
            _abilityBtns[Ability.Hint].SetEnabled(false);


    }

    private void DisableMagnet()
    {
        _abilityBtns[Ability.Magnet].SetEnabled(false);
    }

    public void SetLevelData(LevelData data)
    {
        _levelTheme.text = data.Subject;
        FillWords(data.Words);
        _levelLbl.text = $"Level: {data.Level}";

        // country
        // level
        // words
    }

    public void AnimateWord(List<LetterUnit> letterUnits)
    {
        var key = new string(letterUnits.Select(x => x.Letter).ToArray());
        var word = _words[key];

        for (int i = 0; i < letterUnits.Count; i++)
        {
            var letter = letterUnits[i];
            var viewPos = Camera.main.WorldToScreenPoint(letter.transform.position);
            var letterLbl = new Label(letter.Letter.ToString());

            letterLbl.style.fontSize = new StyleLength(_letterAnimStyle);
            _visualsDiv.Add(letterLbl);
            letterLbl.style.position = Position.Absolute;

            var xStep = word.layout.width / (letterUnits.Count + 1);
            var index = i;

            letterLbl.RegisterCallbackOnce<GeometryChangedEvent>(e =>
            {
                var height = letterLbl.layout.height;
                var width = letterLbl.layout.width;


                letterLbl.style.top = _rootHeight - (viewPos.y + height / 1.5f);
                letterLbl.style.left = viewPos.x - width / 2;

                Vector2 targetPos;
                targetPos.y = word.worldBound.position.y;

                targetPos.x = word.worldBound.position.x + xStep * index;

                MoveLetter(letterLbl, targetPos);
            });

        }


    }

    private void MoveLetter(Label letterLbl, Vector2 targetPos)
    {
        Debug.Log($"target pos x: {targetPos.x} y: {targetPos.y}");
        var startPos = letterLbl.layout.position;
        var endPos = targetPos;

        var x = endPos.x;
        var y = endPos.y - letterLbl.layout.height;
        var fontSize = 34;

        DOTween.To(() => letterLbl.style.left.value.value, x => letterLbl.style.left = x, endPos.x, _letterAnimDuration);
        DOTween.To(() => letterLbl.style.top.value.value, y => letterLbl.style.top = y, endPos.y, _letterAnimDuration);
        DOTween.To(() => letterLbl.style.fontSize.value.value, fontSize => letterLbl.style.fontSize = fontSize, fontSize, _letterAnimDuration).OnComplete(() =>
        {
            letterLbl.RemoveFromHierarchy();
            PlayWordFoundFX(endPos);
        });

    }


    private void PlayWordFoundFX(Vector2 endPos)
    {
        var worldPos = Camera.main.ScreenToWorldPoint(new Vector2(endPos.x, _rootHeight - endPos.y));

        var fx = Instantiate(_wordFoundFX, worldPos, Quaternion.identity);
        fx.Play();

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
        _wordsHolder.Clear();

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
        await Task.Delay((int)_letterAnimDuration * 1000);
        var label = _words[word];
        if (label.ClassListContains(WORD_GRAY)) return;

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

    private void OnDestroy()
    {
        _navRow.Unsubscribe();
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
