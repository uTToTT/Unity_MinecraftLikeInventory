using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    public static InputManager Instance {  get; private set; }

    public IInputHandler Handler { get; private set; }
    public PointerService PointerService { get; private set; }

    public void Init()
    {
        Instance = this;
        Handler = new PlayerInputHandler();
        PointerService = new PointerService(_camera);

        Handler.EnableInput();
    }
}
