using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;    
    public Vector2 deadZone = new Vector2(0.5f, 2.0f);
    public Vector3 offset = new Vector3(0, 0, -10);     // camera offset

    float leftbound = -2.81f;
    float rightbound = 4.24f;
    float upbound = 57.23f; 
    float downbound = -2.25f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 newPos = transform.position;

        // X follows normally
        newPos.x = target.position.x;

        // Y follows only if target is outside the dead zone
        float yDiff = target.position.y - transform.position.y;

        if (Mathf.Abs(yDiff) > deadZone.y)
        {
            newPos.y = target.position.y - Mathf.Sign(yDiff) * deadZone.y;
        }

        // limit camera to map bounds
        if (newPos.x < leftbound) { newPos.x = leftbound; }
        if (newPos.x > rightbound) { newPos.x = rightbound; }
        if (newPos.y > upbound) { newPos.y = upbound; }
        if (newPos.y < downbound) { newPos.y = downbound; }

        transform.position = new Vector3(newPos.x, newPos.y, offset.z);
    }
}
