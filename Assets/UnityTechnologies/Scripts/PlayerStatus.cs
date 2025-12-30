using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    [Header("Score")]
    public int CurrentScore = 0;
    public TextMeshProUGUI scoreTxt;

    [Header("Item")]
    [SerializeField] public bool isUsingItem = false;
    [SerializeField] public float itemTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        //Calculate item usage duration
        if(isUsingItem == true)
        {
            itemTime -= Time.deltaTime;
            
            if (itemTime <= 0)
            {
                isUsingItem = false;
                itemTime = 0;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isUsingItem && other.CompareTag("Enemy"))
        {
            Debug.Log("Kill!");
            CurrentScore += 50;
            Destroy(other);
        }
    }

    public void UpdateScore()
    {
        scoreTxt.text = "Score: " + CurrentScore;
    }

    
}
