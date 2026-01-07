using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSkill : MonoBehaviour
{
    GameManager gameManager;
    public GameObject lockDoor;
    
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    public void Crusifix()
    {
        gameManager.CurrentScore += 50;
    }

    public void DisableItem()
    {
        gameManager.currentItem = "";
        gameManager.itemDuration = 0;
        gameManager.isUsingItem = false;
    }

    public void UnlockDoor(){
        Destroy(lockDoor);
    }
}
