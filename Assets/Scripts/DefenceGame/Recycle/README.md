# Recycle - DI & Event System

이 폴더는 DefenceGame의 DI(Dependency Injection)와 이벤트 시스템을 재사용 가능한 형태로 복사한 것입니다.

## 폴더 구조

```
Recycle/
├── Core/
│   ├── Managers/
│   │   └── GameManager.cs          # Facade 패턴 - 시스템 조율
│   ├── Systems/                    # 단일 책임 시스템들
│   │   ├── Economy/
│   │   │   └── GoldManager.cs      # 골드 관리
│   │   ├── Progression/
│   │   │   └── ScoreManager.cs     # 점수/생존시간 관리
│   │   ├── Defense/
│   │   │   └── CastleManager.cs    # 성 체력 관리
│   │   ├── State/
│   │   │   └── GameStateManager.cs # 게임 상태 관리
│   │   └── Spawning/
│   │       └── EnemySpawner.cs     # 적 생성
│   └── Interfaces/
│       ├── IDamageable.cs          # 데미지 인터페이스
│       ├── IInitializable.cs       # 초기화 인터페이스
│       └── IStatProvider.cs        # 스탯 제공 인터페이스
└── UI/
    └── GameUI.cs                   # 이벤트 구독 예시
```

## 핵심 개념

### 1. 싱글톤 패턴 + 이벤트 시스템

각 매니저는 싱글톤으로 구현되어 있으며, 이벤트를 통해 다른 시스템과 통신합니다.

```csharp
// 이벤트 발행 (GoldManager)
public event Action<int> OnGoldChanged;

public void AddGold(int amount)
{
    currentGold += amount;
    OnGoldChanged?.Invoke(currentGold);  // 이벤트 발행
}
```

```csharp
// 이벤트 구독 (GameUI)
private void Start()
{
    GoldManager.Instance.OnGoldChanged += UpdateGold;
}

private void OnDestroy()
{
    GoldManager.Instance.OnGoldChanged -= UpdateGold;  // 구독 해제
}

private void UpdateGold(int gold)
{
    goldText.text = $"Gold: {gold}";
}
```

### 2. Facade 패턴

GameManager는 Facade로 작동하여 복잡한 시스템들을 단순화된 인터페이스로 제공합니다.

```csharp
// 복잡한 내부 로직
GoldManager.Instance?.AddGold(amount);
ScoreManager.Instance?.RecordEnemyDefeated();

// 단순화된 Facade 인터페이스
GameManager.Instance.EnemyDefeated(rewardGold);
```

### 3. SOLID 원칙 적용

- **S (Single Responsibility)**: 각 매니저가 하나의 책임만 가짐
- **O (Open/Closed)**: 새로운 시스템 추가 시 기존 코드 수정 없이 확장 가능
- **I (Interface Segregation)**: 인터페이스를 작게 유지
- **D (Dependency Inversion)**: 구체 클래스 대신 인터페이스/이벤트에 의존

## 사용 예시

### 골드 변경 감지

```csharp
public class ShopUI : MonoBehaviour
{
    private void Start()
    {
        GoldManager.Instance.OnGoldChanged += UpdateGoldDisplay;
    }
    
    public void BuyItem(int cost)
    {
        if (GoldManager.Instance.SpendGold(cost))
        {
            // 구매 성공 - 이벤트로 자동 UI 업데이트
        }
    }
}
```

### 게임 상태 변경

```csharp
public class PauseMenu : MonoBehaviour
{
    private void Start()
    {
        GameStateManager.Instance.OnGameStateChanged += OnStateChanged;
    }
    
    private void OnStateChanged(GameState state)
    {
        if (state == GameState.GameOver)
        {
            ShowGameOverPanel();
        }
    }
}
```

### 성 체력 감소

```csharp
public class Enemy : MonoBehaviour
{
    private void ReachCastle()
    {
        CastleManager.Instance.DamageCastle(1);
    }
}
```

## 네임스페이스

모든 클래스는 `Recycle` 네임스페이스를 사용합니다.

```csharp
using Recycle.Core;
using Recycle.Core.Systems.Economy;
using Recycle.Core.Systems.State;
using Recycle.UI;
```

## 확장 방법

새로운 시스템 추가 시:

1. `Recycle/Core/Systems/` 아래에 새 폴더 생성
2. MonoBehaviour 상속 클래스 작성
3. 싱글톤 패턴 적용
4. 이벤트 정의
5. GameStateManager에서 초기화

```csharp
namespace Recycle.Core.Systems.Custom
{
    public class CustomManager : MonoBehaviour
    {
        public static CustomManager Instance { get; private set; }
        public event Action OnCustomEvent;
        
        private void Awake() { /* 싱글톤 구현 */ }
        
        public void DoSomething()
        {
            OnCustomEvent?.Invoke();
        }
    }
}
```
