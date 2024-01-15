using UnityEngine;
using System;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput Instance;

    public DebugControls _debug;

    public float InputMagnitude { get; private set; }
    public Vector2 InputDir { get; set; }
    public bool IsRunning { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsJumping { get; private set; }
    public bool Interact { get; private set; }
    public bool EnterVehicle { get; private set; }
    public bool IsHoldingInventoryKey { get; private set; }
    public bool TappedInventoryKey { get; private set; }
    public float ScrollWheelDelta { get; private set; }
    public bool IsAiming { get; private set; }
    public bool IsShooting_HOLD { get; private set; }
    public bool IsShooting_PRESS { get; private set; }

    public bool EquipSideArm { get; private set; }
    public bool EquipLongArm { get; private set; }
    public bool HolsterAllWeapons { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        InputDir = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        InputMagnitude = InputDir.normalized.magnitude;
        IsRunning = Input.GetKey(KeyCode.LeftShift);

        ScrollWheelDelta = Input.mouseScrollDelta.y;

        if (Input.GetKeyDown(KeyCode.LeftControl))
            IsCrouching = !IsCrouching;

        Interact = Input.GetKeyDown(KeyCode.E);

        IsJumping = Input.GetAxis("Jump") > 0;

        IsHoldingInventoryKey = Input.GetKey(KeyCode.Tab);
        TappedInventoryKey = Input.GetKeyDown(KeyCode.Tab);

        _debug.UpdateDebugControls();

        //Weapon Controls
        IsAiming = Input.GetMouseButton(1) || _debug.debug_Aim;
        IsShooting_HOLD = Input.GetMouseButton(0) || _debug.debug_Shoot_hold;
        IsShooting_PRESS = Input.GetMouseButtonDown(0) || _debug.debug_Shoot_press;
        EquipLongArm = Input.GetKeyDown(KeyCode.Alpha2);
        EquipSideArm = Input.GetKeyDown(KeyCode.Alpha1);
        HolsterAllWeapons = Input.GetKeyDown(KeyCode.Alpha0);
    }

    public float GetMouseX(float mouseSensitivity)
    {
        if (Input.GetJoystickNames().Length > 0)
        {
            return Input.GetAxisRaw("RightStickHorizontal") * mouseSensitivity;
        }
        
        return Input.GetAxis("Mouse X") * mouseSensitivity;
    }
    public float GetMouseY(float mouseSensitivity)
    {
        if (Input.GetJoystickNames().Length > 0)
        {
            return Input.GetAxisRaw("RightStickVertical") * mouseSensitivity;
        }

        return Input.GetAxis("Mouse Y") * mouseSensitivity;
    }
}

[System.Serializable]
public struct DebugControls
{
    public bool debug_Aim;
    public bool debug_Shoot_hold;
    public bool debug_Shoot_press;

    public void UpdateDebugControls()
    {
        if (debug_Shoot_press)
            debug_Shoot_press = false;
    }
}