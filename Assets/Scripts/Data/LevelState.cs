using System.Collections.Generic;

public class LevelState
{

    public int Level;
    public bool FakeLettersRemoved;
    public List<Point> ActiveFirstLetters;
    public List<string> FoundWords;
    public List<Point> FoundLetters;


    public LevelState(bool fakeLettersRemoved, List<Point> firstLetters, List<string> foundWords, List<Point> foundLetters)
    {
        FakeLettersRemoved = fakeLettersRemoved;
        ActiveFirstLetters = firstLetters;
        FoundWords = foundWords;
        FoundLetters = foundLetters;
    }
}