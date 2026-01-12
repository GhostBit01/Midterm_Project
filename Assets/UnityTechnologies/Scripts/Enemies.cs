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
        Search,
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
        EnemyState state = EnemyState.Patrol;
        int waypointIndex;
        bool frightened;

        Vector3 randomPatrolTarget;
        bool hasRandomPatrolTarget;

        Vector3 lastKnownPlayerPosition;
        float chaseMemoryTimer;
        bool hasLastKnownPosition;

        float uniqueOffsetAngle;
        Quaternion targetRotation;

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

            if (hasLastKnownPosition)
            {
                chaseMemoryTimer -= Time.deltaTime;
                if (chaseMemoryTimer <= 0)
                {
                    hasLastKnownPosition = false;
                    playerInSight = false;
                }
            }

            if (CanSeePlayer())
            {
                playerInSight = true;
                lastKnownPlayerPosition = player.position;
                chaseMemoryTimer = chaseMemoryDuration;
                hasLastKnownPosition = true;
            }
            else if (playerInSight)
            {
                playerInSight = false;
            }

            if (frightened)
            {
                state = EnemyState.Flee;
            }
            else if (playerInSight)
            {
                state = EnemyState.Chase;
            }
            else if (hasLastKnownPosition)
            {
                state = EnemyState.Search;
            }
            else
            {
                state = EnemyState.Patrol;
            }

            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }

            switch (state)
            {
                case EnemyState.Patrol:
                    Patrol();
                    break;
                case EnemyState.Chase:
                    agent.SetDestination(GetChaseTarget());
                    break;
                case EnemyState.Search:
                    Search();
                    break;
                case EnemyState.Flee:
                    SetDestination(GetFleeTarget());
                    break;
            }

            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name} | State: {state} | playerInSight: {playerInSight} | hasLastKnown: {hasLastKnownPosition}");
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

        void Search()
        {
            if (hasLastKnownPosition)
            {
                agent.SetDestination(lastKnownPlayerPosition);
            }
            else
            {
                Patrol();
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

            if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, sightRange))
            {
                if (hit.transform == player || hit.transform.IsChildOf(player))
                {
                    return true;
                }
                else if (hit.transform.CompareTag("Furniture"))
                {
                    Vector3 newOrigin = hit.point + dir.normalized * 0.1f;
                    float remainingDistance = sightRange - hit.distance;
                    if (Physics.Raycast(newOrigin, dir.normalized, out RaycastHit hit2, remainingDistance))
                    {
                        if (hit2.transform == player || hit2.transform.IsChildOf(player))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void ReceivePlayerAlert(Vector3 playerPosition)
        {
            lastKnownPlayerPosition = playerPosition;
            hasLastKnownPosition = true;
            chaseMemoryTimer = chaseMemoryDuration;
            state = EnemyState.Search;
            
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
            state = EnemyState.Patrol;
            waypointIndex = 0;
            hasRandomPatrolTarget = false;
            hasLastKnownPosition = false;
            chaseMemoryTimer = 0f;
            playerInSight = false;
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