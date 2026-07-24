using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.InputSystem.EnhancedTouch;
using ETouch = UnityEngine.InputSystem.EnhancedTouch;
using System;

public class JoystickHandler : MonoBehaviour
{
    [SerializeField] private Camera MainCamera;
    [SerializeField] private JoystickObject Joystick;
    [SerializeField] protected Image CooldownCircle;

    private Finger JoystickFinger;
    private Vector2 JoystickVector;

    private float MaxKnobMovement;
    private float deadzone;

    private float touchStartTime;
    public Vector2 ReleasedVector { get; private set; }
    public float HoldDuration { get; private set; }

    public event Action<Vector2, float> OnJoystickReleased;

    public static readonly float autoAimTime = 0.15f;

    [HideInInspector] public float cooldownTime = -1f;
    [HideInInspector] public float startCooldownTime = -1f;

    private void Start()
    {
        MaxKnobMovement = Joystick.RectTransform.sizeDelta.x * 0.5f;
        deadzone = MaxKnobMovement * 0.5f;
    }

    private void Update()
    {
        if (cooldownTime>0){
            float TimeRemaining = cooldownTime-(Time.unscaledTime-startCooldownTime);
            if (TimeRemaining<=0) {
                CooldownCircle.fillAmount = 0f;
                cooldownTime = -1f;
                OnEnable();
            }
            else {
                CooldownCircle.fillAmount = TimeRemaining/cooldownTime;
            }
        }
        else {
            CooldownCircle.fillAmount = 0f;
        }
    }

    public void RemoveCooldown()
    {
        cooldownTime = -1f;
        CooldownCircle.fillAmount = 0f;
        OnEnable();
    }

    public void AddCooldown(float time)
    {
        startCooldownTime = Time.unscaledTime;
        cooldownTime = time;
        CooldownCircle.fillAmount = 1f;
        OnDisable();
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        ETouch.Touch.onFingerDown += HandleFingerDown;
        ETouch.Touch.onFingerUp += HandleLoseFinger;
        ETouch.Touch.onFingerMove += HandleFingerMove;
    }

    private void OnDisable()
    {
        ETouch.Touch.onFingerDown -= HandleFingerDown;
        ETouch.Touch.onFingerUp -= HandleLoseFinger;
        ETouch.Touch.onFingerMove -= HandleFingerMove;
        EnhancedTouchSupport.Disable();
    }

    private void HandleFingerMove(Finger MovedFinger)
    {
        if (MovedFinger != JoystickFinger)
        {
            return;
        }

        ETouch.Touch currentTouch = MovedFinger.currentTouch;
        bool converted = RectTransformUtility.ScreenPointToLocalPointInRectangle(Joystick.RectTransform, currentTouch.screenPosition, MainCamera, out Vector2 localPoint);

        if (converted)
        {
            Vector2 knobPosition = localPoint;

            if (knobPosition.magnitude <= deadzone)
            {
                Joystick.Knob.anchoredPosition = Vector2.zero;
                JoystickVector = Vector2.zero;
            }
            else
            {
                if (knobPosition.magnitude > MaxKnobMovement)
                {
                    knobPosition = knobPosition.normalized * MaxKnobMovement;
                }

                Joystick.Knob.anchoredPosition = knobPosition;
                JoystickVector = knobPosition / MaxKnobMovement;
            }
        }
    }

    private void HandleLoseFinger(Finger LostFinger)
    {
        if (LostFinger == JoystickFinger)
        {
            ReleasedVector = JoystickVector;
            HoldDuration = Time.unscaledTime - touchStartTime;

            OnJoystickReleased?.Invoke(ReleasedVector, HoldDuration);

            JoystickFinger = null;
            Joystick.Knob.anchoredPosition = Vector2.zero;
            JoystickVector = Vector2.zero;
        }
    }

    private void HandleFingerDown(Finger TouchedFinger)
    {
        Vector2 joystickScreenPos = RectTransformUtility.WorldToScreenPoint(MainCamera, Joystick.RectTransform.position);
        float distance = Vector2.Distance(TouchedFinger.screenPosition, joystickScreenPos);
        float scaledRadius = GetOnScreenRadius(Joystick.RectTransform);

        if (JoystickFinger != null)
        {
            return;
        }

        if (distance <= scaledRadius)
        {
            JoystickFinger = TouchedFinger;
            JoystickVector = Vector2.zero;

            touchStartTime = Time.unscaledTime;
        }
    }

    private float GetOnScreenRadius(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        Vector2 screenBottomLeft = RectTransformUtility.WorldToScreenPoint(MainCamera, corners[0]);
        Vector2 screenTopRight = RectTransformUtility.WorldToScreenPoint(MainCamera, corners[2]);
        return Vector2.Distance(screenBottomLeft, screenTopRight) / 2f;
    }

    public Vector2 GetJoystickVector(){
        return JoystickVector;
    }

    public Vector3 GetCameraRelativeDirection()
    {
        if (JoystickVector == Vector2.zero) 
            return Vector3.zero;

        Vector3 camForward = MainCamera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = MainCamera.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 relativeDirection = (camForward * JoystickVector.y + camRight * JoystickVector.x).normalized;

        return relativeDirection;
    }
}