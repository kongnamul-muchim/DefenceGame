namespace BallShotGame.Core
{
    /// <summary>
    /// 모든 서비스가 구현해야 하는 마커 인터페이스
    /// DI 컨테이너에서 서비스 등록/조회 시 사용
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// 서비스 초기화
        /// GameService에 등록된 후 호출됨
        /// </summary>
        void Initialize();

        /// <summary>
        /// 서비스 종료 시 리소스 정리
        /// </summary>
        void Dispose();
    }
}
