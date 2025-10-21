using System;
using UnityEngine;

public interface IInputHandler : IDisposable
{
    event Action LMBClick;
    event Action RMBClick;
    event Action HoldRMB;
    event Action HoldLMB;

    void EnableInput();
    void DisableInput();

    Vector2 GetPointerPosition();
}
