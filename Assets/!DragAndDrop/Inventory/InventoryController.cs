using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private Transform _slotsParent;
    [SerializeField] private Transform _inventoryParent;
    [SerializeField] private Canvas _canvas;
    private readonly List<InventorySlot> _slots = new();


    private bool _isItemSelected;
    private InventoryItem _selectedItem;
    private InventorySlot _selectedSlot;

    public void Init()
    {
        _slots.AddRange(_slotsParent.GetComponentsInChildren<InventorySlot>());

        foreach (var slot in _slots)
        {
            slot.Init(this);
        }
    }

    #region ==== Unity API ====

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        if (!_isItemSelected || _selectedItem == null) return;

        var rawPos = InputManager.Instance.Handler.GetPointerPosition();
        var worldPos = InputManager.Instance.PointerService.ScreenToWorld(rawPos);


        _selectedItem.Rect.position = worldPos;
    }

    #endregion

    public bool AddItem(ItemSO item, int amount = 1)
    {

        return false;
    }

    public void RemoveItem(ItemSO item, int amount = 1)
    {

    }

    public bool HasItem(ItemSO item, int amount = 1)
    {
        return false;
    }

    #region ==== Handlers ====

    // Items

    public void OnItemBeginDragHandler(RectTransform rect, PointerEventData eventData)
    {
        var slot = rect.parent;
        slot.SetAsLastSibling();
    }

    public void OnItemDragHandler(RectTransform rect, PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public void OnItemEndDragHandler(RectTransform rect, PointerEventData eventData)
    {
        MoveToPosition(rect, rect.parent.position);
    }

    public void OnItemClickHandler(InventoryItem item, PointerEventData eventData)
    {
        _isItemSelected = true;

        _selectedItem = item;
        _selectedItem.Rect.SetParent(_inventoryParent);
        _selectedItem.Rect.SetAsLastSibling();
        _selectedItem.CanvasGroup.blocksRaycasts = false;

        if (_isItemSelected)
        {

        }
        else
        {

        }
    }

    // Slots

    public void OnSlotEnter(InventorySlot slot)
    {
        _selectedSlot = slot;
    }

    public void OnSlotExit(InventorySlot slot)
    {
        _selectedSlot = null;
    }

    public void OnSlotClick(InventorySlot slot)
    {
        if (_isItemSelected)
        {
            _selectedSlot.PutItem(_selectedItem);
            _selectedItem.CanvasGroup.blocksRaycasts = true;
            _selectedItem = null;
            _isItemSelected = false;
        }
    }

    #endregion

    private void MoveToPosition(RectTransform rect, Vector3 pos)
    {
        rect.position = pos;
    }
}
