using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class LetterUnit : MonoBehaviour
{

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
    [SerializeField] private float _animScaleUp = 1.5f;
    private float _baseScale;

    [SerializeField] private float _animScaleDuration = 0.3f;

    private List<Direction> _possibleDirections;
    public List<Direction> PossibleDirections => _possibleDirections;
    public Vector2 Pos => transform.position;

    void Awake()
    {
        _text = GetComponent<TextMeshPro>();
        _collider = GetComponent<BoxCollider2D>();

    }

    [ContextMenu("Log Scale")]
    private void LogScale()
    {
        Debug.Log(transform.position);
    }

    public void GetPossibleDirections()
    {

    }


    public void SetLetter(char letter, Point point)
    {
        _text = GetComponent<TextMeshPro>();
        _text.text = letter.ToString().ToUpper();
        Point = point;
    }

    public void SetPossibleDirections(List<Direction> possibleDirections)
    {
        _possibleDirections = possibleDirections;
    }

    internal void SetSize(Vector2 letterSize, float scale)
    {
        var size = letterSize.x > letterSize.y ? letterSize.y : letterSize.x;
        size *= scale;
        transform.localScale = new Vector2(size, size);
        _baseScale = transform.localScale.x;
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
        // Debug.Log($"Animate selection: {isSelected} color: {targetColor}");
        _text.color = targetColor;
    }

    [ContextMenu("Scale")]
    public void AnimateScale()
    {
        transform.DOScale(_animScaleUp, _animScaleDuration).SetEase(Ease.InOutSine).OnComplete(() => transform.DOScale(_baseScale, _animScaleDuration).SetEase(Ease.InOutSine));
    }


    public static Vector2 operator -(LetterUnit a, LetterUnit b) => a.transform.position - b.transform.position;

}
