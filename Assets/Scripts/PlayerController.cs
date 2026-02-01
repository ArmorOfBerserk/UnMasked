using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool hoChiave = false;

    public void prendoChiave()
    {
        hoChiave = true;
    }

    public bool CheckChiave()
    {
        return hoChiave;
    }
}
