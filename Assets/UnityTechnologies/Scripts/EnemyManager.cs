using UnityEngine;
using System.Collections.Generic;

namespace StealthGame
{
    public class EnemyManager : MonoBehaviour
    {
        [Header("Enemy References")]
        public List<Enemies> allEnemies = new List<Enemies>();

        [Header("Spawn Points")]
        public Transform[] spawnPoints;

        [Header("Game Settings")]
        public float frightenedDuration = 10f;

        public AudioClip frightenedSound;

        private AudioSource audioSource;
        private bool isGlobalFrightened = false;
        private float frightenedTimer = 0f;

        public static EnemyManager Instance { get; private set; }

        void Awake()
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

        void Start()
        {
            audioSource = GetComponent<AudioSource>();

            if (allEnemies.Count != 0)
            {
                return;
            }
            allEnemies.AddRange(Object.FindObjectsByType<Enemies>(FindObjectsSortMode.None));

            AssignArchetypes();
        }

        void AssignArchetypes()
        {
            for (int i = 0; i < allEnemies.Count; i++)
            {
                if (allEnemies[i] != null)
                {
                    EnemyArchetype assignedArchetype = (EnemyArchetype)(i % 4);
                    allEnemies[i].archetype = assignedArchetype;
                }
            }
        }

        void Update()
        {
            if (isGlobalFrightened)
            {
                frightenedTimer -= Time.deltaTime;
                if (frightenedTimer <= 0)
                {
                    EndFrightenedMode();
                }
            }
        }

        public void ActivateFrightenedMode()
        {
            isGlobalFrightened = true;
            frightenedTimer = frightenedDuration;

            foreach (var enemy in allEnemies)
            {
                if (enemy != null)
                {
                    enemy.SetFrightened(true);
                }
            }

            // เล่นเสียง
            if (audioSource != null && frightenedSound != null)
            {
                audioSource.PlayOneShot(frightenedSound);
            }
        }

        void EndFrightenedMode()
        {
            isGlobalFrightened = false;
            foreach (var enemy in allEnemies)
            {
                if (enemy != null)
                {
                    enemy.SetFrightened(false);
                }
            }
        }

        public void ResetAllEnemies()
        {
            for (int i = 0; i < allEnemies.Count; i++)
            {
                if (allEnemies[i] != null && spawnPoints != null && i < spawnPoints.Length)
                {
                    allEnemies[i].ResetToStart(spawnPoints[i].position);
                }
            }
        }

        public void ResetEnemy(Enemies enemy)
        {
            int index = allEnemies.IndexOf(enemy);
            if (index >= 0 && spawnPoints != null && index < spawnPoints.Length)
            {
                enemy.ResetToStart(spawnPoints[index].position);
            }
        }

        public bool IsFrightenedModeActive()
        {
            return isGlobalFrightened;
        }

        public float GetFrightenedTimeRemaining()
        {
            return frightenedTimer;
        }
    }
}
