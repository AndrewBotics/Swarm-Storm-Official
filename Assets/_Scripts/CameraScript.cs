using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] public static string PlayerMode = "NeuroPlayer";
    public static string CurrentPlayerMode = null;
    private Transform playerTarget;
    private Vector3 offset = new Vector3(0f, 8f, 4f);
    private float smoothSpeed = 5f;

    void Awake()
    {
    }

    void Update()
    {
        if (PlayerMode != CurrentPlayerMode){
            GameObject player = GameObject.FindWithTag(PlayerMode);
            if (player != null){
                playerTarget = player.transform;
            }

            CurrentPlayerMode = PlayerMode;
        }
    }

    void LateUpdate()
    {
        if (playerTarget == null) {
            Debug.LogWarning("No player assigned to camera.");
            return;
        }
        else {
            Vector3 desiredPosition = playerTarget.position+offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed*Time.deltaTime);
        }
    }
}
