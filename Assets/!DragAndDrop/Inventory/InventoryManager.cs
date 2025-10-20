using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private InventoryController _controller;
    [SerializeField] private ItemSO _coal;
    [SerializeField] private ItemSO _iron;

    public void Init()
    {
        _controller.Init();
    }

    public void AddCoal() => AddItem(_coal);
    public void AddIron() => AddItem(_iron);

    private void AddItem(ItemSO item) => _controller.AddItem(item);
}
