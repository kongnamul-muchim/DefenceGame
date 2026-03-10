using System;
using System.Collections.Generic;

namespace BallShotGame.Core
{
    /// <summary>
    /// 제네릭 이벤트 버스 (Pub/Sub 패턴)
    /// 타입 안전한 이벤트 시스템
    /// </summary>
    public class EventBus
    {
        private static EventBus _instance;
        private readonly Dictionary<Type, object> _handlers;

        /// <summary>
        /// 싱글톤 인스턴스
        /// </summary>
        public static EventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EventBus();
                }
                return _instance;
            }
        }

        /// <summary>
        /// private 생성자 (싱글톤)
        /// </summary>
        private EventBus()
        {
            _handlers = new Dictionary<Type, object>();
        }

        /// <summary>
        /// 이벤트 구독
        /// </summary>
        /// <typeparam name="T">이벤트 타입</typeparam>
        /// <param name="handler">이벤트 핸들러</param>
        public void Subscribe<T>(Action<T> handler)
        {
            Type eventType = typeof(T);
            
            if (!_handlers.ContainsKey(eventType))
            {
                _handlers[eventType] = new List<Action<T>>();
            }

            var handlers = (List<Action<T>>)_handlers[eventType];
            handlers.Add(handler);
        }

        /// <summary>
        /// 이벤트 구독 해제
        /// </summary>
        /// <typeparam name="T">이벤트 타입</typeparam>
        /// <param name="handler">이벤트 핸들러</param>
        public void Unsubscribe<T>(Action<T> handler)
        {
            Type eventType = typeof(T);
            
            if (_handlers.TryGetValue(eventType, out object handlersObj))
            {
                var handlers = (List<Action<T>>)handlersObj;
                handlers.Remove(handler);
                
                // 핸들러가 모두 제거되면 Dictionary에서도 제거
                if (handlers.Count == 0)
                {
                    _handlers.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// 이벤트 발행
        /// </summary>
        /// <typeparam name="T">이벤트 타입</typeparam>
        /// <param name="eventData">이벤트 데이터</param>
        public void Publish<T>(T eventData)
        {
            Type eventType = typeof(T);
            
            if (_handlers.TryGetValue(eventType, out object handlersObj))
            {
                var handlers = (List<Action<T>>)handlersObj;
                
                // 리스트 복사본으로 순회 (구독 해제 중에도 안전)
                var handlersCopy = new List<Action<T>>(handlers);
                
                foreach (var handler in handlersCopy)
                {
                    try
                    {
                        handler?.Invoke(eventData);
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Error handling event {eventType.Name}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 모든 구독 초기화
        /// </summary>
        public void Clear()
        {
            _handlers.Clear();
            UnityEngine.Debug.Log("EventBus cleared");
        }

        /// <summary>
        /// 특정 이벤트 타입의 구독자 수 확인
        /// </summary>
        /// <typeparam name="T">이벤트 타입</typeparam>
        /// <returns>구독자 수</returns>
        public int GetSubscriberCount<T>()
        {
            Type eventType = typeof(T);
            
            if (_handlers.TryGetValue(eventType, out object handlersObj))
            {
                return ((List<Action<T>>)handlersObj).Count;
            }
            
            return 0;
        }
    }
}
