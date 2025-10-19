using UnityEngine;

public interface IInputHandler 
{
    void EnableInput();
    void DisableInput();

    Vector2 GetPointerPosition();
}
