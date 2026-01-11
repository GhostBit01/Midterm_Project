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
        public float sightRange = 15f;
        public float ambushDistance = 4f;
        public float flankDistance = 3f;
        public float shyDistance = 8f;

        [Header("Chase Memory")]
        public float chaseMemoryDuration = 5f;
        
        public float hearingRange = 15f;

        [Header("Debug")]
        public bool showDebugInfo = true;

        NavMeshAgent agent;
        EnemyState state = EnemyState.Patrol;
        int waypointIndex;
        bool frightened;

        Vector3 randomPatrolTarget;
        bool hasRandomPatrolTarget;

        Vector3 lastKnownPlayerPosition;
        float chaseMemoryTimer;
        bool hasLastKnownPosition;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.speed = speed;
                agent.updateRotation = true;
                agent.acceleration = 120f;
                agent.angularSpeed = 720f;
                agent.stoppingDistance = 0f;
                agent.autoBraking = false;
                agent.radius = 0.2f;
                agent.height = 1.0f;
                agent.baseOffset = 0.1f;
            }
        }

        void Start()
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
            }

            if (archetype == EnemyArchetype.Flanker && anchor == null)
            {
                FindAnchor();
            }
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

            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
            }

            bool canSee = CanSeePlayer();
            bool canHear = CanHearPlayer();
            bool playerDetected = canSee || canHear;

            if (playerDetected && player != null)
            {
                lastKnownPlayerPosition = player.position;
                chaseMemoryTimer = chaseMemoryDuration;
                hasLastKnownPosition = true;
            }
            else if (hasLastKnownPosition)
            {
                chaseMemoryTimer -= Time.deltaTime;
                if (chaseMemoryTimer <= 0f)
                {
                    hasLastKnownPosition = false;
                }
            }

            if (frightened)
            {
                state = EnemyState.Flee;
            }
            else if (playerDetected || hasLastKnownPosition)
            {
                state = EnemyState.Chase;
            }
            else
            {
                state = EnemyState.Patrol;
            }

            agent.speed = frightened ? fleeSpeed : speed;

            if (agent.autoBraking != (state != EnemyState.Chase))
            {
                agent.autoBraking = (state != EnemyState.Chase);
            }

            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name} | State: {state} | Archetype: {archetype} | CanSee: {canSee} | CanHear: {canHear} | Memory: {hasLastKnownPosition}");
            }

            if (state == EnemyState.Chase && player != null)
            {
                agent.SetDestination(player.position);
                return;
            }

            if (state == EnemyState.Flee)
            {
                SetDestination(GetFleeTarget());
                return;
            }

            Patrol();
        }

        bool CanHearPlayer()
        {
            if (player == null)
            {
                return false;
            }

            float distance = Vector3.Distance(transform.position, player.position);
            return distance <= hearingRange;
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

            if (!CanSeePlayer() && !CanHearPlayer())
            {
                return lastKnownPlayerPosition;
            }

            switch (archetype)
            {
                case EnemyArchetype.Chaser:
                    return targetPosition;

                case EnemyArchetype.Ambusher:
                    return targetPosition + (player != null ? player.forward : Vector3.forward) * ambushDistance;

                case EnemyArchetype.Flanker:
                    if (anchor != null && player != null)
                    {
                        Vector3 toAnchor = anchor.transform.position - player.position;
                        Vector3 perpendicular = Vector3.Cross(toAnchor.normalized, Vector3.up).normalized;
                        return player.position + perpendicular * flankDistance;
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

            if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, sightRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform == player || hit.transform.IsChildOf(player))
                {
                    return true;
                }
            }

            return dir.sqrMagnitude <= sightRange * sightRange;
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
            chaseMemoryTimer = 0f;
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