using System;
using UnityEngine;

public class EventMessageManager : MonoBehaviour
{

    public static event Action OnResetTimer;
    public static event Action OnStartEquipMask;
    public static event Action OnLockPlayerMovement;

    public static event Action OnStartCameraScroll;

    public static void StartCameraScrolling()
    {
        OnStartCameraScroll?.Invoke();
    }


    public static void ResetTimerMask()
    {
        OnResetTimer?.Invoke();
    }

    public static void StartEquipMask()
    {
        OnStartEquipMask?.Invoke();
    }

    public static void StopUsingMask()
    {
        OnLockPlayerMovement?.Invoke();
    }

  

}
