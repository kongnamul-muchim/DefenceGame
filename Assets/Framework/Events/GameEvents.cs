using UnityEngine;

namespace BallShotGame.Events
{
    /// <summary>
    /// 골인 이벤트
    /// 공이 골인지점에 도달했을 때 발행
    /// </summary>
    public struct GoalReachedEvent
    {
        /// <summary>골인 위치</summary>
        public Vector2 GoalPosition;
        
        /// <summary>현재 스테이지 (확장용)</summary>
        public int StageNumber;
    }

    /// <summary>
    /// 리셋 요청 이벤트
    /// 게임을 초기 상태로 되돌릴 때 발행
    /// </summary>
    public struct ResetRequestedEvent
    {
        /// <summary>리셋 이유</summary>
        public ResetReason Reason;
        
        /// <summary>리셋 지연 시간</summary>
        public float Delay;
    }

    /// <summary>
    /// 게임 리셋 완료 이벤트
    /// 모든 초기화가 완료되었을 때 발행
    /// </summary>
    public struct GameResetCompletedEvent
    {
        /// <summary>초기화된 위치</summary>
        public Vector2 ResetPosition;
        
        /// <summary>리셋 시도 횟수</summary>
        public int AttemptCount;
    }

    /// <summary>
    /// 리셋 이유 열거형
    /// </summary>
    public enum ResetReason
    {
        /// <summary>골인 후 자동 리셋</summary>
        GoalReached,
        
        /// <summary>수동 리셋</summary>
        Manual,
        
        /// <summary>화면 밖 이탈</summary>
        OutOfBounds,
        
        /// <summary>게임 재시작</summary>
        GameRestart
    }
}
