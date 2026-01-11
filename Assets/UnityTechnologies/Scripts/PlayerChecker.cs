using System.Collections;
using System.Collections.Generic;
using StealthGame;
using UnityEngine;

public class PlayerChecker : MonoBehaviour
{
    Enemies enemies;

    void Start()
    {
        enemies = FindAnyObjectByType<Enemies>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemies.playerInSight = true;
            Debug.Log("Player Detected");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemies.playerInSight = false;
            Debug.Log("Player Gone");
        }
    }
}
