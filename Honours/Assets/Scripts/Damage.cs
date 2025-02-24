using UnityEngine;

public class Damage : MonoBehaviour
{
    public interface IDamageable
    {
        void Damage(float damage);
    }
}
