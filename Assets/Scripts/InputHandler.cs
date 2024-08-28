using System;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static event Action<LetterUnit> OnLetterClick;
    public static event Action<Direction, Vector2> OnDrag;
    public static event Action OnInputStop;

    private Camera _cam;

    private void Start()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        if (Input.touchCount == 0) return;
        var touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            if (!DoRay(out var letter)) return;
            OnLetterClick?.Invoke(letter);
        }


        if (touch.phase == TouchPhase.Moved)
        {
            if (DoRay(out var letter))
            {
                OnLetterClick?.Invoke(letter);
            }
            else
            {
                var touchPos = _cam.ScreenToWorldPoint(touch.position);
                var lastTouchPos = _cam.ScreenToWorldPoint(touch.position - touch.deltaPosition);
                Vector2 dirNormalized = (touchPos - lastTouchPos).normalized;
                if (dirNormalized == Vector2.zero) return;
                var direction = GetDirection(dirNormalized);
                OnDrag?.Invoke(direction, lastTouchPos);
            }
        }

        if (touch.phase == TouchPhase.Ended)
        {
            OnInputStop?.Invoke();
        }
    }

    private Direction GetDirection(Vector2 direction)
    {
        switch (direction)
        {
            case { x: < 0 }:
                return Direction.Left;
            case { x: > 0 }:
                return Direction.Right;
            case { y: < 0 }:
                return Direction.Up;
            case { y: > 0 }:
                return Direction.Down;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }



    private bool DoRay(out LetterUnit letter)
    {
        letter = null;
        var ray = _cam.ScreenPointToRay(Input.mousePosition);

        var hit = Physics2D.Raycast(ray.origin, ray.direction);
        if (hit.collider != null)
        {
            letter = hit.collider.GetComponent<LetterUnit>();
        }
        return hit.collider != null;

    }
}

public enum Direction { Up, Down, Left, Right }