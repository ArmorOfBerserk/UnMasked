using System;
using UnityEngine;

public class RaccoltaChiave : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            this.gameObject.SetActive(false);
            
            other.GetComponent<PlayerController>().prendoChiave();
        }
        
        
    }
}
