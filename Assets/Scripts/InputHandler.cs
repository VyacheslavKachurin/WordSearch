using System;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static event Action<Vector2> OnInputDrag;

    [SerializeField] LayerMask _detectLayerMask;
    [SerializeField] InputTrigger _inputTriggerPrefab;

    public static event Action OnInputStop;

    private Camera _cam;
    private bool _isSelecting = false;

    private InputTrigger _trigger;
    private Direction _direction;
    private Diagonal _diagonalDir;
    private List<LetterUnit> _letterUnits;

    private void Start()
    {
        _cam = Camera.main;
    }

    public void SetLetterUnits(List<LetterUnit> letterUnits) => _letterUnits = letterUnits;


    private void Update()
    {
        if (Input.touchCount == 0) return;
        var touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            var pos = ToWorldPosition(touch.position);
            _trigger = Instantiate(_inputTriggerPrefab, pos, Quaternion.identity);
            _direction = Direction.None;
        }


        if (touch.phase == TouchPhase.Moved)
        {
            Vector2 newPos;
            if (_letterUnits.Count < 2)
                newPos = ToWorldPosition(touch.position);
            else
            {
                _direction = _direction == Direction.None ? GetDirection() : _direction;
                _diagonalDir = _diagonalDir == Diagonal.None ? GetDiagonalDirection() : _diagonalDir;
                newPos = GetNextPos(ToWorldPosition(touch.position), _direction);
            }

            _trigger.transform.position = newPos;

            if (_direction == Direction.Diagonal && IsOnDiagonalDirection(ToWorldPosition(touch.position)))
                OnInputDrag?.Invoke(newPos);
            else if (_direction != Direction.Diagonal)
                OnInputDrag?.Invoke(newPos);

        }

        if (touch.phase == TouchPhase.Ended)
        {
            OnInputStop?.Invoke();
            Destroy(_trigger.gameObject);
            _trigger = null;
            //_isSelecting = false;
            _direction = Direction.None;
            _diagonalDir = Diagonal.None;
        }
    }

    private Diagonal GetDiagonalDirection()
    {
        var a = _letterUnits[0].transform.position;
        var b = _letterUnits[1].transform.position;

        if (a.x < b.x && a.y < b.y) return Diagonal.UpRight;
        if (a.x < b.x && a.y > b.y) return Diagonal.DownRight;
        if (a.x > b.x && a.y < b.y) return Diagonal.UpLeft;
        if (a.x > b.x && a.y > b.y) return Diagonal.DownLeft;
        return Diagonal.None;
    }

    private Vector2 GetNextDiagonalPoint(Vector2 point)
    {
        if (_letterUnits.Count < 2) return point;
        Vector2 AB = _letterUnits[1].transform.position - _letterUnits[0].transform.position;
        Vector2 AP = point - (Vector2)_letterUnits[0].transform.position;
        float t = Vector2.Dot(AP, AB) / Vector2.Dot(AB, AB);
        Vector2 nextPoint = (Vector2)_letterUnits[0].transform.position + AB * t;
        return nextPoint;

    }

    private bool IsOnDiagonalDirection(Vector2 point)
    {
        var letter = _letterUnits[^1].transform.position;
        if (_letterUnits.Count < 2) return false;

        if (point.x > letter.x && point.y > letter.y && _diagonalDir == Diagonal.UpRight) return true;
        if (point.x > letter.x && point.y < letter.y && _diagonalDir == Diagonal.DownRight) return true;
        if (point.x < letter.x && point.y > letter.y && _diagonalDir == Diagonal.UpLeft) return true;
        if (point.x < letter.x && point.y < letter.y && _diagonalDir == Diagonal.DownLeft) return true;
        return false;
    }

    internal Vector2 GetNextPos(Vector2 point, Direction direction)
    {

        var lastLetterPos = _letterUnits[^1].transform.position;
        Vector2 newPoint;
        switch (direction)
        {
            case Direction.Left:
            case Direction.Right:
                newPoint = new Vector2(point.x, lastLetterPos.y);
                break;
            case Direction.Up:
            case Direction.Down:
                newPoint = new Vector2(lastLetterPos.x, point.y);
                break;
            case Direction.Diagonal:
                newPoint = GetNextDiagonalPoint(point);
                break;
            default:
                newPoint = point;
                break;
        }
        return newPoint;
    }

    private Direction GetDirection()
    {
        var secondLetter = _letterUnits[1].transform.position;
        var firstLetter = _letterUnits[0].transform.position;

        if (secondLetter.x > firstLetter.x && secondLetter.y == firstLetter.y) _direction = Direction.Right;
        else if (secondLetter.x < firstLetter.x && secondLetter.y == firstLetter.y) _direction = Direction.Left;
        else if (secondLetter.y > firstLetter.y && secondLetter.x == firstLetter.x) _direction = Direction.Up;
        else if (secondLetter.y < firstLetter.y && secondLetter.x == firstLetter.x) _direction = Direction.Down;

        else _direction = Direction.Diagonal;

        return _direction;
    }

    private Vector2 ToWorldPosition(Vector2 touchPos)
    {
        return _cam.ScreenToWorldPoint(touchPos);
    }

}

public enum Diagonal
{
    None,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}

