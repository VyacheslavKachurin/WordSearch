
using System;
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
    public List<Point> FirstLetters;
    [Preserve]
    public List<string> FoundWords;
    [Preserve]
    public List<Point> FoundLetters;
    [Preserve]
    public List<LineState> Lines;
    [Preserve]
    public List<Point> OpenLetters;
    [Preserve]
    public List<Point> RevealedFakeLetters;
    [Preserve]

    public LevelState(bool fakeLettersRemoved, List<Point> firstLetters, List<string> foundWords, List<Point> foundLetters, List<LineState> lines, List<Point> openLetters)
    {
        FakeLettersRemoved = fakeLettersRemoved;
        FirstLetters = firstLetters;
        FoundWords = foundWords;
        FoundLetters = foundLetters;
        Lines = lines;
        OpenLetters = openLetters;
    }

}