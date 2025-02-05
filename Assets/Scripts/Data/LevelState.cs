
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;


public class LevelState
{

    public bool FakeLettersRemoved { get; set; }
    public List<Point> ActiveFirstLetters { get; set; }
    public List<Point> RevealedFakeLetters { get; set; }
    public List<RevealedLetter> RevealedFirstLetters { get; set; }

    public List<FoundWord> FoundWords { get; set; }


    public LevelState(bool fakeLettersRemoved, List<Point> firstLetters)
    {
        FakeLettersRemoved = fakeLettersRemoved;
        ActiveFirstLetters = firstLetters;
        FoundWords = new List<FoundWord>();
        RevealedFakeLetters = new List<Point>();
        RevealedFirstLetters = new List<RevealedLetter>();
    }

}