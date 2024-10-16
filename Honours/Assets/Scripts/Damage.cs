using UnityEngine;

public class Damage : MonoBehaviour
{
    public interface IDamageable
    {
        float Health { get; }
        void Damage(float damage);
    }
}
