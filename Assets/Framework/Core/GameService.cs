using System;
using System.Collections.Generic;

namespace BallShotGame.Core
{
    /// <summary>
    /// DI 컨테이너 (Service Locator 패턴)
    /// 모든 서비스의 등록과 조회를 담당
    /// </summary>
    public class GameService
    {
        private static GameService _instance;
        private readonly Dictionary<Type, IService> _services;

        /// <summary>
        /// 싱글톤 인스턴스
        /// </summary>
        public static GameService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameService();
                }
                return _instance;
            }
        }

        /// <summary>
        /// private 생성자 (싱글톤)
        /// </summary>
        private GameService()
        {
            _services = new Dictionary<Type, IService>();
        }

        /// <summary>
        /// 서비스 등록
        /// </summary>
        /// <typeparam name="T">서비스 인터페이스 타입</typeparam>
        /// <param name="service">서비스 인스턴스</param>
        public void Register<T>(T service) where T : IService
        {
            Type type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                UnityEngine.Debug.LogWarning($"Service {type.Name} is already registered. Overwriting...");
                _services[type].Dispose();
            }

            _services[type] = service;
            service.Initialize();
            
            UnityEngine.Debug.Log($"Service registered: {type.Name}");
        }

        /// <summary>
        /// 서비스 조회
        /// </summary>
        /// <typeparam name="T">서비스 인터페이스 타입</typeparam>
        /// <returns>서비스 인스턴스</returns>
        public T Get<T>() where T : IService
        {
            Type type = typeof(T);
            
            if (_services.TryGetValue(type, out IService service))
            {
                return (T)service;
            }

            UnityEngine.Debug.LogError($"Service {type.Name} is not registered!");
            return default;
        }

        /// <summary>
        /// 서비스가 등록되어 있는지 확인
        /// </summary>
        /// <typeparam name="T">서비스 인터페이스 타입</typeparam>
        /// <returns>등록 여부</returns>
        public bool IsRegistered<T>() where T : IService
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 모든 서비스 종료 및 리소스 정리
        /// </summary>
        public void DisposeAll()
        {
            foreach (var service in _services.Values)
            {
                service.Dispose();
            }
            _services.Clear();
            UnityEngine.Debug.Log("All services disposed");
        }
    }
}
