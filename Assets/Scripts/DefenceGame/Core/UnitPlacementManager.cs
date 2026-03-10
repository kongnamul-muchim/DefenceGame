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
        
        private List<Vector3> placedPositions = new List<Vector3>();
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
            foreach (var mapping in towerPrefabs)
            {
                if (mapping.prefab != null)
                {
                    towerPrefabDict[mapping.towerId] = mapping.prefab;
                }
            }
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
                foreach (Vector3 placedPos in placedPositions)
                {
                    if (Vector3.Distance(worldPos, placedPos) < minDistanceBetweenUnits)
                    {
                        tooClose = true;
                        break;
                    }
                }
                
                if (!tooClose)
                {
                    placedPositions.Add(worldPos);
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
            
            if (towerPrefabDict.ContainsKey(towerId))
            {
                return towerPrefabDict[towerId];
            }
            
            return defaultUnitPrefab;
        }
        
        /// <summary>
        /// Clear all placed units (e.g., on game restart)
        /// </summary>
        public void ClearAllUnits()
        {
            placedPositions.Clear();
        }
    }
}