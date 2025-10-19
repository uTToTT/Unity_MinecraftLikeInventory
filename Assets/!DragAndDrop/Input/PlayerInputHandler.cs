using UnityEngine;

public class PlayerInputHandler : IInputHandler
{
    private PlayerInput _input;

    public PlayerInputHandler()
    {
        _input = new PlayerInput();
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
}
