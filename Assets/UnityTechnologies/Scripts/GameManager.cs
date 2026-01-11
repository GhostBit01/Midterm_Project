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

    [Header("Score")]
    public int CurrentScore = 0;
    public TextMeshProUGUI scoreTxt;
    public TextMeshProUGUI HealthTxt;
    public TextMeshProUGUI ObjectiveTxt;

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

        if(keyCount >= 4)
        {
            itemSkill.UnlockDoor();
        }
        
        if(Health <= 0)
        {
            SceneManager.LoadScene(2);
        }
    }

    public void UpdateScore()
    {
        scoreTxt.text = $" {CurrentScore}";
        HealthTxt.text = $" {Health}";
        ObjectiveTxt.text = $" ({keyCount}/4)";
    }
 
}
