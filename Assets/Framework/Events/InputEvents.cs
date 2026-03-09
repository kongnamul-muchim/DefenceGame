using UnityEngine;

namespace BallShotGame.Events
{
    /// <summary>
    /// 마우스 버튼 누름 이벤트
    /// 마우스 버튼을 눌렀을 때 발행
    /// </summary>
    public struct InputMouseDownEvent
    {
        /// <summary>마우스 위치 (월드 좌표)</summary>
        public Vector2 Position;
        
        /// <summary>마우스 위치 (스크린 좌표)</summary>
        public Vector2 ScreenPosition;
    }

    /// <summary>
    /// 마우스 드래그 이벤트
    /// 마우스를 드래그 중일 때 매 프레임 발행
    /// </summary>
    public struct InputMouseDragEvent
    {
        /// <summary>현재 마우스 위치 (월드 좌표)</summary>
        public Vector2 Position;
        
        /// <summary>마우스 위치 (스크린 좌표)</summary>
        public Vector2 ScreenPosition;
        
        /// <summary>이전 프레임과의 차이</summary>
        public Vector2 Delta;
    }

    /// <summary>
    /// 마우스 버튼 놓음 이벤트
    /// 마우스 버튼을 뗐을 때 발행
    /// </summary>
    public struct InputMouseUpEvent
    {
        /// <summary>마우스 위치 (월드 좌표)</summary>
        public Vector2 Position;
        
        /// <summary>마우스 위치 (스크린 좌표)</summary>
        public Vector2 ScreenPosition;
        
        /// <summary>드래그 지속 시간 (초)</summary>
        public float DragDuration;
    }
}
