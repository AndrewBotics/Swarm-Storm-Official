using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Transform playerTarget;
    private Vector3 offset = new Vector3(0f, 8f, 4f);
    private float smoothSpeed = 5f;

    public void SetTarget(Transform target)
    {
        playerTarget = target;
    }

    void LateUpdate()
    {
        if (playerTarget == null) 
        {
            return; 
        }
        
        Vector3 desiredPosition = playerTarget.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}