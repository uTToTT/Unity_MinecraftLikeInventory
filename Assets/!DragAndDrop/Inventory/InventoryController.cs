using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private InventoryItem _itemPrefab;
    [SerializeField] private Transform _slotsParent;
    [SerializeField] private Transform _inventoryParent;
    [SerializeField] private Canvas _canvas;

    private readonly List<InventorySlot> _slots = new();
    private readonly List<InventoryItem> _items = new();

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

    public void OnItemClickHandler(InventoryItem item, PointerEventData eventData)
    {
        _isItemSelected = true;

        _selectedItem = item;
        _selectedItem.Rect.SetParent(_inventoryParent);
        _selectedItem.Rect.SetAsLastSibling();
        _selectedSlot.RemoveItem();

        if (!_items.Contains(item))
        {
            _items.Add(item);
        }


        SetItemBlockRaycastState(false);
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

    public void OnSlotClick(InventorySlot slot, PointerEventData eventData)
    {
        if (_isItemSelected)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    PutItemStackToSlot();
                    Debug.Log("Left click");
                    break;

                case PointerEventData.InputButton.Right:
                    Debug.Log("Right click");
                    PutItemToSlot();
                    break;

                default:
                    break;
            }


        }
    }

    #endregion

    private void SetItemBlockRaycastState(bool state)
    {
        foreach (var item in _items)
        {
            item.CanvasGroup.blocksRaycasts = state;
        }
    }

    private void PutItemStackToSlot()
    {
        _selectedSlot.PutItem(_selectedItem);
        _selectedItem.CanvasGroup.blocksRaycasts = true;
        _selectedItem = null;
        _isItemSelected = false;

        SetItemBlockRaycastState(true);
    }

    private void PutItemToSlot()
    {
        if (_selectedSlot.ContainedItem == null)
        {
            var newStack = Instantiate(_itemPrefab);
            newStack.Init(this);
            _selectedSlot.PutItem(newStack);
            newStack.transform.localScale = Vector3.one;
            newStack.SetQuantity(1);


            _items.Add(newStack);
            var resQuantity = _selectedItem.GetQuantity() - 1;

            if (resQuantity <= 0)
            {
                DeleteSelectedStack();
                return;
            }

            _selectedItem.SetQuantity(resQuantity);
        }
        else
        {
            _selectedSlot.ContainedItem.SetQuantity(_selectedSlot.ContainedItem.GetQuantity() + 1);

            var resQuantity = _selectedItem.GetQuantity() - 1;

            if (resQuantity <= 0)
            {
                DeleteSelectedStack();
                return;
            }

            _selectedItem.SetQuantity(resQuantity);
        }
    }

    private void DeleteSelectedStack()
    {
        _items.Remove(_selectedItem);
        Destroy(_selectedItem.gameObject);
        _selectedItem = null;
        _isItemSelected = false;
        SetItemBlockRaycastState(true);
    }
}
