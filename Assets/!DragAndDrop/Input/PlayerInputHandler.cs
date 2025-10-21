using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : IInputHandler
{
    public event Action LMBClick;
    public event Action RMBClick;
    public event Action HoldRMB;
    public event Action HoldLMB;

    private PlayerInput _input;

    public PlayerInputHandler()
    {
        _input = new PlayerInput();

        SubOnEvents();
    }

    public void EnableInput() => _input.Enable();
    public void DisableInput() => _input.Disable();

    public void SwitchToGameplay() { }

    public void SwitchToUI()
    {
        // disable other maps

        _input.UI.Enable();
    }

    public Vector2 GetPointerPosition() => _input.UI.Pointer.ReadValue<Vector2>();
    public void OnLMBClickPerformed(InputAction.CallbackContext context) => LMBClick?.Invoke();
    public void OnRMBClickPerformed(InputAction.CallbackContext context) => RMBClick?.Invoke();
    public void OnHoldLMBPerformed(InputAction.CallbackContext context) => HoldLMB?.Invoke();
    public void OnHoldRMBPerformed(InputAction.CallbackContext context) => HoldRMB?.Invoke();

    public void Dispose()
    {
        UnsubOnEvents();
    }

    #region ==== Sub ====

    private void SubOnEvents()
    {
        _input.UI.LMBClick.performed += OnLMBClickPerformed;
        _input.UI.RMBClick.performed += OnRMBClickPerformed;
        _input.UI.HoldLMB.performed += OnHoldLMBPerformed;
        _input.UI.HoldRMB.performed += OnHoldRMBPerformed;
    }

    private void UnsubOnEvents()
    {
        _input.UI.LMBClick.performed -= OnLMBClickPerformed;
        _input.UI.RMBClick.performed -= OnRMBClickPerformed;
        _input.UI.HoldLMB.performed -= OnHoldLMBPerformed;
        _input.UI.HoldRMB.performed -= OnHoldRMBPerformed;
    }

    #endregion
}
