using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private Image _highlight;
    [SerializeField] private RectTransform _itemContainer;

    private RectTransform _rectTransform;
    private InventoryController _inventoryController;

    public void Init(InventoryController controller) => _inventoryController = controller;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void PutItem(InventoryItem item)
    {
        item.Rect.SetParent(_itemContainer);
        item.Rect.localPosition = Vector3.zero;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _highlight.enabled = true;
        _inventoryController.OnSlotEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _highlight.enabled = false;
        _inventoryController.OnSlotExit(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _inventoryController.OnSlotClick(this);
    }
}
