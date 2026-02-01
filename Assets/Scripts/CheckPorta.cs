using System;
using UnityEngine;

public class CheckPorta : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PlayerController>().CheckChiave())
            {
                
            }
        }
    }
}
