using UnityEngine;

namespace BallShotGame.Events
{
    /// <summary>
    /// 공 드래그 시작 이벤트
    /// 마우스 버튼을 눌러 드래그를 시작할 때 발행
    /// </summary>
    public struct BallDragStartedEvent
    {
        /// <summary>드래그 시작 위치 (월드 좌표)</summary>
        public Vector2 StartPosition;
        
        /// <summary>공의 현재 위치</summary>
        public Vector2 BallPosition;
    }

    /// <summary>
    /// 공 드래그 업데이트 이벤트
    /// 드래그 중 마우스가 움직일 때마다 발행
    /// </summary>
    public struct BallDragUpdatedEvent
    {
        /// <summary>현재 마우스 위치 (월드 좌표)</summary>
        public Vector2 CurrentPosition;
        
        /// <summary>드래그 시작 위치</summary>
        public Vector2 StartPosition;
        
        /// <summary>드래그 벡터 (Start - Current)</summary>
        public Vector2 DragVector => StartPosition - CurrentPosition;
        
        /// <summary>드래그 거리</summary>
        public float DragDistance => DragVector.magnitude;
    }

    /// <summary>
    /// 공 발사 이벤트
    /// 마우스 버튼을 떼어 발사할 때 발행
    /// </summary>
    public struct BallLaunchedEvent
    {
        /// <summary>발사 힘 (반대 방향)</summary>
        public Vector2 Force;
        
        /// <summary>발사 방향 (정규화)</summary>
        public Vector2 Direction;
        
        /// <summary>발사 세기</summary>
        public float Magnitude;
    }

    /// <summary>
    /// 공 정지 이벤트
    /// 공의 속도가 threshold 이하로 떨어졌을 때 발행
    /// </summary>
    public struct BallStoppedEvent
    {
        /// <summary>정지한 위치</summary>
        public Vector2 StopPosition;
        
        /// <summary>정지 시 속도 (0에 가까운 값)</summary>
        public float FinalVelocity;
    }
}
