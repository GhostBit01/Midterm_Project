using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    GameManager gameManager;
    Animator m_Animator;
    Rigidbody m_Rigidbody;
    AudioSource m_AudioSource;
    Vector3 m_Movement;
    Quaternion m_Rotation = Quaternion.identity;

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        m_Animator = GetComponent<Animator>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_AudioSource = GetComponent<AudioSource> ();
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            m_Animator.SetFloat("WalkSpeed", gameManager.runSpeed);
            m_AudioSource.pitch = 1.5f;
        }
        else
        {
            m_Animator.SetFloat("WalkSpeed", gameManager.walkSpeed);
            m_AudioSource.pitch = 1f;
        }

        m_Movement.Set(horizontal, 0f, vertical);
        m_Movement.Normalize();
        bool hasHorizontalInput = !Mathf.Approximately(horizontal, 0f);
        bool hasVerticalInput = !Mathf.Approximately(vertical, 0f);
        bool isWalking = hasHorizontalInput || hasVerticalInput;
        m_Animator.SetBool("IsWalking", isWalking);

        if (isWalking)
        {
            if (!m_AudioSource.isPlaying)
            {
                m_AudioSource.Play();
            }
        }
        else
        {
            m_AudioSource.Stop();
        }

        Vector3 desiredForward = Vector3.RotateTowards
        (transform.forward, m_Movement, gameManager.turnSpeed * Time.deltaTime, 0f);
        m_Rotation = Quaternion.LookRotation(desiredForward);
    }

    void OnAnimatorMove()
    {
        m_Rigidbody.MovePosition(m_Rigidbody.position + m_Movement *
        m_Animator.deltaPosition.magnitude);
        m_Rigidbody.MoveRotation(m_Rotation);
    }
}