using UnityEngine;
using UnityEngine.InputSystem;

public class ConoVisivoFollowCursor : MonoBehaviour
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
    }

    // Update is called once per frame
    public float smooth = 15f;

    void Update()
    {
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();

        Vector3 target = Camera.main.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x, mouseScreenPos.y, -Camera.main.transform.position.z)
        );

        target.z = transform.position.z;
        transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * smooth);
    }

}
