using System;
using Unity.VisualScripting;
using UnityEngine;

public class InputTrigger : MonoBehaviour
{
    public static event Action<LetterUnit> OnLetterEnter;
    public static event Action<LetterUnit> OnLetterExit;
    private Rigidbody2D _rb;

    private Vector2? _newPos;
    private float _speed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

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

    private void FixedUpdate()
    {
        if (_newPos == null) return;
        _rb.MovePosition((Vector2)_newPos);
    }

    public void Move(Vector2? pos)
    {
        _newPos = pos;

    }

}