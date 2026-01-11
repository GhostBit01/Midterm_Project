using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItem : MonoBehaviour
{
    GameManager gameManager;
    public string itemName = "";
    public float itemTime = 4f;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.PlayAudio("item");
            gameManager.isUsingItem = true;
            gameManager.currentItem = itemName;
            gameManager.itemDuration = itemTime;
            Destroy(gameObject);
        }
    }

}
