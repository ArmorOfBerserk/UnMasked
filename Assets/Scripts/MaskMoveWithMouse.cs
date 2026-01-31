using UnityEngine;
using UnityEngine.InputSystem;

public class MaskMoveWithMouse : MonoBehaviour
{
    public float smooth = 15f;

    void Update()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        Vector3 target = Camera.main.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x + 100, mouseScreenPos.y, -Camera.main.transform.position.z)
        );

        target.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * smooth);
    }

}
