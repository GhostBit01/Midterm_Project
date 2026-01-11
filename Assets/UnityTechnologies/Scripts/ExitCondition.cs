using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExitCondition : MonoBehaviour
{
    GameManager gameManager;
    GameEnding gameEnding;
    
    public TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        gameEnding = FindAnyObjectByType<GameEnding>();
    }

    void OnTriggerEnter(Collider other)
    {
        if(gameManager.keyCount == 4)
        {
            gameEnding.m_IsPlayerAtExit = true;
        }
        else
        {
            text.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        text.gameObject.SetActive(false);
    }
}
