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
    [SerializeField] ColorData _colorData;
    [SerializeField] Color _disableColor = new(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] Color _activeColor = new(0, 0, 0, 1);
    [SerializeField] Color _foundColor = new(1, 1, 1, 1);

    private TextMeshPro _text;
    public char Letter => _text.text[0];
    private BoxCollider2D _collider;
    [SerializeField] private float _disableDelay = 0.1f;
    [SerializeField] private float _rotationLimit = 30;

    public Point Point;

    private Color? _lineColor = null;

    void Awake()
    {
        _text = GetComponent<TextMeshPro>();
        _collider = GetComponent<BoxCollider2D>();
    }


    public void SetLetter(char letter, Point point)
    {
        _text = GetComponent<TextMeshPro>();
        _text.text = letter.ToString().ToUpper();
        Point = point;
    }



    internal void SetSize(Vector2 letterSize, float scale)
    {
        var size = letterSize.x > letterSize.y ? letterSize.y : letterSize.x;
        size *= scale;
        transform.localScale = new Vector2(size, size);
    }

    internal void Disable()
    {
        _collider.enabled = false;
    }

    [ContextMenu("Disable")]
    internal void Hide()
    {

        StartCoroutine(AnimateDisable());
        StartCoroutine(AnimateRotate());
        Disable();
    }

    private IEnumerator AnimateRotate()
    {
        var randomRotation = UnityEngine.Random.Range(-_rotationLimit, _rotationLimit);

        var startRotation = _text.transform.rotation;
        var targetRotation = Quaternion.Euler(0, 0, randomRotation);
        var step = 0.1f;

        while (_text.transform.rotation.z != targetRotation.z)
        {
            _text.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, step);
            step += 0.1f;
            yield return new WaitForSeconds(_disableDelay);

        }
    }

    private IEnumerator AnimateDisable()
    {
        var startColor = _text.color;
        var step = 0.1f;

        while (_text.color.a != _disableColor.a)
        {
            _text.color = Color.Lerp(startColor, _disableColor, step);
            step += 0.1f;
            yield return new WaitForSeconds(_disableDelay);
        }
    }

    internal void AnimateSelection(bool isSelected)
    {
        var targetColor = isSelected ? _foundColor : _activeColor;
        Debug.Log($"Animate selection: {isSelected} color: {targetColor}");
        _text.color = targetColor;
    }

    internal Color GetColor()
    {
        _lineColor ??= _colorData.GetRandom();
        return (Color)_lineColor;
    }

}
