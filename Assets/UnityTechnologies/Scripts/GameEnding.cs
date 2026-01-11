using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnding : MonoBehaviour
{
    public float fadeDuration = 1f;
    public float displayImageDuration = 1f;
    GameManager gameManager;
    public CanvasGroup exitBackgroundImageCanvasGroup;
    public AudioSource exitAudio;
    public CanvasGroup caughtBackgroundImageCanvasGroup;
    public AudioSource caughtAudio;
    public GameObject enemy;
    public bool m_IsPlayerAtExit;
    public bool m_IsPlayerCaught;
    
    bool m_HasAudioPlayed;
    float m_Timer;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }


    void Update()
    {
        if (m_IsPlayerAtExit)
        {
            gameManager.UpdateTotalScore();
            EndLevel(exitBackgroundImageCanvasGroup, exitAudio);
        }
        else if (m_IsPlayerCaught)
        {
            gameManager.UpdateTotalScore();
            EndLevel(caughtBackgroundImageCanvasGroup, caughtAudio);
        }
    }
    void EndLevel(CanvasGroup imageCanvasGroup, AudioSource audioSource)
    {
        if (!m_HasAudioPlayed)
        {
            audioSource.Play();
            m_HasAudioPlayed = true;
        }

        m_Timer += Time.deltaTime;
        imageCanvasGroup.alpha = m_Timer / fadeDuration;
        enemy.SetActive(false);
    }
}