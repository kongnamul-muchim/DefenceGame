namespace DefenceGame.Core
{
    /// <summary>
    /// 데이터로 초기화할 수 있는 객체의 인터페이스
    /// </summary>
    public interface IInitializable<T>
    {
        void Initialize(T data);
    }
}
