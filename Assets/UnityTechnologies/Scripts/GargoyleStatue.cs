using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StealthGame;
using TMPro;

public class GargoyleStatue : MonoBehaviour
{
    [Header("Player Detection")]
    [Tooltip("Distance at which Gargoyle detects the player")]
    public float detectionRange = 10f;
    
    [Tooltip("Gargoyle's field of view (360 = sees all around)")]
    [Range(0f, 360f)]
    public float viewAngle = 120f;
    
    [Tooltip("Use Raycast to check for wall obstructions")]
    public bool checkLineOfSight = true;

    [Header("Ghost Alert System")]
    [Tooltip("Specific ghosts to alert (if empty, finds all Chasers within alertRadius)")]
    public List<Enemies> specificGhostsToAlert = new List<Enemies>();
    
    [Tooltip("Radius to search for ghosts to alert (if no specific ghosts specified)")]
    public float alertRadius = 20f;
    
    [Tooltip("Alert only ghosts of this archetype")]
    public EnemyArchetype targetArchetype = EnemyArchetype. Chaser;
    
    [Tooltip("Cooldown time before alerting again (seconds)")]
    public float alertCooldown = 3f;

    [Header("Visual & Audio")]
    [Tooltip("Gizmo color when detecting player")]
    public Color detectedColor = Color.red;
    
    [Tooltip("Gizmo color when not detecting")]
    public Color normalColor = Color.yellow;
    
    [Tooltip("Sound to play when detecting player")]
    public AudioClip alertSound;
    
    [Tooltip("Effect to spawn when alerting")]
    public GameObject alertEffect;

    [Header("Animation")]
    [Tooltip("Animator for playing alert animation")]
    public Animator animator;
    
    [Tooltip("Trigger parameter name in Animator for alert")]
    public string alertTriggerName = "Alert";

    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool showGizmos = true;
    
    //Sound and text
    public AudioSource alert;
    public bool playerInSight;

    // Private variables
    private Transform player;
    private bool playerDetected = false;
    private float lastAlertTime = -Mathf.Infinity;
    private AudioSource audioSource;
    private List<Enemies> alertedGhosts = new List<Enemies>();

    void Start()
    {
        // Find AudioSource or create new
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && alertSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Find Player
        GameObject playerObj = GameObject. FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Find Ghosts if not specified
        if (specificGhostsToAlert.Count == 0)
        {
            FindNearbyGhosts();
        }
    }

    void Update()
    {
        if (player == null)
        {
            // Try to find Player again
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj. transform;
            }
            return;
        }

        // Detect player
        playerDetected = DetectPlayer();

        // If player detected and cooldown passed
        if (playerInSight)
        {
            alert.Play();
            AlertGhosts();
            lastAlertTime = Time.time;
        }

    }

    bool DetectPlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > detectionRange)
        {
            return false;
        }

        if (viewAngle < 360f)
        {
            Vector3 forward = transform.forward;
            float angleToPlayer = Vector3.Angle(forward, directionToPlayer);
            
            if (angleToPlayer > viewAngle / 2f)
            {
                return false;
            }
        }

        if (checkLineOfSight)
        {
            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 target = player.position + Vector3.up * 1f;
            Vector3 direction = target - origin;

            if (Physics.Raycast(origin, direction. normalized, out RaycastHit hit, distanceToPlayer))
            {
                if (hit.transform != player)
                {
                    return false;
                }
            }
        }

        return true;
    }

    void AlertGhosts()
    {
        
        if (player == null) return;

        Vector3 playerPosition = player.position;
        alertedGhosts.Clear();

        List<Enemies> ghostsToAlert = specificGhostsToAlert. Count > 0 
            ? specificGhostsToAlert 
            : FindNearbyGhosts();

        foreach (Enemies ghost in ghostsToAlert)
        {
            if (ghost != null)
            {
                ghost.ReceivePlayerAlert(playerPosition);
                alertedGhosts.Add(ghost);
            }
        }

        if (audioSource != null && alertSound != null)
        {
            audioSource.PlayOneShot(alertSound);
        }

        if (alertEffect != null)
        {
            GameObject effect = Instantiate(alertEffect, transform. position + Vector3.up * 2f, Quaternion.identity);
            Destroy(effect, 2f);
        }

        if (animator != null && ! string.IsNullOrEmpty(alertTriggerName))
        {
            animator.SetTrigger(alertTriggerName);
        }

        if (showDebugInfo)
        {
            Debug.Log($"[Gargoyle] Detected player and alerted {alertedGhosts.Count} ghost(s)!");
        }
    }

    List<Enemies> FindNearbyGhosts()
    {
        List<Enemies> foundGhosts = new List<Enemies>();
        Enemies[] allEnemies = Object.FindObjectsByType<Enemies>(FindObjectsSortMode.None);

        foreach (Enemies enemy in allEnemies)
        {
            if (enemy == null) continue;

            if (enemy.archetype != targetArchetype) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= alertRadius)
            {
                foundGhosts.Add(enemy);
            }
        }

        if (showDebugInfo)
        {
            Debug.Log($"[Gargoyle] Found {foundGhosts.Count} nearby ghost(s) within {alertRadius}m");
        }

        return foundGhosts;
    }

    public void AddGhostToAlert(Enemies ghost)
    {
        if (ghost != null && !specificGhostsToAlert.Contains(ghost))
        {
            specificGhostsToAlert. Add(ghost);
        }
    }

    public void RemoveGhostFromAlert(Enemies ghost)
    {
        if (specificGhostsToAlert.Contains(ghost))
        {
            specificGhostsToAlert. Remove(ghost);
        }
    }

    public bool IsPlayerDetected()
    {
        return playerDetected;
    }

    void OnDrawGizmos()
    {
        if (! showGizmos) return;

        Gizmos.color = playerDetected ? detectedColor : normalColor;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (viewAngle < 360f)
        {
            Vector3 forward = transform.forward;
            Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * forward * detectionRange;
            Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * forward * detectionRange;

            Gizmos.color = new Color(normalColor.r, normalColor.g, normalColor.b, 0.3f);
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform. position, transform.position + rightBoundary);
        }

        Gizmos.color = Color. cyan;
        Gizmos. DrawWireSphere(transform. position, alertRadius);

        if (player != null && playerDetected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up, player.position + Vector3.up);
        }

        Gizmos.color = Color. magenta;
        foreach (Enemies ghost in specificGhostsToAlert)
        {
            if (ghost != null)
            {
                Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, ghost. transform.position + Vector3.up * 0.5f);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color. yellow;
        Gizmos. DrawWireSphere(transform. position, detectionRange);
    }
}