using System;
using UnityEngine;

public interface IInputHandler : IDisposable
{
    event Action LMBClickUp;
    event Action RMBClickUp;
    event Action RMBClickDown;
    event Action LMBClickDown;

    void EnableInput();
    void DisableInput();

    Vector2 GetPointerPosition();
}
