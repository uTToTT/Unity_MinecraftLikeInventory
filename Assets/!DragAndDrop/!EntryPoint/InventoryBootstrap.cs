using UnityEngine;

public class InventoryBootstrap : MonoBehaviour
{
    [SerializeField] private InventoryManager _inventoryManager;
    [SerializeField] private InputManager _inputManager;

    private void Awake()
    {
        _inputManager.Init();
        _inventoryManager.Init();
    }
}
