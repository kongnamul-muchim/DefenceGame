using UnityEngine;

namespace DefenceGame.Core
{
    /// <summary>
    /// 적의 이동을 담당하는 컴포넌트
    /// 단일 책임: 이동 로직
    /// </summary>
    public class EnemyMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        public bool useRandomMovement = false;
        public float directionChangeInterval = 2f;
        public float directionRandomness = 0.5f;
        public float wallCheckDistance = 0.5f;
        public LayerMask obstacleLayer;
        
        private float baseSpeed;
        private float currentSpeed;
        private Vector3 targetPosition;
        private Vector3 currentDirection;
        private float directionChangeTimer;
        
        private PathAgent pathAgent;
        private SpriteRenderer spriteRenderer;
        
        public float CurrentSpeed => currentSpeed;
        
        private void Awake()
        {
            pathAgent = GetComponent<PathAgent>() ?? gameObject.AddComponent<PathAgent>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        public void Initialize(float speed, Vector3 target, bool randomMovement = false)
        {
            baseSpeed = speed;
            currentSpeed = speed;
            targetPosition = target;
            useRandomMovement = randomMovement;
            
            if (useRandomMovement)
            {
                InitializeRandomMovement();
            }
            else
            {
                FindPathToTarget(targetPosition);
            }
            
            if (pathAgent != null)
            {
                pathAgent.speed = currentSpeed;
            }
        }
        
        public void SetSpeed(float speed)
        {
            currentSpeed = speed;
            if (pathAgent != null)
            {
                pathAgent.speed = currentSpeed;
            }
        }
        
        public void UpdateMovement()
        {
            if (useRandomMovement)
            {
                UpdateRandomMovement();
            }
        }
        
        private void InitializeRandomMovement()
        {
            currentDirection = (targetPosition - transform.position).normalized;
            directionChangeTimer = directionChangeInterval;
        }
        
        private void UpdateRandomMovement()
        {
            CheckAndAvoidObstacles();
            
            directionChangeTimer -= Time.deltaTime;
            if (directionChangeTimer <= 0)
            {
                ChangeDirectionWithRandomness();
                directionChangeTimer = directionChangeInterval;
            }
            
            transform.position += currentDirection * currentSpeed * Time.deltaTime;
            UpdateSpriteDirection(currentDirection);
        }
        
        private void CheckAndAvoidObstacles()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, currentDirection, wallCheckDistance, obstacleLayer);
            
            if (hit.collider != null)
            {
                currentDirection = FindClearDirection();
            }
            
            Vector3 nextPosition = transform.position + currentDirection * wallCheckDistance;
            if (GridSystem.Instance != null)
            {
                GridSystem.Node nextNode = GridSystem.Instance.GetNodeFromWorldPosition(nextPosition);
                if (nextNode == null || !nextNode.isWalkable)
                {
                    currentDirection = FindClearDirection();
                }
            }
        }
        
        private Vector3 FindClearDirection()
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = Random.Range(0f, 360f);
                Vector3 testDirection = Quaternion.Euler(0, 0, angle) * Vector3.right;
                Vector3 nextPos = transform.position + testDirection * wallCheckDistance;
                
                RaycastHit2D hit = Physics2D.Raycast(transform.position, testDirection, wallCheckDistance, obstacleLayer);
                if (hit.collider != null) continue;
                
                if (GridSystem.Instance != null)
                {
                    GridSystem.Node node = GridSystem.Instance.GetNodeFromWorldPosition(nextPos);
                    if (node == null || !node.isWalkable) continue;
                }
                
                return testDirection;
            }
            
            return (targetPosition - transform.position).normalized;
        }
        
        private void ChangeDirectionWithRandomness()
        {
            Vector3 toCastle = (targetPosition - transform.position).normalized;
            float randomAngle = Random.Range(-directionRandomness * 90f, directionRandomness * 90f);
            currentDirection = Quaternion.Euler(0, 0, randomAngle) * toCastle;
        }
        
        private void UpdateSpriteDirection(Vector3 direction)
        {
            if (spriteRenderer == null) return;
            
            if (direction.x > 0.01f)
            {
                spriteRenderer.flipX = true;
            }
            else if (direction.x < -0.01f)
            {
                spriteRenderer.flipX = false;
            }
        }
        
        private void FindPathToTarget(Vector3 target)
        {
            if (pathAgent != null && Pathfinder.Instance != null)
            {
                pathAgent.SetDestination(target);
            }
        }
        
        public float GetDistanceToTarget()
        {
            return Vector3.Distance(transform.position, targetPosition);
        }
    }
}
