using System.Collections;
using System.Collections.Generic;
using StealthGame;
using UnityEngine;

public class PlayerChecker : MonoBehaviour
{
    Enemies enemies;

    void Start()
    {
        enemies = GetComponentInParent<Enemies>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemies.playerInSight = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            enemies.playerInSight = false;
        }
    }
}
