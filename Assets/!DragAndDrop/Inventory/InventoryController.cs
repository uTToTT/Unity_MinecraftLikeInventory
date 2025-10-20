using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryController : MonoBehaviour, IDisposable
{
    [Header("Debug")]
    [SerializeField] private Vector3 _markerNullPosition;
    [SerializeField] private RectTransform _selectedSlotMarker;

    [SerializeField] private InventoryStack _stackPrefab;
    [SerializeField] private Transform _slotsParent;
    [SerializeField] private Transform _inventoryParent;
    [SerializeField] private Canvas _canvas;

    private readonly List<InventorySlot> _slots = new();
    private readonly List<InventoryStack> _stacks = new();
    private readonly Dictionary<string, List<InventorySlot>> _stackMap = new();

    private InventoryStack _selectedStack;
    private InventorySlot _selectedSlot;

    public void Init()
    {
        _slots.AddRange(_slotsParent.GetComponentsInChildren<InventorySlot>());
        InputManager.Instance.Handler.LMBClick += OnClick;

        int i = 1;

        foreach (var slot in _slots)
        {
            Debug.Log(i);
            slot.SlotPointerEnter += OnSlotEnter;
            slot.SlotPointerExit += OnSlotExit;
            slot.SetIndex(i++);
        }
    }

    public void Dispose()
    {
        foreach (var slot in _slots)
        {
            slot.SlotPointerEnter -= OnSlotEnter;
            slot.SlotPointerExit -= OnSlotExit;
        }

        _stacks.Clear();
        _slots.Clear();
    }

    #region ==== Unity API ====

    private void Update()
    {
        if (_selectedStack == null) return;

        var rawPos = InputManager.Instance.Handler.GetPointerPosition();
        var worldPos = InputManager.Instance.PointerService.ScreenToWorld(rawPos);

        _selectedStack.Rect.position = worldPos;
    }

    #endregion

    public bool AddItem(ItemSO item, int amount = 1)
    {
        if (!TryGetFirstEmptySlot(out var emptySlot))
        {
            Debug.Log("All slots are full!");
            return false;
        }

        var newStack = Instantiate(_stackPrefab, _canvas.transform);
        newStack.Init(item);
        newStack.SetQuantity(amount);

        _stacks.Add(newStack);

        emptySlot.AddStack(newStack);
        return true;
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

    public void OnItemClickHandler(InventoryStack item, PointerEventData eventData)
    {
        _selectedStack = item;
        _selectedStack.Rect.SetParent(_inventoryParent);
        _selectedStack.Rect.SetAsLastSibling();
        _selectedSlot.RemoveStack();

        if (!_stacks.Contains(item))
        {
            _stacks.Add(item);
        }
    }

    // Slots

    public void OnSlotEnter(InventorySlot slot, PointerEventData eventData)
    {
        _selectedSlot = slot;
        _selectedSlotMarker.position = _selectedSlot.transform.position;
    }

    public void OnSlotExit(InventorySlot slot, PointerEventData eventData)
    {
        _selectedSlot = null;
        _selectedSlotMarker.position = _markerNullPosition;
    }

    public void OnSlotClick(InventorySlot slot, PointerEventData eventData)
    {
        return;
        if (_selectedStack != null)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    PutItemStackToSlot();
                    break;

                case PointerEventData.InputButton.Right:
                    PutItemToSlot();
                    break;

                default:
                    break;
            }
        }
    }

    #endregion

    private void OnClick()
    {
        if (_selectedSlot != null &&
            _selectedStack == null &&
            _selectedSlot.HasStack())
        {
            _selectedStack = _selectedSlot.GetStack();
            _selectedStack.Rect.SetParent(_inventoryParent);
            _selectedStack.Rect.SetAsLastSibling();

            _selectedSlot.RemoveStack();
            return;
        }

        if (_selectedStack != null &&
            _selectedSlot != null)
        {
            if (_selectedSlot.HasStack())
            {
                var stackInSlot = _selectedSlot.GetStack();
                stackInSlot.SetQuantity(stackInSlot.GetQuantity() + _selectedStack.GetQuantity());
                Destroy(_selectedStack.gameObject);
            }
            else
            {
                _selectedSlot.AddStack(_selectedStack);
            }

            _selectedStack = null;
            return;
        }
    }

    private bool TryGetSameNotCompleteSlot(string itemID, out InventorySlot slot)
    {
        slot = null;

        return false;
    }

    private bool TryGetFirstEmptySlot(out InventorySlot slot)
    {
        slot = _slots.FirstOrDefault(x => !x.HasStack());

        return slot != null;
    }

    private void PutItemStackToSlot()
    {
        _selectedSlot.AddStack(_selectedStack);
        _selectedStack = null;
    }

    private void PutItemToSlot()
    {
        if (_selectedSlot.GetStack() == null)
        {
            var newStack = Instantiate(_stackPrefab);
            _selectedSlot.AddStack(newStack);
            newStack.transform.localScale = Vector3.one;
            newStack.SetQuantity(1);


            _stacks.Add(newStack);
            var resQuantity = _selectedStack.GetQuantity() - 1;

            if (resQuantity <= 0)
            {
                DeleteSelectedStack();
                return;
            }

            _selectedStack.SetQuantity(resQuantity);
        }
        else
        {
            _selectedSlot.GetStack().SetQuantity(_selectedSlot.GetStack().GetQuantity() + 1);

            var resQuantity = _selectedStack.GetQuantity() - 1;

            if (resQuantity <= 0)
            {
                DeleteSelectedStack();
                return;
            }

            _selectedStack.SetQuantity(resQuantity);
        }
    }

    private void DeleteSelectedStack()
    {
        _stacks.Remove(_selectedStack);
        Destroy(_selectedStack.gameObject);
        _selectedStack = null;
    }
}
