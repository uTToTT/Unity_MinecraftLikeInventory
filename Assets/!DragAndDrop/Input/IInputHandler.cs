using System;
using UnityEngine;

public interface IInputHandler : IDisposable
{
    event Action LMBClick;

    void EnableInput();
    void DisableInput();

    Vector2 GetPointerPosition();
}
