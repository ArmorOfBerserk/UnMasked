using System;
using UnityEngine;

public class RaccoltaChiave : MonoBehaviour
{
    [SerializeField] bool trueKey;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gameObject.SetActive(false);
            
            if(trueKey)
            other.GetComponent<PlayerController>().prendoChiave();
        }
        
        
    }
}
