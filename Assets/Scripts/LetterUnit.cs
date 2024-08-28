using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class LetterUnit : MonoBehaviour
{
    private TextMeshPro _text;
    public char Letter => _text.text[0];

    void Awake()
    {
        _text = GetComponent<TextMeshPro>();
    }


    public void SetLetter(char letter)
    {
        _text = GetComponent<TextMeshPro>();
        _text.text = letter.ToString().ToUpper();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Letter collided with {collision.gameObject.name}");
    }

    internal void SetSize(Vector2 letterSize, float scale)
    {
        var size = letterSize.x > letterSize.y ? letterSize.y : letterSize.x;
        size *= scale;
        transform.localScale = new Vector2(size, size);
    }
}
