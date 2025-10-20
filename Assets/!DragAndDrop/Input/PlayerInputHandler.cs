using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : IInputHandler
{
    public event Action LMBClick;

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

    public void Dispose()
    {
        UnsubOnEvents();
    }

    #region ==== Sub ====

    private void SubOnEvents()
    {
        _input.UI.LMBClick.performed += OnLMBClickPerformed;
    }

    private void UnsubOnEvents()
    {
        _input.UI.LMBClick.performed -= OnLMBClickPerformed;
    }

    #endregion
}
