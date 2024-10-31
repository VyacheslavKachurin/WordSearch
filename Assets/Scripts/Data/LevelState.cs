
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class LevelState
{
    [Preserve]
    public int Level;
    [Preserve]
    public bool FakeLettersRemoved;
    [Preserve]
    public List<Point> ActiveFirstLetters;
    [Preserve]
    public List<string> FoundWords;
    [Preserve]
    public List<Point> FoundLetters;
    [Preserve]
    public List<LineState> Lines;
    [Preserve]
    public List<Point> OpenLetters;
    [Preserve]

    public LevelState(bool fakeLettersRemoved, List<Point> firstLetters, List<string> foundWords, List<Point> foundLetters, List<LineState> lines, List<Point> openLetters)
    {
        FakeLettersRemoved = fakeLettersRemoved;
        ActiveFirstLetters = firstLetters;
        FoundWords = foundWords;
        FoundLetters = foundLetters;
        Lines = lines;
        OpenLetters = openLetters;
    }
}