using System.Collections.Generic;
using Color = UnityEngine.Color;


public class LevelState
{

    public int Level;
    public bool FakeLettersRemoved;
    public List<Point> ActiveFirstLetters;
    public List<string> FoundWords;
    public List<Point> FoundLetters;
    public List<LineState> Lines;


    public LevelState(bool fakeLettersRemoved, List<Point> firstLetters, List<string> foundWords, List<Point> foundLetters, List<LineState> lines)
    {
        FakeLettersRemoved = fakeLettersRemoved;
        ActiveFirstLetters = firstLetters;
        FoundWords = foundWords;
        FoundLetters = foundLetters;
        Lines = lines;
    }
}