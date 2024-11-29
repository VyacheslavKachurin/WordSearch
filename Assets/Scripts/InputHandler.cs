using System;

using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static event Action<Vector2, Direction> OnTriggerMove;

    [SerializeField] LayerMask _letterMask;
    [SerializeField] LayerMask _gameBoardMask;
    [SerializeField] InputTrigger _inputTriggerPrefab;

    public static event Action OnInputStop;
    public static event Action<LetterUnit, Vector2> OnLetterDeselect;

    private Camera _cam;

    private InputTrigger _trigger;
    private Direction _direction;
    private bool _isSelecting;

    // private List<LetterUnit> _letterUnits;
    private Touch _touch;
    [SerializeField] private float _speed = 40;
    private Vector2 _upLeft;
    private Vector2 _upRight;
    private Vector2 _downLeft;
    private Vector2 _downRight;
    [SerializeField] private Sprite _debugSprite;
    private List<GameObject> _debugSprites;
    private Collider2D _lastHitCollider;

    private float _horizontalDistance;
    private float _verticalDistance;
    private float _diagonalDistance;
    private bool _isMoving = false;

    private Vector2? _nextPosition;
    private Vector2 _triggerDirection;

    [SerializeField] private LetterUnit _targetLetter;
    [SerializeField] private LetterUnit _firstLetter;

    private List<LetterUnit> _letterUnits = new List<LetterUnit>();

    [ContextMenu("ImitateInput")]
    private void ImitateInput()
    {
        _trigger = Instantiate(_inputTriggerPrefab, _firstLetter.transform.position, Quaternion.identity);
        _nextPosition = _targetLetter.transform.position;
        _isMoving = true;
    }

    [ContextMenu("StopInput")]
    private void StopInput()
    {
        _isMoving = false;
        _isSelecting = false;
        OnInputStop?.Invoke();
        Destroy(_trigger.gameObject);
        _trigger = null;
        _direction = Direction.None;
    }

    private void Start()
    {
        _cam = Camera.main;
        InputTrigger.OnLetterExit += HandleLetterExit;
        InputTrigger.OnLetterEnter += HandleLetterEnter;
    }

    private void HandleLetterEnter(LetterUnit unit)
    {
        if (!_letterUnits.Contains(unit))
        {
            _letterUnits.Add(unit);
        }
        if (_letterUnits.Count == 1)
            _direction = Direction.None;
        //        Debug.Log($"Letter units count: {_letterUnits.Count}");
        _lastHitCollider = unit.GetComponent<Collider2D>();
        // _nextPosition = unit.transform.position;

    }

    private void OnDestroy()
    {
        InputTrigger.OnLetterExit -= HandleLetterExit;
        InputTrigger.OnLetterEnter -= HandleLetterEnter;

    }

    private void HandleLetterExit(LetterUnit unit)
    {
        if (!_isSelecting) return;
        var isOnDirection = IsOnDirection(_trigger.RbPosition);

        if (!isOnDirection)
        {
            //  Debug.Log($"we are not on direction;current dir: {_direction}");
            _letterUnits.Remove(unit);
            OnLetterDeselect?.Invoke(unit, _trigger.RbPosition);
            if (_letterUnits.Count == 1)
            {
                _lastHitCollider = _letterUnits[0].GetComponent<Collider2D>();
                //  _trigger.RbPosition = _letterUnits[0].transform.position;

                var touchPos = Camera.main.ScreenToWorldPoint(_touch.position);
                if (Vector2.Distance(_trigger.RbPosition, touchPos) > _horizontalDistance)
                {
                    FinishSelecting();
                }
                //   _isMoving = false;
                //  _nextPosition = _letterUnits[0].transform.position;
            }

        }
    }


    public void SetLetterDistances(List<float> distances)
    {
        _horizontalDistance = distances[0];
        _verticalDistance = distances[1];
        _diagonalDistance = distances[2];

    }

    private void Update()
    {
        if (!Session.IsSelecting) return;
        if (Input.touchCount == 0) return;
        _touch = Input.GetTouch(0);

        if (_touch.phase == TouchPhase.Began)
        {
            if (!IsOnGameField(_touch.position))
            {
                Debug.Log($"not on game field");
                return;
            }

            // var pos = ToWorldPosition(_touch.position);

            var ray = _cam.ScreenPointToRay(_touch.position);
            var hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, _letterMask);

            if (hit && hit.collider.name.Contains("Letter"))
            {
                _lastHitCollider = hit.collider;
            }
            else
            {
                var worldTouchPos = Camera.main.ScreenToWorldPoint(_touch.position);
                var letters = Physics2D.OverlapCircleAll(worldTouchPos, _horizontalDistance);
                Debug.Log($"letters count: {letters.Length}");

                _lastHitCollider = FindTheClosestLetter(letters, worldTouchPos);
                if (_lastHitCollider == null) return;
            }

            ShowPositions(_lastHitCollider.GetComponent<LetterUnit>());

            _trigger = Instantiate(_inputTriggerPrefab, _lastHitCollider.transform.position, Quaternion.identity);
            _direction = Direction.None;
            _isSelecting = true;
        }

        if (_touch.phase == TouchPhase.Moved && _isSelecting)
        {
            if (!IsOnGameField(_touch.position)) return;
            if (_isMoving) return;

            if (Vector2.Distance(_lastHitCollider.transform.position, _cam.ScreenToWorldPoint(_touch.position)) < _upLeft.magnitude * 0.7f)
            {
                _trigger.RbPosition = _lastHitCollider.transform.position;
                return;
            }

            else
            {
                var lastLetter = _lastHitCollider.GetComponent<LetterUnit>(); // TODO : FIX IT
                if (_letterUnits.Count < 2) SetDirection(_touch.position, lastLetter);

                var newPos = GetNewPosition(_touch.position, lastLetter);

                _nextPosition = newPos;

                _triggerDirection = (Vector2)_nextPosition - _trigger.Rb.position;
                _triggerDirection.Normalize();
                _isMoving = true;

                return;

            }

        }


        if (_touch.phase == TouchPhase.Ended && _isSelecting)
        {
            FinishSelecting();
        }

    }

    private void FinishSelecting()
    {
        StopSelecting();
        DeletePositions();
        _lastHitCollider = null;
        _direction = Direction.None;
        _nextPosition = null;
        _isMoving = false;
        _letterUnits.Clear();
    }


    private Collider2D? FindTheClosestLetter(Collider2D[] letters, Vector3 worldTouchPos)
    {
        Debug.Log($"start looping letters");
        Transform closest = null;
        var closestDistance = _horizontalDistance;
        foreach (var letter in letters)
        {
            var distance = Vector2.Distance(worldTouchPos, letter.transform.position);
            if (distance < closestDistance)
            {
                closest = letter.transform;
                closestDistance = distance;
            }
        }
        if (closest == null) return null;
        else
            return closest.TryGetComponent<Collider2D>(out var collider) ? collider : null;
    }


    private void FixedUpdate()
    {
        if (!_isMoving) return;

        Vector2 targetPosition = (Vector2)_nextPosition;
        Vector2 currentPosition = _trigger.Rb.position;
        float step = _speed * Time.fixedDeltaTime;

        /*
                if (Vector2.Distance(currentPosition, targetPosition) > _horizontalDistance / 2)
                {
                    _isMoving = false;
                    return;
                }
                */


        _trigger.Rb.position = Vector2.MoveTowards(currentPosition, targetPosition, step);

        OnTriggerMove?.Invoke(_trigger.RbPosition, _direction);

        if (Vector2.Distance(_trigger.Rb.position, targetPosition) < 0.1f)
        {
            _trigger.Rb.linearVelocity = Vector2.zero;

            _isMoving = false;
        }
    }


    private Vector2 GetNewPosition(Vector2 touchPos, LetterUnit letter)
    {
        var worldTouchPos = _cam.ScreenToWorldPoint(touchPos);
        var possibleDirections = letter.PossibleDirections;
        var isDirectionAvailable = possibleDirections.Contains(_direction);

        Vector2 newPos = letter.Pos;
        switch (_direction)
        {
            case Direction.Left:
                newPos = new Vector2(worldTouchPos.x, letter.Pos.y);
                newPos.x = isDirectionAvailable ? worldTouchPos.x : Mathf.Max(worldTouchPos.x, letter.Pos.x);
                break;
            case Direction.Right:
                newPos = new Vector2(worldTouchPos.x, letter.Pos.y);
                newPos.x = isDirectionAvailable ? worldTouchPos.x : Mathf.Min(worldTouchPos.x, letter.Pos.x);
                break;
            case Direction.Up:
                newPos = new Vector2(letter.Pos.x, worldTouchPos.y);
                newPos.y = isDirectionAvailable ? worldTouchPos.y : Mathf.Min(worldTouchPos.y, letter.Pos.y);
                break;
            case Direction.Down:
                newPos = new Vector2(letter.Pos.x, worldTouchPos.y);
                newPos.y = isDirectionAvailable ? worldTouchPos.y : Mathf.Max(worldTouchPos.y, letter.Pos.y);
                break;
            default:
                var nextDiagonalPos = GetNextDiagonalLetterPos(letter);
                newPos = GetPointOnLine(letter.Pos, nextDiagonalPos, worldTouchPos);
                newPos = isDirectionAvailable ? newPos : ClampDiagonalDirection(newPos, possibleDirections, letter);
                break;
        }
        return newPos;

    }

    private Vector2 ClampDiagonalDirection(Vector2 newPos, List<Direction> possibleDirections, LetterUnit letter)
    {
        switch (_direction)
        {
            case Direction.FromUpLeftToDownRight:
                newPos.x = Mathf.Min(newPos.x, letter.Pos.x);
                newPos.y = Mathf.Max(newPos.y, letter.Pos.y);
                break;
            case Direction.FromUpRightToDownLeft:
                newPos.x = Mathf.Max(newPos.x, letter.Pos.x);
                newPos.y = Mathf.Max(newPos.y, letter.Pos.y);
                break;
            case Direction.FromDownLeftToUpRight:
                newPos.x = Mathf.Min(newPos.x, letter.Pos.x);
                newPos.y = Mathf.Min(newPos.y, letter.Pos.y);
                break;
            case Direction.FromDownRightToUpLeft:
                newPos.x = Mathf.Max(newPos.x, letter.Pos.x);
                newPos.y = Mathf.Min(newPos.y, letter.Pos.y);
                break;
            default:
                break;
        }
        return newPos;
    }

    private Vector2 GetNextDiagonalLetterPos(LetterUnit letter)
    {
        switch (_direction)
        {
            case Direction.FromUpLeftToDownRight:
                return new Vector2(letter.Pos.x + _horizontalDistance, letter.Pos.y - _verticalDistance);
            case Direction.FromUpRightToDownLeft:
                return new Vector2(letter.Pos.x - _horizontalDistance, letter.Pos.y - _verticalDistance);
            case Direction.FromDownLeftToUpRight:
                return new Vector2(letter.Pos.x + _horizontalDistance, letter.Pos.y + _verticalDistance);
            case Direction.FromDownRightToUpLeft:
                return new Vector2(letter.Pos.x - _horizontalDistance, letter.Pos.y + _verticalDistance);
            default:
                return letter.Pos;
        }

    }

    private Vector2 GetPointOnLine(Vector2 letterPos, Vector2 endPos, Vector2 worldTouchPos)
    {
        Vector2 lineDirection = (endPos - letterPos).normalized;
        // float projectionLength = Vector2.Dot(worldTouchPos - letterPos, lineDirection);
        float projectionLength = Vector2.Dot(worldTouchPos - letterPos, lineDirection);
        //   projectionLength = Mathf.Clamp(projectionLength, 0, Vector2.Distance(letterPos, endPos)); // Constrain within A and B
        return letterPos + lineDirection * projectionLength;
    }

    private void StopSelecting()
    {
        _isSelecting = false;
        OnInputStop?.Invoke();
        Destroy(_trigger.gameObject);
        _trigger = null;
        _direction = Direction.None;

        _touch = default;
    }


    private bool IsOnGameField(Vector2 point)
    {
        var ray = _cam.ScreenPointToRay(point);
        if (Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, _gameBoardMask)) return true;
        return false;
    }


    private bool IsOnDirection(Vector2 triggerPos)
    {
        //Debug.Log($"letter units count:{_letterUnits.Count}");
        if (_letterUnits.Count < 2) return true;
        var lastLetter = _letterUnits[^1].transform.position;
        if (triggerPos.x > lastLetter.x && triggerPos.y == lastLetter.y && _direction == Direction.Right) return true;
        else if (triggerPos.x < lastLetter.x && triggerPos.y == lastLetter.y && _direction == Direction.Left) return true;
        else if (triggerPos.y > lastLetter.y && triggerPos.x == lastLetter.x && _direction == Direction.Up) return true;
        else if (triggerPos.y < lastLetter.y && triggerPos.x == lastLetter.x && _direction == Direction.Down) return true;
        else if (triggerPos.x < lastLetter.x && triggerPos.y < lastLetter.y && _direction == Direction.FromUpRightToDownLeft) return true;
        else if (triggerPos.x > lastLetter.x && triggerPos.y < lastLetter.y && _direction == Direction.FromUpLeftToDownRight) return true;
        else if (triggerPos.x > lastLetter.x && triggerPos.y > lastLetter.y && _direction == Direction.FromDownLeftToUpRight) return true;
        else if (triggerPos.x < lastLetter.x && triggerPos.y > lastLetter.y && _direction == Direction.FromDownRightToUpLeft) return true;

        else
        {
            return false;
        }
    }


    private void SetDirection(Vector2 touchPos, LetterUnit letter)
    {
        Vector2 worldPos = _cam.ScreenToWorldPoint(touchPos);
        var points = GetDirectionPoints(letter);
        var closestPoint = Extensions.FindClosestPoint(points, worldPos);

        foreach (var pair in points)
        {
            if (pair.Value == closestPoint)
            {
                _direction = pair.Key;
                break;
            }
        }
    }


    private Dictionary<Direction, Vector2> GetDirectionPoints(LetterUnit letter)
    {
        Vector2 letterPos = letter.transform.position;
        var possibleDirections = letter.PossibleDirections;
        var possiblePositions = new Dictionary<Direction, Vector2>();

        foreach (var dir in possibleDirections)
        {
            possiblePositions.Add(dir, GetPos(letterPos, dir));
        }

        return possiblePositions;

    }

    private Vector2 GetPos(Vector2 letterPos, Direction direction)
    {
        var addedVector = Vector2.zero;
        switch (direction)
        {
            case Direction.Up:
                addedVector = Vector2.up;
                break;
            case Direction.Down:
                addedVector = Vector2.down;
                break;
            case Direction.Left:
                addedVector = Vector2.left;
                break;
            case Direction.Right:
                addedVector = Vector2.right;
                break;
            case Direction.FromDownLeftToUpRight:
                addedVector = _upRight;
                break;
            case Direction.FromUpRightToDownLeft:
                addedVector = _downLeft;
                break;
            case Direction.FromDownRightToUpLeft:
                addedVector = _downRight;
                break;
            case Direction.FromUpLeftToDownRight:
                addedVector = _upLeft;
                break;
        }

        return letterPos + addedVector;
    }

    private void ShowPositions(LetterUnit letterUnit)
    {
        Vector2 letterPos = letterUnit.transform.position;
        var possibleDirections = letterUnit.PossibleDirections;
        var possiblePositions = new Dictionary<Direction, Vector2>();

        foreach (var dir in possibleDirections)
        {
            possiblePositions.Add(dir, GetPos(letterPos, dir));
        }

        void CreateSprite(Vector2 pos, Direction direction)
        {
            var go = new GameObject();
            go.transform.position = pos;
            go.AddComponent<SpriteRenderer>().sprite = _debugSprite;
            go.GetComponent<SpriteRenderer>().sortingOrder = 14;
            go.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
            go.name = "Debug Sprite " + direction;
            go.layer = LayerMask.NameToLayer("Default");

            _debugSprites.Add(go);
        }
        _debugSprites = new List<GameObject>();


        foreach (var pos in possiblePositions)
        {
            CreateSprite(pos.Value, pos.Key);
        }

    }

    private void DeletePositions()
    {
        //find gameobject with name Debug Sprite
        foreach (var sprite in _debugSprites)
        {
            Destroy(sprite);
        }
        _debugSprites.Clear();
    }



    internal void SetDirections(Dictionary<Direction, Vector2> directions)
    {
        _upLeft = directions[Direction.FromUpLeftToDownRight];
        _upRight = directions[Direction.FromDownLeftToUpRight];
        _downLeft = directions[Direction.FromUpRightToDownLeft];
        _downRight = directions[Direction.FromDownRightToUpLeft];

        _upLeft.Normalize();
        _upRight.Normalize();
        _downLeft.Normalize();
        _downRight.Normalize();
    }
}


public enum Direction
{
    Up, Down, Left, Right,
    None,
    FromDownLeftToUpRight, FromUpRightToDownLeft, FromDownRightToUpLeft, FromUpLeftToDownRight
}


