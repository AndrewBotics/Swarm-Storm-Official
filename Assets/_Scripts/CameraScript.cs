using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private Transform playerTarget;
    private Vector3 offset = new Vector3(0f, 8f, 4f);
    
    private Vector3 currentVelocity;
    private float smoothTime = 0.15f; 

    public void SetTarget(Transform target)
    {
        playerTarget = target;
    }

    void LateUpdate()
    {
        if (playerTarget == null) return; 
        
        Vector3 desiredPosition = playerTarget.position + offset;
        
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
    }
}