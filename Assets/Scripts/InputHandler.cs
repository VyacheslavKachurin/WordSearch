using System;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static event Action<Vector2> OnInputDrag;

    [SerializeField] LayerMask _detectLayerMask;
    [SerializeField] InputTrigger _inputTriggerPrefab;

    public static event Action OnInputStop;
    public static event Action<LetterUnit> OnLetterDeselect;

    private Camera _cam;

    private InputTrigger _trigger;
    private Direction _direction;
    private bool _isSelecting;
    private Diagonal _diagonalDir;
    private List<LetterUnit> _letterUnits;
    private Touch _touch;
    [SerializeField] private float _speed = 100;

    private void Start()
    {
        _cam = Camera.main;
        InputTrigger.OnLetterExit += HandleLetterExit;
    }

    private void OnDestroy()
    {
        InputTrigger.OnLetterExit -= HandleLetterExit;
    }

    private void HandleLetterExit(LetterUnit unit)
    {
        if (!_isSelecting) return;
        var isOnDirection = IsOnDirection(_trigger.transform.position);



        if (!isOnDirection) OnLetterDeselect?.Invoke(unit);

    }

    public void SetLetterUnits(List<LetterUnit> letterUnits) => _letterUnits = letterUnits;


    private void Update()
    {
        if (!Session.IsSelecting) return;
        if (Input.touchCount == 0) return;
        _touch = Input.GetTouch(0);

        if (_touch.phase == TouchPhase.Began)
        {
            if (!IsOnGameField(_touch.position)) return;
            var pos = ToWorldPosition(_touch.position);
            _trigger = Instantiate(_inputTriggerPrefab, pos, Quaternion.identity);
            _direction = Direction.None;
            _isSelecting = true;
        }


        if (_touch.phase == TouchPhase.Moved && _isSelecting)
        {
            if (!IsOnGameField(_touch.position)) return;

            Vector2 newPos;
            if (_letterUnits.Count < 2)
                newPos = ToWorldPosition(_touch.position);
            else
            {
                _direction = _direction == Direction.None ? GetDirection() : _direction;
                _diagonalDir = _diagonalDir == Diagonal.None ? GetDiagonalDirection() : _diagonalDir;
                newPos = GetNextPos(ToWorldPosition(_touch.position), _direction);
            }


            if (Vector2.Distance(_trigger.transform.position, newPos) > 0.1f)
                _trigger.Move(newPos);

            if (_direction == Direction.Diagonal && IsOnDiagonalDirection(ToWorldPosition(_touch.position)))
                OnInputDrag?.Invoke(newPos);
            else if (_direction != Direction.Diagonal)
                OnInputDrag?.Invoke(newPos);

        }

        if (_touch.phase == TouchPhase.Ended && _isSelecting)
        {
            StopSelecting();
        }
    }

    private void StopSelecting()
    {
        _isSelecting = false;
        OnInputStop?.Invoke();
        Destroy(_trigger.gameObject);
        _trigger = null;
        _direction = Direction.None;
        _diagonalDir = Diagonal.None;
        _touch = default;
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

    private bool IsOnGameField(Vector2 point)
    {
        var ray = _cam.ScreenPointToRay(point);
        if (Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, _detectLayerMask)) return true;
        return false;
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

    private bool IsOnDirection(Vector2 triggerPos)
    {
        if (_letterUnits.Count < 2) return true;
        var lastLetter = _letterUnits[^1].transform.position;
        if (triggerPos.x > lastLetter.x && triggerPos.y == lastLetter.y && _direction == Direction.Right) return true;
        else if (triggerPos.x < lastLetter.x && triggerPos.y == lastLetter.y && _direction == Direction.Left) return true;
        else if (triggerPos.y > lastLetter.y && triggerPos.x == lastLetter.x && _direction == Direction.Up) return true;
        else if (triggerPos.y < lastLetter.y && triggerPos.x == lastLetter.x && _direction == Direction.Down) return true;
        else if (triggerPos.x > lastLetter.x && triggerPos.y > lastLetter.y && _diagonalDir == Diagonal.UpRight) return true;
        else if (triggerPos.x > lastLetter.x && triggerPos.y < lastLetter.y && _diagonalDir == Diagonal.DownRight) return true;
        else if (triggerPos.x < lastLetter.x && triggerPos.y > lastLetter.y && _diagonalDir == Diagonal.UpLeft) return true;
        else if (triggerPos.x < lastLetter.x && triggerPos.y < lastLetter.y && _diagonalDir == Diagonal.DownLeft) return true;

        else
        {
            //            Debug.Log($"Not on direction: {_direction} \n triggerPos: {triggerPos} \n lastLetter: {lastLetter}");
            return false;
        }
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

