using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnding : MonoBehaviour
{
    public float fadeDuration = 1f;
    public float displayImageDuration = 1f;
    GameManager gameManager;
    public CanvasGroup exitBackgroundImageCanvasGroup;
    public CanvasGroup caughtBackgroundImageCanvasGroup;
    public bool m_IsPlayerAtExit;
    public bool m_IsPlayerCaught;
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
            EndLevel(exitBackgroundImageCanvasGroup);
        }
        else if (m_IsPlayerCaught)
        {
            gameManager.UpdateTotalScore();
            EndLevel(caughtBackgroundImageCanvasGroup);
        }
    }
    void EndLevel(CanvasGroup imageCanvasGroup)
    {
        m_Timer += Time.deltaTime;
        imageCanvasGroup.alpha = m_Timer / fadeDuration;
    }
}