using UnityEngine;
using UnityEngine.AI;

namespace StealthGame
{
    public enum EnemyArchetype
    {
        Chaser,
        Ambusher,
        Flanker,
        Shy
    }

    public enum EnemyState
    {
        Patrol,
        Chase,
        Flee
    }

    public class Enemies : MonoBehaviour
    {
        [Header("Setup")]
        public EnemyArchetype archetype = EnemyArchetype.Chaser;
        public Transform player;
        public bool playerInSight;
        public Enemies anchor;
        public Transform home;

        [Header("Movement")]
        public float speed = 2.5f;
        public float fleeSpeed = 2.0f;

        [Header("Patrol")]
        public Transform[] waypoints;

        [Header("Random Patrol (No Waypoints)")]
        public bool useRandomPatrolWhenNoWaypoints = true;

        [Min(0.05f)]
        public float randomPatrolArriveDistance = 0.6f;

        [Tooltip("If set, random patrol targets are picked around this transform. Otherwise uses home, then current position.")]
        public Transform randomPatrolCenter;

        [Header("Tuning")]
        public float sightRange = 12f;
        public float ambushDistance = 4f;
        public float flankDistance = 3f;
        public float shyDistance = 8f;

        [Header("Chase Memory")]
        public float chaseMemoryDuration = 5f;
        
        public float hearingRange = 10f;

        [Header("Debug")]
        public bool showDebugInfo = true;

        NavMeshAgent agent;
        //EnemyState state = EnemyState.Patrol;
        int waypointIndex;
        bool frightened;

        Vector3 randomPatrolTarget;
        bool hasRandomPatrolTarget;

        Vector3 lastKnownPlayerPosition;
        // float chaseMemoryTimer;
        bool hasLastKnownPosition;

        float uniqueOffsetAngle;
        Quaternion targetRotation;
        // float speedVariation = 0.1f;
        // float detectionTime;
        // float chaseDelay;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = speed;
                agent.updateRotation = false;
                agent.acceleration = 60f;
                agent.angularSpeed = 360f;
                agent.stoppingDistance = 0f;
                agent.autoBraking = false;
                agent.radius = 0.2f;
                agent.height = 1.0f;
                agent.baseOffset = 0f;
            }
        }

        void Start()
        {

            if (archetype == EnemyArchetype.Flanker && anchor == null)
            {
                FindAnchor();
            }

            uniqueOffsetAngle = Random.Range(0f, 360f);
            //chaseDelay = archetype == EnemyArchetype.Chaser ? 0f : archetype == EnemyArchetype.Ambusher ? 1f : archetype == EnemyArchetype.Flanker ? 2f : 3f;
        }

        void FindAnchor()
        {
            Enemies[] allEnemies = Object.FindObjectsByType<Enemies>(FindObjectsSortMode.None);
            foreach (Enemies enemy in allEnemies)
            {
                if (enemy != null && enemy != this && enemy.archetype == EnemyArchetype.Chaser)
                {
                    anchor = enemy;
                    break;
                }
            }
        }

        void Update()
        {
            if (agent == null)
            {
                return;
            }

            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
            
            if (playerInSight)
            {        
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
                agent.SetDestination(GetChaseTarget());
            }
            else
            {
                player = null;
                Patrol();
            }
            
            
        }

        void Patrol()
        {
            if (waypoints != null && waypoints.Length > 0)
            {
                Transform target = waypoints[waypointIndex];
                if (target != null)
                {
                    SetDestination(target.position);
                    if (!agent.pathPending && agent.remainingDistance <= 0.5f)
                    {
                        waypointIndex = (waypointIndex + 1) % waypoints.Length;
                    }
                }
                return;
            }

            if (useRandomPatrolWhenNoWaypoints)
            {
                if (!hasRandomPatrolTarget || HasArrivedAtRandomTarget() || agent.pathStatus == NavMeshPathStatus.PathInvalid)
                {
                    PickNewRandomPatrolTarget();
                }

                if (hasRandomPatrolTarget)
                {
                    SetDestination(randomPatrolTarget);
                    return;
                }
            }

            if (home != null)
            {
                SetDestination(home.position);
            }
        }

        bool HasArrivedAtRandomTarget()
        {
            if (agent.pathPending)
            {
                return false;
            }
            return agent.remainingDistance <= Mathf.Max(randomPatrolArriveDistance, agent.stoppingDistance + 0.1f);
        }

        void PickNewRandomPatrolTarget()
        {
            hasRandomPatrolTarget = false;

            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            if (triangulation.vertices.Length == 0) return;

            for (int i = 0; i < 10; i++)
            {
                int index = Random.Range(0, triangulation.vertices.Length);
                Vector3 point = triangulation.vertices[index];

                if (Vector3.Distance(transform.position, point) < 15f)
                    continue;

                if (NavMesh.SamplePosition(point, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                {
                    randomPatrolTarget = hit.position;
                    hasRandomPatrolTarget = true;
                    break;
                }
            }
        }

        Vector3 GetChaseTarget()
        {
            Vector3 targetPosition = player != null ? player.position : lastKnownPlayerPosition;
            
            if (player == null && !hasLastKnownPosition)
            {
                return transform.position;
            }

            if (!CanSeePlayer())
            {
                return lastKnownPlayerPosition;
            }

            switch (archetype)
            {
                case EnemyArchetype.Chaser:
                    Vector3 chaserOffset = Quaternion.Euler(0, uniqueOffsetAngle, 0) * Vector3.forward * 0.5f;
                    return targetPosition + chaserOffset;

                case EnemyArchetype.Ambusher:
                    Vector3 ambushDirection = player != null ? player.forward : Vector3.forward;
                    Vector3 ambushVariation = Quaternion.Euler(0, uniqueOffsetAngle * 0.5f, 0) * ambushDirection * ambushDistance;
                    return targetPosition + ambushVariation;

                case EnemyArchetype.Flanker:
                    if (anchor != null && player != null)
                    {
                        Vector3 toAnchor = anchor.transform.position - player.position;
                        Vector3 perpendicular = Vector3.Cross(toAnchor.normalized, Vector3.up).normalized;
                        float flankAngle = uniqueOffsetAngle * Mathf.Deg2Rad;
                        Vector3 flankOffset = perpendicular * flankDistance + Vector3.right * Mathf.Sin(flankAngle) * 0.5f;
                        return player.position + flankOffset;
                    }
                    return targetPosition;

                case EnemyArchetype.Shy:
                    float d = Vector3.Distance(transform.position, targetPosition);
                    if (d <= shyDistance)
                    {
                        return GetFleeTarget();
                    }
                    return targetPosition;

                default:
                    return targetPosition;
            }
        }

        Vector3 GetFleeTarget()
        {
            if (player == null)
            {
                return transform.position;
            }

            Vector3 away = (transform.position - player.position);
            away.y = 0f;
            if (away.sqrMagnitude < 0.001f)
            {
                away = transform.forward;
            }

            Vector3 rawTarget = transform.position + away.normalized * 10f;
            if (NavMesh.SamplePosition(rawTarget, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                return hit.position;
            }
            return transform.position;
        }

        bool CanSeePlayer()
        {
            if (player == null)
            {
                return false;
            }

            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 target = player.position + Vector3.up * 0.5f;
            Vector3 dir = target - origin;

            if (dir.sqrMagnitude > sightRange * sightRange)
            {
                return false;
            }

            Vector3 currentOrigin = origin;
            float remainingDistance = sightRange;
            

                if (Physics.Raycast(currentOrigin, dir.normalized, out RaycastHit hit, remainingDistance))
                {
                    if(playerInSight)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
        }

        public void ReceivePlayerAlert(Vector3 playerPosition)
        {
            lastKnownPlayerPosition = playerPosition;
            hasLastKnownPosition = true;
            playerInSight = true;
            
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"[{archetype}] Received alert from Gargoyle! Player at {playerPosition}");
            }
        }

        void SetDestination(Vector3 destination)
        {
            if (agent.enabled)
            {
                agent.SetDestination(destination);
            }
        }

        public void SetFrightened(bool enabled)
        {
            frightened = enabled;
        }

        public void ResetToStart(Vector3 startPosition)
        {
            transform.position = startPosition;
            frightened = false;
            waypointIndex = 0;
            hasRandomPatrolTarget = false;
            hasLastKnownPosition = false;
            //chaseMemoryTimer = 0f;
            if (agent != null)
            {
                agent.ResetPath();
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, sightRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, hearingRange);

            if (player != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, player.position + Vector3.up * 0.5f);
            }

            if (hasLastKnownPosition)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(lastKnownPlayerPosition, 0.5f);
            }
        }
    }
}