using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GargoylePOV : MonoBehaviour
{
    GargoyleStatue gargoyleStatue;

    void Start()
    {
        gargoyleStatue = GetComponentInParent<GargoyleStatue>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gargoyleStatue.playerInSight = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gargoyleStatue.playerInSight = false;
        }
    }
}
