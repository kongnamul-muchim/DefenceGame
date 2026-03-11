using System.Collections.Generic;
using UnityEngine;
using DefenceGame.Data;

namespace DefenceGame.Core
{
    [System.Serializable]
    public class TowerPrefabMapping
    {
        public int towerId;
        public GameObject prefab;
    }
    
    public class UnitPlacementManager : MonoBehaviour
    {
        public static UnitPlacementManager Instance { get; private set; }
        
        [Header("References")]
        public GameObject defaultUnitPrefab;
        public GridSystem gridSystem;
        
        [Header("Tower Prefabs")]
        // Map tower ID to specific prefab
        public List<TowerPrefabMapping> towerPrefabs = new List<TowerPrefabMapping>();
        
        [Header("Placement Settings")]
        public int maxPlacementAttempts = 50;
        public float placementHeight = 0.5f;
        public Vector2 placementOffset = new Vector2(0.5f, 0.5f);
        
        private Dictionary<Vector3, Unit> placedUnits = new Dictionary<Vector3, Unit>(); // 위치별 유닛 추적
        private float minDistanceBetweenUnits = 1.0f;
        private Dictionary<int, GameObject> towerPrefabDict;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeTowerPrefabDict();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeTowerPrefabDict()
        {
            towerPrefabDict = new Dictionary<int, GameObject>();
            Debug.Log("[UnitPlacement] Initializing Tower Prefab Dictionary...");
            
            foreach (var mapping in towerPrefabs)
            {
                if (mapping.prefab != null)
                {
                    towerPrefabDict[mapping.towerId] = mapping.prefab;
                    Debug.Log($"[UnitPlacement] Registered Tower ID {mapping.towerId} -> {mapping.prefab.name}");
                }
                else
                {
                    Debug.LogWarning($"[UnitPlacement] Tower ID {mapping.towerId} has null prefab!");
                }
            }
            
            Debug.Log($"[UnitPlacement] Total prefabs registered: {towerPrefabDict.Count}");
        }
        
        private void Start()
        {
            if (gridSystem == null)
            {
                gridSystem = GridSystem.Instance;
            }
        }
        
        /// <summary>
        /// Place a unit randomly on the map
        /// </summary>
        public void PlaceUnit(TowerData towerData)
        {
            Vector3? position = FindValidPlacementPosition();
            
            if (position.HasValue)
            {
                SpawnUnit(towerData, position.Value);
            }
            else
            {
                Debug.LogWarning("Could not find valid placement position for unit!");
            }
        }
        
        private Vector3? FindValidPlacementPosition()
        {
            if (gridSystem == null || gridSystem.wallTilemap == null) return null;
            
            for (int i = 0; i < maxPlacementAttempts; i++)
            {
                // Get random cell position within grid bounds
                int randomX = Random.Range(gridSystem.gridOffset.x, gridSystem.gridOffset.x + gridSystem.gridSize.x);
                int randomY = Random.Range(gridSystem.gridOffset.y, gridSystem.gridOffset.y + gridSystem.gridSize.y);
                
                Vector3Int cellPos = new Vector3Int(randomX, randomY, 0);
                
                // Check if wall exists at this position
                if (!gridSystem.wallTilemap.HasTile(cellPos)) continue;
                
                // Get world position (center of tile)
                Vector3 worldPos = gridSystem.wallTilemap.CellToWorld(cellPos);
                worldPos += new Vector3(placementOffset.x, placementOffset.y, placementHeight);
                
                // Check distance from other units
                bool tooClose = false;
                foreach (var kvp in placedUnits)
                {
                    if (Vector3.Distance(worldPos, kvp.Key) < minDistanceBetweenUnits)
                    {
                        tooClose = true;
                        break;
                    }
                }
                
                if (!tooClose)
                {
                    return worldPos;
                }
            }
            
            Debug.LogWarning("Could not find valid wall tile for unit placement after " + maxPlacementAttempts + " attempts");
            return null;
        }
        
        private void SpawnUnit(TowerData towerData, Vector3 position)
        {
            // Get prefab for this tower ID
            GameObject prefabToUse = GetTowerPrefab(towerData.Id);
            
            // If no specific prefab, use default (create generic unit)
            if (prefabToUse == null)
            {
                prefabToUse = defaultUnitPrefab;
            }
            
            if (prefabToUse == null)
            {
                Debug.LogError($"No prefab available for tower ID: {towerData.Id}! Please assign prefabs in UnitPlacementManager inspector.");
                return;
            }
            
            GameObject unit = Instantiate(prefabToUse, position, Quaternion.identity);
            unit.name = $"Unit_{towerData.Name}";
            
            // Initialize unit with tower data
            Unit unitComponent = unit.GetComponent<Unit>();
            if (unitComponent != null)
            {
                unitComponent.Initialize(towerData);
                // 위치 추적 추가
                placedUnits[position] = unitComponent;
            }
            else
            {
                Debug.LogWarning($"Unit component not found on prefab for {towerData.Name}");
            }
            
            Debug.Log($"Placed [{towerData.Grade}] {towerData.Name} (ID: {towerData.Id}) at {position}");
        }
        
        private GameObject GetTowerPrefab(int towerId)
        {
            if (towerPrefabDict == null)
            {
                InitializeTowerPrefabDict();
            }
            
            // ID 1~20을 1~4로 변환 (5단계 등급 시스템 지원)
            // 1,5,9,13,17 → 1 (archer)
            // 2,6,10,14,18 → 2 (wizard)
            // 3,7,11,15,19 → 3 (wizardTower)
            // 4,8,12,16,20 → 4 (Laser)
            int normalizedId = ((towerId - 1) % 4) + 1;
            
            if (towerPrefabDict.ContainsKey(normalizedId))
            {
                GameObject prefab = towerPrefabDict[normalizedId];
                Debug.Log($"[UnitPlacement] Tower ID {towerId} normalized to {normalizedId}, using prefab: {prefab.name}");
                return prefab;
            }
            
            Debug.Log($"[UnitPlacement] No prefab for normalized ID {normalizedId} (original ID {towerId}), using default");
            return defaultUnitPrefab;
        }
        
        /// <summary>
        /// Clear all placed units (e.g., on game restart)
        /// </summary>
        public void ClearAllUnits()
        {
            placedUnits.Clear();
        }
        
        /// <summary>
        /// Get unit at specific position
        /// </summary>
        public Unit GetUnitAtPosition(Vector3 position)
        {
            // 위치 허용 오차 내에서 검색
            float tolerance = 0.5f;
            foreach (var kvp in placedUnits)
            {
                if (Vector3.Distance(kvp.Key, position) < tolerance)
                {
                    return kvp.Value;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Move unit to new position
        /// </summary>
        public bool MoveUnitToPosition(Unit unit, Vector3 newPosition)
        {
            if (unit == null) return false;
            
            // 1. 새 위치가 Wall 타일인지 확인
            if (gridSystem == null) return false;
            Vector3Int cellPos = gridSystem.wallTilemap.WorldToCell(newPosition);
            if (!gridSystem.wallTilemap.HasTile(cellPos)) return false;
            
            // 2. 기존 위치 찾기 및 제거
            Vector3? oldPosition = null;
            foreach (var kvp in placedUnits)
            {
                if (kvp.Value == unit)
                {
                    oldPosition = kvp.Key;
                    break;
                }
            }
            
            if (oldPosition.HasValue)
            {
                placedUnits.Remove(oldPosition.Value);
            }
            
            // 3. 새 위치에 추가
            placedUnits[newPosition] = unit;
            
            // 4. 유닛 위치 업데이트
            unit.transform.position = newPosition;
            
            Debug.Log($"Unit moved from {oldPosition} to {newPosition}");
            return true;
        }
        
        /// <summary>
        /// Remove unit from placement manager
        /// </summary>
        public void RemoveUnit(Unit unit)
        {
            if (unit == null) return;
            
            // placedUnits에서 해당 유닛 찾아 제거
            Vector3? positionToRemove = null;
            foreach (var kvp in placedUnits)
            {
                if (kvp.Value == unit)
                {
                    positionToRemove = kvp.Key;
                    break;
                }
            }
            
            if (positionToRemove.HasValue)
            {
                placedUnits.Remove(positionToRemove.Value);
                Debug.Log($"Unit removed from position: {positionToRemove.Value}");
            }
        }
        
        /// <summary>
        /// Place unit at specific position (for merge)
        /// </summary>
        public void PlaceUnitAtPosition(TowerData towerData, Vector3 position)
        {
            // Get prefab for this tower ID
            GameObject prefabToUse = GetTowerPrefab(towerData.Id);
            
            // If no specific prefab, use default
            if (prefabToUse == null)
            {
                prefabToUse = defaultUnitPrefab;
            }
            
            if (prefabToUse == null)
            {
                Debug.LogError($"No prefab available for tower ID: {towerData.Id}!");
                return;
            }
            
            GameObject unit = Instantiate(prefabToUse, position, Quaternion.identity);
            unit.name = $"Unit_{towerData.Name}";
            
            // Initialize unit with tower data
            Unit unitComponent = unit.GetComponent<Unit>();
            if (unitComponent != null)
            {
                unitComponent.Initialize(towerData);
                placedUnits[position] = unitComponent;
            }
            else
            {
                Debug.LogWarning($"Unit component not found on prefab for {towerData.Name}");
            }
            
            Debug.Log($"Placed [{towerData.Grade}] {towerData.Name} (ID: {towerData.Id}) at {position}");
        }
        
        /// <summary>
        /// Update unit position tracking (called when unit moves)
        /// </summary>
        public void UpdateUnitPosition(Unit unit, Vector3 oldPosition, Vector3 newPosition)
        {
            if (placedUnits.ContainsKey(oldPosition) && placedUnits[oldPosition] == unit)
            {
                placedUnits.Remove(oldPosition);
                placedUnits[newPosition] = unit;
            }
        }
    }
}