using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    ItemSkill itemSkill;
    GameEnding gameEnding;

    [Header("Score")]
    public int currentScore = 0;
    public int killCount = 0;

    [Header("Text")]
    public TextMeshProUGUI scoreTxt;
    public TextMeshProUGUI healthTxt;
    public TextMeshProUGUI objectiveTxt;
    public TextMeshProUGUI exitTxt;
    public TextMeshProUGUI totalScoreTxt1;
    public TextMeshProUGUI totalScoreTxt2;


    [Header("Player Stat")]
    public int Health = 3;
    

    [Header("Movement")]
    public float turnSpeed = 20f;
    public float walkSpeed = 1f;
    public float runSpeed = 2f;

    [Header("Item")]
    public string currentItem;
    public bool isUsingItem = false;
    public int keyCount = 0;
    public float itemDuration = 0;

    // Start is called before the first frame update
    void Start()
    {
        itemSkill = FindAnyObjectByType<ItemSkill>();
        gameEnding = FindAnyObjectByType<GameEnding>();

    }

    // Update is called once per frame
    void Update()
    {   
        UpdateScore();

        //Calculate item usage duration
        if(isUsingItem == true)
        {
            itemDuration -= Time.deltaTime;
            
            if (itemDuration <= 0)
            {
                itemSkill.DisableItem();
            }
        }
        
        if(Health <= 0)
        {
            gameEnding.m_IsPlayerCaught = true;
        }
    }

    public void UpdateScore()
    {
        scoreTxt.text = $"Score: {currentScore}";
        healthTxt.text = $"Health: {Health}";
        objectiveTxt.text = $"Key: ({keyCount}/4)";

        if(keyCount >= 4)
        {
            exitTxt.gameObject.SetActive(true);
        }
    }
    

    public void UpdateTotalScore()
    {
        totalScoreTxt1.text = $"Coins: {currentScore} \n Kills: {killCount}";
        totalScoreTxt2.text = $"Coins: {currentScore} \n Kills: {killCount}";
    }
 
}
