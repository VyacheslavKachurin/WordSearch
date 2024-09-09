using System;
using Unity.VisualScripting;
using UnityEngine;

public class InputTrigger : MonoBehaviour
{
    public static event Action<LetterUnit> OnLetterEnter;
    public static event Action<LetterUnit> OnLetterExit;

    private void OnTriggerEnter2D(Collider2D letter)
    {
        OnLetterEnter?.Invoke(letter.GetComponent<LetterUnit>());
    }

    private void OnTriggerExit2D(Collider2D letter)
    {
        OnLetterExit?.Invoke(letter.GetComponent<LetterUnit>());
    }

    private void OnDestroy()
    {
        Debug.Log($"Input trigger destroyed: {name}");
    }

}