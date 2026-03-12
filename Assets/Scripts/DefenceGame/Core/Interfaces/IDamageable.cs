using UnityEngine;

namespace DefenceGame.Core
{
    /// <summary>
    /// 데미지를 받을 수 있는 객체의 인터페이스
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float damage);
        void TakeDamage(float damage, Unit attacker);
    }
}
