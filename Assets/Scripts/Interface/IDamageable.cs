using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage, int count = 1);    
    void TakeDirectDamage(int damage, int count = 1);
}
