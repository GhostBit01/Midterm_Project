using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSkill : MonoBehaviour
{
    GameManager gameManager;
    
    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    public void Crusifix()
    {
        gameManager.currentScore += 50;
    }

    public void DisableItem()
    {
        gameManager.currentItem = "";
        gameManager.itemDuration = 0;
        gameManager.isUsingItem = false;
    }


}
