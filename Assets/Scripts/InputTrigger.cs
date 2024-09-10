using System;
using Unity.VisualScripting;
using UnityEngine;

public class InputTrigger : MonoBehaviour
{
    public static event Action<LetterUnit> OnLetterEnter;
    public static event Action<LetterUnit> OnLetterExit;

    private void OnTriggerEnter2D(Collider2D letter)
    {
        var res = letter.TryGetComponent<LetterUnit>(out var letterUnit);
        if (!res) return;
        OnLetterEnter?.Invoke(letterUnit);
    }

    private void OnTriggerExit2D(Collider2D letter)
    {
        OnLetterExit?.Invoke(letter.GetComponent<LetterUnit>());
    }

}