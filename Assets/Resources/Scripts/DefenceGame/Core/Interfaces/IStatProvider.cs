namespace DefenceGame.Core
{
    /// <summary>
    /// 스탯을 제공하는 객체의 인터페이스
    /// </summary>
    public interface IStatProvider
    {
        float GetAttackPower();
        float GetAttackSpeed();
        float GetRange();
    }
}
