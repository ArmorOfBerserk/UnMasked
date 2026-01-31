using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMask : MonoBehaviour
{
    [SerializeField] private int timeout = 10;
    [SerializeField] private GameObject mask;
    private PlayerInput _playerInput;
    private bool maskActive = false;
    

    void Start()
    {
        mask.SetActive(false);
    }

    private InputAction toggleMask;

    void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        toggleMask = _playerInput.actions["Active/DeactiveMask"];
    }

    void OnEnable()
    {
        toggleMask.started += OnToggleMask;
    }

    void OnDisable()
    {
        toggleMask.started -= OnToggleMask;
    }

    void OnToggleMask(InputAction.CallbackContext ctx)
    {
        if (maskActive) return;
        StartCoroutine(ShowMask());
    }
    
    private IEnumerator ShowMask()
    {
        maskActive = true;
        mask.SetActive(true);

        yield return new WaitForSeconds(timeout);

        mask.SetActive(false);
        maskActive = false;
    }

}
