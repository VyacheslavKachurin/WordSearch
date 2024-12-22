
using UnityEngine;

public class CoinsFX_Handler : MonoBehaviour
{
    [SerializeField] private ParticleSystem _coins;
    [SerializeField] private ParticleSystemForceField _forceField;
    [SerializeField] private Collider2D _trigger;


    public void PlayCoinsFX(Vector2 startPos, int scale = 1)
    {
        var zeroPos = startPos;
        var fx = Instantiate(_coins, new Vector2(zeroPos.x, zeroPos.y), Quaternion.identity);
        fx.transform.localScale = new Vector3(scale, scale, scale);
        fx.trigger.AddCollider(_trigger);

    }

    public void SetForceField(Vector2 worldPos, int scale = 1)
    {
    
        _forceField.transform.position = worldPos;
        _forceField.transform.localScale = new Vector3(scale, scale, scale);

    }

}