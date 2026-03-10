using System.Collections.Generic;
using UnityEngine;

namespace DefenceGame.Core
{
    public class Pathfinder : MonoBehaviour
    {
        public static Pathfinder Instance { get; private set; }
        
        [Header("Pathfinding Settings")]
        public bool useRandomPath = true;
        [Range(0f, 1f)]
        public float randomness = 0.3f;
        
        [Header("Waypoint Settings")]
        public bool useRandomWaypoints = true;
        public int waypointCount = 5;
        public float minWaypointDistance = 5f;
        
        private GridSystem gridSystem;
        private System.Random random;
        private List<Vector3> waypoints;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                random = new System.Random();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            gridSystem = GridSystem.Instance;
            if (gridSystem == null)
            {
                Debug.LogError("GridSystem not found!");
            }
        }
        
        public List<Vector3> FindPath(Vector3 startPos, Vector3 targetPos)
        {
            if (gridSystem == null)
            {
                gridSystem = GridSystem.Instance;
            }
            
            // Generate waypoints if needed
            if (useRandomWaypoints && (waypoints == null || waypoints.Count == 0))
            {
                GenerateRandomWaypoints();
            }
            
            List<Vector3> finalPath = new List<Vector3>();
            Vector3 currentPos = startPos;
            
            // If using waypoints, pick one randomly
            if (useRandomWaypoints && waypoints != null && waypoints.Count > 0)
            {
                Vector3 randomWaypoint = waypoints[Random.Range(0, waypoints.Count)];
                
                // Path: Start → Waypoint
                List<Vector3> pathToWaypoint = FindPathInternal(currentPos, randomWaypoint);
                if (pathToWaypoint.Count > 0)
                {
                    finalPath.AddRange(pathToWaypoint);
                    currentPos = randomWaypoint;
                }
            }
            
            // Path: CurrentPos (or Start) → Target
            List<Vector3> pathToTarget = FindPathInternal(currentPos, targetPos);
            if (pathToTarget.Count > 0)
            {
                finalPath.AddRange(pathToTarget);
            }
            
            return finalPath;
        }
        
        private List<Vector3> FindPathInternal(Vector3 startPos, Vector3 targetPos)
        {
            if (gridSystem == null)
            {
                gridSystem = GridSystem.Instance;
            }
            
            GridSystem.Node startNode = gridSystem.GetNodeFromWorldPosition(startPos);
            GridSystem.Node targetNode = gridSystem.GetNodeFromWorldPosition(targetPos);
            
            if (startNode == null || targetNode == null)
            {
                Debug.LogWarning("Start or target node is outside grid bounds");
                return new List<Vector3>();
            }
            
            if (!startNode.isWalkable || !targetNode.isWalkable)
            {
                Debug.LogWarning("Start or target node is not walkable");
                return new List<Vector3>();
            }
            
            List<GridSystem.Node> openSet = new List<GridSystem.Node>();
            HashSet<GridSystem.Node> closedSet = new HashSet<GridSystem.Node>();
            
            openSet.Add(startNode);
            
            while (openSet.Count > 0)
            {
                GridSystem.Node currentNode = openSet[0];
                
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || 
                        (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                    {
                        currentNode = openSet[i];
                    }
                }
                
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                
                if (currentNode == targetNode)
                {
                    return RetracePath(startNode, targetNode);
                }
                
                foreach (GridSystem.Node neighbor in GetRandomizedNeighbors(currentNode))
                {
                    if (closedSet.Contains(neighbor))
                        continue;
                    
                    int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                    
                    // Add randomness to cost
                    if (useRandomPath)
                    {
                        int randomCost = Mathf.RoundToInt(Random.Range(0f, randomness * 10f));
                        newCostToNeighbor += randomCost;
                    }
                    
                    if (newCostToNeighbor < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newCostToNeighbor;
                        neighbor.hCost = GetDistance(neighbor, targetNode);
                        neighbor.parent = currentNode;
                        
                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }
            
            Debug.LogWarning("Path not found!");
            return new List<Vector3>();
        }
        
        private List<Vector3> RetracePath(GridSystem.Node startNode, GridSystem.Node endNode)
        {
            List<Vector3> path = new List<Vector3>();
            GridSystem.Node currentNode = endNode;
            
            while (currentNode != startNode)
            {
                path.Add(currentNode.worldPosition);
                currentNode = currentNode.parent;
            }
            
            path.Reverse();
            return path;
        }
        
        private int GetDistance(GridSystem.Node nodeA, GridSystem.Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
            
            // 4-directional movement cost
            return dstX + dstY;
        }
        
        private List<GridSystem.Node> GetRandomizedNeighbors(GridSystem.Node node)
        {
            List<GridSystem.Node> neighbors = gridSystem.GetNeighbors(node);
            
            if (!useRandomPath || neighbors.Count <= 1)
                return neighbors;
            
            // Shuffle neighbors using Fisher-Yates algorithm
            for (int i = neighbors.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                GridSystem.Node temp = neighbors[i];
                neighbors[i] = neighbors[j];
                neighbors[j] = temp;
            }
            
            return neighbors;
        }
        
        private void GenerateRandomWaypoints()
        {
            waypoints = new List<Vector3>();
            
            if (gridSystem == null)
            {
                gridSystem = GridSystem.Instance;
            }
            
            if (gridSystem == null) return;
            
            // Try to generate waypoints
            int attempts = 0;
            int maxAttempts = waypointCount * 10;
            
            while (waypoints.Count < waypointCount && attempts < maxAttempts)
            {
                attempts++;
                
                // Pick random position in grid
                int randomX = Random.Range(0, gridSystem.gridSize.x);
                int randomY = Random.Range(0, gridSystem.gridSize.y);
                
                GridSystem.Node node = gridSystem.GetNode(randomX, randomY);
                if (node != null && node.isWalkable)
                {
                    Vector3 waypointPos = node.worldPosition;
                    
                    // Check distance from existing waypoints
                    bool tooClose = false;
                    foreach (Vector3 existing in waypoints)
                    {
                        if (Vector3.Distance(waypointPos, existing) < minWaypointDistance)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    
                    if (!tooClose)
                    {
                        waypoints.Add(waypointPos);
                    }
                }
            }
            
            Debug.Log($"Generated {waypoints.Count} random waypoints");
        }
    }
}
