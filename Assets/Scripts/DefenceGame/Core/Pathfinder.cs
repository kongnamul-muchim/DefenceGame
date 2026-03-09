using System.Collections.Generic;
using UnityEngine;

namespace DefenceGame.Core
{
    public class Pathfinder : MonoBehaviour
    {
        public static Pathfinder Instance { get; private set; }
        
        private GridSystem gridSystem;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
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
                
                foreach (GridSystem.Node neighbor in gridSystem.GetNeighbors(currentNode))
                {
                    if (closedSet.Contains(neighbor))
                        continue;
                    
                    int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, neighbor);
                    
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
    }
}
