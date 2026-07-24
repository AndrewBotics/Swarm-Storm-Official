using UnityEngine;

[RequireComponent (typeof(RectTransform))]
[DisallowMultipleComponent]

public class JoystickObject : MonoBehaviour
{
    [HideInInspector]
    public RectTransform RectTransform;
    public RectTransform Knob;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }
}