
using UnityEngine;

public class CoinsFX_Handler : MonoBehaviour
{
    [SerializeField] private ParticleSystem _coins;
    [SerializeField] private ParticleSystemForceField _forceField;
    [SerializeField] private Collider2D _trigger;


    public void PlayCoinsFX(Vector2 startPos)
    {
        var zeroPos = startPos;
        var fx = Instantiate(_coins, new Vector2(zeroPos.x, -zeroPos.y), Quaternion.identity);
        fx.trigger.AddCollider(_trigger);

    }

    public void SetForceField(Vector2 worldPos)
    {
        Debug.Log($"Setting Force Field: {worldPos}");
        _forceField.transform.position = worldPos;
    }

}