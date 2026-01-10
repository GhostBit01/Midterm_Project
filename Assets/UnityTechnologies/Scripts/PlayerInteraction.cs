using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    GameManager gameManager;
    ItemSkill itemSkill;
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        itemSkill = FindAnyObjectByType<ItemSkill>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Key"))
        {
            gameManager.keyCount++;
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Enemy"))
        {
            if(gameManager.isUsingItem && gameManager.currentItem.ToLower() == "crucifix")
            {
                itemSkill.Crusifix();
                Destroy(other.gameObject);
            }
            else
            {
                gameManager.Health--;
            }
        }
    }
}
