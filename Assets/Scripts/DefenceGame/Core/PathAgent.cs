using System.Collections.Generic;
using UnityEngine;

namespace DefenceGame.Core
{
    public class PathAgent : MonoBehaviour
    {
        [Header("Movement")]
        public float speed = 2f;
        public float stoppingDistance = 0.1f;
        public bool flipSprite = true;
        
        [Header("References")]
        public SpriteRenderer spriteRenderer;
        
        [Header("Path")]
        public bool drawGizmos = true;
        public Color pathColor = Color.yellow;
        
        [Header("No Backtracking")]
        public bool enableVisitedTracking = true;
        
        private List<Vector3> path;
        private int currentWaypointIndex = 0;
        private bool isMoving = false;
        private Vector3 targetPosition;
        private HashSet<GridSystem.Node> visitedNodes = new HashSet<GridSystem.Node>();
        
        public System.Action OnPathComplete;
        public System.Action<Vector3> OnWaypointReached;
        
        public bool IsMoving => isMoving;
        public int CurrentWaypoint => currentWaypointIndex;
        public int TotalWaypoints => path != null ? path.Count : 0;
        
        public void SetPath(List<Vector3> newPath)
        {
            if (newPath == null || newPath.Count == 0)
            {
                Debug.LogWarning("Path is empty!");
                isMoving = false;
                return;
            }
            
            path = newPath;
            currentWaypointIndex = 0;
            isMoving = true;
            
            if (path.Count > 0)
            {
                targetPosition = path[0];
            }
            
            Debug.Log($"Path set with {path.Count} waypoints");
        }
        
        public void SetDestination(Vector3 destination)
        {
            if (Pathfinder.Instance == null)
            {
                Debug.LogError("Pathfinder not found!");
                return;
            }
            
            List<Vector3> newPath = Pathfinder.Instance.FindPath(transform.position, destination, visitedNodes);
            SetPath(newPath);
        }
        
        private void Update()
        {
            if (!isMoving || path == null || path.Count == 0)
                return;
            
            MoveToNextWaypoint();
        }
        
        private void MoveToNextWaypoint()
        {
            if (currentWaypointIndex >= path.Count)
            {
                CompletePath();
                return;
            }
            
            Vector3 target = path[currentWaypointIndex];
            Vector3 direction = (target - transform.position).normalized;
            
            // Flip sprite based on movement direction
            if (flipSprite)
            {
                UpdateSpriteDirection(direction);
            }
            
            // Move towards target
            transform.position += direction * speed * Time.deltaTime;
            
            // Check if reached waypoint
            float distanceToTarget = Vector3.Distance(transform.position, target);
            if (distanceToTarget <= stoppingDistance)
            {
                OnWaypointReached?.Invoke(target);
                
                // Mark this node as visited
                if (enableVisitedTracking)
                {
                    MarkNodeAsVisited(target);
                }
                
                currentWaypointIndex++;
                
                if (currentWaypointIndex >= path.Count)
                {
                    CompletePath();
                }
            }
        }
        
        private void MarkNodeAsVisited(Vector3 position)
        {
            if (GridSystem.Instance == null) return;
            
            GridSystem.Node node = GridSystem.Instance.GetNodeFromWorldPosition(position);
            if (node != null)
            {
                visitedNodes.Add(node);
            }
        }
        
        public void ClearVisitedNodes()
        {
            visitedNodes.Clear();
            Debug.Log("Visited nodes cleared");
        }
        
        private void CompletePath()
        {
            isMoving = false;
            OnPathComplete?.Invoke();
            Debug.Log("Path completed!");
        }
        
        private void UpdateSpriteDirection(Vector3 direction)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null) return;
            }
            
            // Flip based on horizontal movement
            // If moving right (direction.x > 0), flip sprite
            // If moving left (direction.x < 0), keep original
            if (direction.x > 0.01f)
            {
                spriteRenderer.flipX = true;  // Face right
            }
            else if (direction.x < -0.01f)
            {
                spriteRenderer.flipX = false; // Face left (default)
            }
        }
        
        public void Stop()
        {
            isMoving = false;
        }
        
        public void Resume()
        {
            if (path != null && currentWaypointIndex < path.Count)
            {
                isMoving = true;
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!drawGizmos || path == null || path.Count == 0)
                return;
            
            Gizmos.color = pathColor;
            
            // Draw path lines
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(path[i], path[i + 1]);
            }
            
            // Draw waypoints
            Gizmos.color = Color.red;
            foreach (Vector3 waypoint in path)
            {
                Gizmos.DrawWireSphere(waypoint, 0.2f);
            }
            
            // Draw current target
            if (isMoving && currentWaypointIndex < path.Count)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(path[currentWaypointIndex], 0.3f);
            }
            
            // Draw visited nodes
            if (enableVisitedTracking && visitedNodes != null && visitedNodes.Count > 0)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f); // Orange
                foreach (var node in visitedNodes)
                {
                    if (node != null)
                    {
                        Gizmos.DrawWireCube(node.worldPosition, Vector3.one * 0.4f);
                    }
                }
            }
        }
    }
}
