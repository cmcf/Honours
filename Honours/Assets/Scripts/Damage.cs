using UnityEngine;

public class Damage : MonoBehaviour
{
    public interface IDamageable
    {
        void Damage(int damage);
    }
}
