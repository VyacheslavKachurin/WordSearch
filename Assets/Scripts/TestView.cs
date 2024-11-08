using System;
using UnityEngine;
using UnityEngine.UIElements;

public class TestView : MonoBehaviour
{
    public static event Action<Vector2> PicResolved;

    private VisualElement _root;
    private VisualElement _picStart;
    private VisualElement _picAbsolute;


    [SerializeField] CoinsFX_Handler _coinsFX_Handler;

    private void Start()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;
        _picStart = _root.Q<VisualElement>("pic-start");
        _picAbsolute = _root.Q<VisualElement>("pic-absolute");
        _picAbsolute.RegisterCallback<GeometryChangedEvent>((e) =>
        {

            var finalPos = _picAbsolute.GetWorldPosition(_root);

            _coinsFX_Handler.SetForceField(finalPos);
        });
    }

    [ContextMenu("CreateAnim")]
    private void CreateAnim()
    {
        var worldPos = _picStart.GetWorldPosition(_root);
        _coinsFX_Handler.PlayCoinsFX(worldPos);
    }

}