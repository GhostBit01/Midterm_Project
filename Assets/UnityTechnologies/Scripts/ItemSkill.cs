using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSkill : MonoBehaviour
{
    PlayerStatus playerStatus;
    
    // Start is called before the first frame update
    void Start()
    {
        playerStatus = FindAnyObjectByType<PlayerStatus>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (playerStatus.isUsingItem && other.CompareTag("Enemy"))
        {
            Debug.Log("Kill!");
            playerStatus.CurrentScore += 50;
            playerStatus.UpdateScore();
            Destroy(other.gameObject);
        }
    }
}
