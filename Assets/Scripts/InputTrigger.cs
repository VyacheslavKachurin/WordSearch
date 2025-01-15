using System;
using Unity.VisualScripting;
using UnityEngine;

public class InputTrigger : MonoBehaviour
{
    public static event Action<LetterUnit> OnLetterEnter;
    public static event Action<LetterUnit> OnLetterExit;
    private Rigidbody2D _rb;

    private Vector2? _direction;
    [SerializeField] private float _speed;
    private Vector2 _targetPos;

    public Vector2 RbPosition
    {
        get => _rb.position;
        set => _rb.position = value;
    }

    public Rigidbody2D Rb => _rb;

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
        var res = letter.TryGetComponent<LetterUnit>(out var letterUnit);
        if (!res) return;
        OnLetterExit?.Invoke(letter.GetComponent<LetterUnit>());

    }

    private void Update()
    {

        if (_direction == null) return;

        _rb.MovePosition((Vector2)_rb.position + (Vector2)_direction * Time.fixedDeltaTime * _speed);


        if (Vector2.Distance(_rb.position, _targetPos) < 0.1f)
        {
            _direction = null;
        }

    }

    public void Move(Vector2? dir, float speed, Vector2 targetPos)
    {
        _direction = dir;
        _speed = speed;
        _targetPos = targetPos;

    }

    public void ChangeColor()
    {
        gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
    }


    private void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log($"OnCollisionExit2D: {collision.gameObject.name}");
    }

    /*
        internal void SetAxis(Direction direction)
        {
            var pos = _rb.position;
            switch (direction)
            {
                case Direction.Up:
                    pos += Vector2.up;
                    break;
                case Direction.Down:
                    pos += Vector2.down;
                    break;
                case Direction.Left:
                    pos += Vector2.left;
                    break;
                case Direction.Right:
                    pos += Vector2.right;
                    break;
            }
           // _rb.MovePosition(pos);
           _rb.position = pos;
        }
        */

}