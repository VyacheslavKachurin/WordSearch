using System;
using UnityEngine;

public class InputMock : MonoBehaviour
{
    [SerializeField] private Transform _letter1;
    [SerializeField] private Transform _letter2;
    [SerializeField] private InputTrigger _triggerPrefab;
    [SerializeField] private float _triggerSpeed;
    private Vector2 _direction;

    private InputTrigger _triggerInstance;




    [ContextMenu("MoveTrigger")]
    private void MoveTrigger()
    {
        _triggerInstance = Instantiate(_triggerPrefab, _letter1.position, Quaternion.identity);
        _direction = _letter2.position - _letter1.position;
        _direction.Normalize();
        Debug.Log($"Direction: {_direction}");
        _triggerInstance.Move(_direction, _triggerSpeed, _letter2.position);

    }

    [ContextMenu("DestroyTrigger")]
    private void DestroyTrigger()
    {
        Destroy(_triggerInstance.gameObject);
    }

    internal void SetFirstLetter(LetterUnit letterUnit)
    {
        _letter1 = letterUnit.transform;
    }

    internal void SetSecondLetter(LetterUnit letterUnit)
    {
        _letter2 = letterUnit.transform;
    }
}