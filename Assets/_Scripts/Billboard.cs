using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera MainCamera;

    void Start()
    {
        MainCamera = Constants.MainCamera; 
    }

    void LateUpdate()
    {
        transform.rotation = MainCamera.transform.rotation * Quaternion.Euler(0, 180f, 0);
    }
}