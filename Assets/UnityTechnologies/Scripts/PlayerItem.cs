using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItem : MonoBehaviour
{
    PlayerStatus playerStatus;
    public float itemTime = 4f;

    void Start()
    {
        playerStatus = FindAnyObjectByType<PlayerStatus>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerStatus.isUsingItem = true;
            playerStatus.itemTime = itemTime;
            Destroy(gameObject);
        }
    }

}
