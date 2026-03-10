using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace DefenceGame.Core
{
    public class GridSystem : MonoBehaviour
    {
        public static GridSystem Instance { get; private set; }
        
        [Header("Tilemap References")]
        public Tilemap wallTilemap;
        public Tilemap barrierTilemap;
        public Tilemap groundTilemap;
        
        [Header("Grid Settings")]
        public Vector2Int gridSize = new Vector2Int(28, 14);
        public Vector2Int gridOffset = new Vector2Int(-18, -7);
        
        private Node[,] grid;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeGrid();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeGrid()
        {
            grid = new Node[gridSize.x, gridSize.y];
            
            for (int x = 0; x < gridSize.x; x++)
            {
                for (int y = 0; y < gridSize.y; y++)
                {
                    Vector3Int cellPos = new Vector3Int(gridOffset.x + x, gridOffset.y + y, 0);
                    Vector3 worldPos = groundTilemap.CellToWorld(cellPos) + new Vector3(0.5f, 0.5f, 0);
                    
                    bool isWalkable = !IsWall(cellPos);
                    
                    grid[x, y] = new Node(isWalkable, worldPos, x, y);
                }
            }
            
            Debug.Log($"Grid initialized: {gridSize.x}x{gridSize.y} nodes");
        }
        
        private bool IsWall(Vector3Int cellPos)
        {
            // Check wall tilemap
            if (wallTilemap != null && wallTilemap.HasTile(cellPos))
                return true;
            
            // Check barrier tilemap
            if (barrierTilemap != null && barrierTilemap.HasTile(cellPos))
                return true;
            
            return false;
        }
        
        public Node GetNodeFromWorldPosition(Vector3 worldPosition)
        {
            Vector3Int cellPos = groundTilemap.WorldToCell(worldPosition);
            int x = cellPos.x - gridOffset.x;
            int y = cellPos.y - gridOffset.y;
            
            if (IsInGridBounds(x, y))
            {
                return grid[x, y];
            }
            
            return null;
        }
        
        public Node GetNode(int x, int y)
        {
            if (IsInGridBounds(x, y))
            {
                return grid[x, y];
            }
            return null;
        }
        
        public bool IsInGridBounds(int x, int y)
        {
            return x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y;
        }
        
        public List<Node> GetNeighbors(Node node)
        {
            List<Node> neighbors = new List<Node>();
            
            // 4방향 (상하좌우)
            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { 1, -1, 0, 0 };
            
            for (int i = 0; i < 4; i++)
            {
                int checkX = node.gridX + dx[i];
                int checkY = node.gridY + dy[i];
                
                if (IsInGridBounds(checkX, checkY))
                {
                    Node neighbor = grid[checkX, checkY];
                    if (neighbor.isWalkable)
                    {
                        neighbors.Add(neighbor);
                    }
                }
            }
            
            return neighbors;
        }
        
        public class Node
        {
            public bool isWalkable;
            public Vector3 worldPosition;
            public int gridX;
            public int gridY;
            
            // A* pathfinding variables
            public int gCost;
            public int hCost;
            public Node parent;
            
            public int fCost => gCost + hCost;
            
            public Node(bool walkable, Vector3 worldPos, int gridX, int gridY)
            {
                this.isWalkable = walkable;
                this.worldPosition = worldPos;
                this.gridX = gridX;
                this.gridY = gridY;
            }
        }
    }
}
