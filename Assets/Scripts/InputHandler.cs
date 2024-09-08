using System;
using UnityEngine;

public class InputHandler : MonoBehaviour
{

    [SerializeField] LayerMask _detectLayerMask;
    public static event Action<LetterUnit> OnLetterHover;
    public static event Action<Vector2> OnPointerDrag;
    public static event Action OnInputStop;

    private Camera _cam;
    private bool _isSelecting = false;


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
            OnLetterHover?.Invoke(letter);
            _isSelecting = true;
        }


        if (touch.phase == TouchPhase.Moved && _isSelecting)
        {
            if (DoRay(out var letter))
            {
                OnLetterHover?.Invoke(letter);
            }
            else
            {
                var touchPos = _cam.ScreenToWorldPoint(touch.position);
                var lastTouchPos = _cam.ScreenToWorldPoint(touch.position - touch.deltaPosition);
                Vector2 dirNormalized = (touchPos - lastTouchPos).normalized;
                if (dirNormalized == Vector2.zero) return;

                OnPointerDrag?.Invoke(lastTouchPos);
            }
        }

        if (touch.phase == TouchPhase.Ended)
        {
            OnInputStop?.Invoke();
            _isSelecting = false;
        }
    }

    private Direction GetDirection(Vector2 touchPos, Vector2 lastTouchPos)
    {
        var dir = Direction.Up;

        if (touchPos.x > lastTouchPos.x && touchPos.y == lastTouchPos.y)
        {
            dir = Direction.Right;
        }
        else if (touchPos.x < lastTouchPos.x && touchPos.y < lastTouchPos.y)
        {
            dir = Direction.Left;
        }
        else if (touchPos.x < lastTouchPos.x && touchPos.y > lastTouchPos.y)
        {
            dir = Direction.Down;
        }
        else if (touchPos.x > lastTouchPos.x && touchPos.y > lastTouchPos.y)
        {
            dir = Direction.Up;
        }
        return dir;

    }



    private bool DoRay(out LetterUnit letter)
    {
        letter = null;
        var ray = _cam.ScreenPointToRay(Input.mousePosition);

        var hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, (int)_detectLayerMask);
        if (hit.collider != null)
        {
            letter = hit.collider.GetComponent<LetterUnit>();
        }
        return hit.collider != null;

    }
}

