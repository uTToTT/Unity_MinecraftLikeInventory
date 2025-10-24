using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : IInputHandler
{
    public event Action LMBClickUp;
    public event Action RMBClickUp;

    public event Action RMBClickDown;
    public event Action LMBClickDown;

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



    public void Dispose()
    {
        UnsubOnEvents();
    }

    #region ==== Handlers ====

    private void OnLMBClickUp(InputAction.CallbackContext context) => LMBClickUp?.Invoke();
    private void OnRMBClickUp(InputAction.CallbackContext context) => RMBClickUp?.Invoke();
    private void OnLMBClickDown(InputAction.CallbackContext context) => LMBClickDown?.Invoke();
    private void OnRMBClickDown(InputAction.CallbackContext context) => RMBClickDown?.Invoke();

    #endregion ===============

    #region ==== Sub ====

    private void SubOnEvents()
    {
        _input.UI.LMBClick.started += OnLMBClickDown;
        _input.UI.LMBClick.canceled += OnLMBClickUp;

        _input.UI.RMBClick.started += OnRMBClickDown;
        _input.UI.RMBClick.canceled += OnRMBClickUp;
    }

    private void UnsubOnEvents()
    {
        _input.UI.LMBClick.started -= OnLMBClickDown;
        _input.UI.LMBClick.canceled -= OnLMBClickUp;

        _input.UI.RMBClick.started -= OnRMBClickDown;
        _input.UI.RMBClick.canceled -= OnRMBClickUp;
    }

    #endregion
}
