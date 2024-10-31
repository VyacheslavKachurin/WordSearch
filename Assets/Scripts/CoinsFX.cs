
using UnityEngine;

public class CoinsFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem _coins;
    [SerializeField] private ParticleSystemForceField _forceField;
    [SerializeField] private Collider2D _trigger;

    public static CoinsFX Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void CreateAnim(Vector2 startPos, int amount)
    {
        var fx = Instantiate(_coins, new Vector2(startPos.x, -startPos.y), Quaternion.identity);
        fx.trigger.AddCollider(_trigger);
   
    }

    public void SetForceField(Vector2 worldPos)
    {
        _forceField.transform.position = new Vector2(worldPos.x + _forceField.transform.lossyScale.x / 2, -worldPos.y);
    }

    public void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}