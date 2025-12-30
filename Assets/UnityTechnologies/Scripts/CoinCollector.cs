using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinCollector : MonoBehaviour
{
    PlayerStatus playerStatus;

    // Start is called before the first frame update
    void Start()
    {
        playerStatus = FindAnyObjectByType<PlayerStatus>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerStatus.score += 1;
            playerStatus.UpdateScore();
            Destroy(gameObject);
        }
    }

}
