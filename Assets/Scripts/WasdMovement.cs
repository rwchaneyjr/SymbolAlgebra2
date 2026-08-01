using UnityEngine;

/// <summary>
/// Simple WASD movement using Transform.Translate.
/// Attach to any GameObject you want to move.
/// </summary>
public class WasdMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private void Update()
    {
        Vector3 input = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) input.z += 1f;
        if (Input.GetKey(KeyCode.S)) input.z -= 1f;
        if (Input.GetKey(KeyCode.A)) input.x -= 1f;
        if (Input.GetKey(KeyCode.D)) input.x += 1f;

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        transform.Translate(input * speed * Time.deltaTime, Space.World);
    }
}
