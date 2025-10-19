using UnityEngine;

public class PointerService
{
    private readonly Camera _camera;

    public PointerService(Camera camera) => _camera = camera;

    public Vector2 ScreenToWorld(Vector2 screenPos)
    {
        var worldPos = _camera.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;
        return worldPos;
    }
}
