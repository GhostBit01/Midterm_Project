using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    GameManager gameManager;
    public AudioSource coinSfx;
    public AudioSource killSfx;
    public AudioSource itemSfx;

    int coin;
    int key;
    int kill;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();

        key = gameManager.keyCount;
        coin = gameManager.currentScore;
        kill = gameManager.killCount;
    }

    // Update is called once per frame
    void Update()
    {
        if (coin < gameManager.currentScore)
        {
            coinSfx.Play();
            coin = gameManager.currentScore;
        }

        if (kill < gameManager.killCount)
        {
            killSfx.Play();
            kill = gameManager.killCount;
        }
    }
}
