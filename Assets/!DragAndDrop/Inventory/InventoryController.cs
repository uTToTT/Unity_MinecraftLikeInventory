using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

using Mathf = UnityEngine.Mathf;

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

    private void OnDragLMB() => Debug.Log("Drag LMB");
    private void OnDragRMB() => Debug.Log("Drag RMB");

    public void Init()
    {
        _slots.AddRange(_slotsParent.GetComponentsInChildren<InventorySlot>());
        InputManager.Instance.Handler.LMBClick += OnLMBClick;
        InputManager.Instance.Handler.RMBClick += OnRMBClick;
        InputManager.Instance.Handler.HoldLMB += OnDragLMB;
        InputManager.Instance.Handler.HoldRMB += OnDragRMB;

        int i = 1;

        foreach (var slot in _slots)
        {
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
        if (!TryGetSameNotCompleteSlot(item.ID, out var notCompleteSlot))
        {
            Debug.Log("Not found same slot.");
        }

        if (!TryGetFirstEmptySlot(out var emptySlot))
        {
            Debug.Log("All slots are full!");
            return false;
        }


        if (notCompleteSlot != null)
        {
            amount = FillStack(item, amount, notCompleteSlot.GetStack());

            if (amount <= 0)
            {
                return true;
            }
        }

        var newStack = CreateStack(item, amount);

        emptySlot.AddStack(newStack);
        _stackMap[item.ID].Add(emptySlot);
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

    #endregion

    private void OnLMBClick()
    {
        if (_selectedSlot != null &&
            _selectedStack == null &&
            _selectedSlot.HasStack())
        {
            SelectStackFromSlot(_selectedSlot.GetStack(), _selectedSlot);
            return;
        }

        if (_selectedStack != null &&
            _selectedSlot != null)
        {

            if (_selectedSlot.HasStack())
            {
                var stackInSlot = _selectedSlot.GetStack();

                if (stackInSlot.ItemID == _selectedStack.ItemID &&
                    stackInSlot.IsFull == false)
                {
                    FillStack(_selectedStack, stackInSlot);
                }
                else
                {
                    SwapStack(_selectedStack, stackInSlot, _selectedSlot);
                }
            }
            else
            {
                AddStackToSlot(_selectedStack, _selectedSlot);
                _selectedStack = null;
            }

            return;
        }
    }

    private void OnRMBClick()
    {
        if (_selectedSlot != null &&
            _selectedSlot.HasStack() &&
            _selectedStack == null)
        {
            var stackInSlot = _selectedSlot.GetStack();
            var stackInSlotQuanity = stackInSlot.GetQuantity();

            if (stackInSlotQuanity <= 1)
            {
                SelectStackFromSlot(stackInSlot, _selectedSlot);
                return;
            }

            bool isPairAmount = stackInSlotQuanity % 2 == 0;
            var halfAmountInStack = stackInSlotQuanity / 2;

            int stackInSlotNewAmount = halfAmountInStack;
            int newStackNewAmount = halfAmountInStack;

            if (!isPairAmount)
            {
                newStackNewAmount += 1;
            }

            var newStack = CreateStack(stackInSlot.Item, newStackNewAmount);
            _selectedStack = newStack;

            stackInSlot.SetQuantity(stackInSlotNewAmount);

            return;
        }

        if (_selectedSlot != null &&
            _selectedStack != null)
        {
            if (_selectedSlot.HasStack())
            {
                var stackInSlot = _selectedSlot.GetStack();
                FillStack(_selectedStack, stackInSlot, 1);
            }
            else
            {
                var newStack = CreateStack(_selectedStack.Item);
                _selectedSlot.AddStack(newStack);
                var selectedStackAmount = _selectedStack.GetQuantity();
                if (selectedStackAmount > 1)
                {
                    _selectedStack.SetQuantity(selectedStackAmount - 1);
                }
                else
                {
                    Destroy(_selectedStack.gameObject);
                }
            }
        }
    }

    private void SelectStackFromSlot(InventoryStack stack, InventorySlot slot)
    {
        _selectedStack = stack;
        _selectedStack.Rect.SetParent(_inventoryParent);
        _selectedStack.Rect.SetAsLastSibling();

        _stackMap[_selectedStack.ItemID].Remove(slot);

        slot.RemoveStack();
    }

    private void AddStackToSlot(InventoryStack stack, InventorySlot slot)
    {
        slot.AddStack(stack);
        _stackMap[stack.ItemID].Add(slot);
    }

    private void SwapStack(InventoryStack from, InventoryStack to, InventorySlot inSlot)
    {
        InventoryStack tmp = from;
        SelectStackFromSlot(to, inSlot);
        AddStackToSlot(tmp, inSlot);
    }

    private void FillStack(InventoryStack from, InventoryStack to, int fillAmount = -1)
    {
        if (from.ItemID != to.ItemID)
        {
            Debug.LogWarning("Different items.");
            return;
        }

        var amountFrom = from.GetQuantity();
        var amountTo = to.GetQuantity();
        var amountAll = amountFrom + amountTo;

        if (fillAmount < 0)
        {
            if (amountAll <= to.MaxStack)
            {
                to.SetQuantity(amountAll);
                Destroy(from.gameObject);
            }
            else
            {
                to.SetQuantity(to.MaxStack);
                from.SetQuantity(amountAll - to.MaxStack);
            }
        }
        else
        {
            if (fillAmount >= amountFrom)
            {
                var fill = Mathf.Min(amountFrom, fillAmount);

                to.SetQuantity(amountTo + fill);
                Destroy(from.gameObject);
            }
            else
            {
                to.SetQuantity(amountTo + fillAmount);

                amountFrom = amountFrom - fillAmount;
                from.SetQuantity(amountFrom);
            }
        }
    }

    private int FillStack(ItemSO item, int amountFrom, InventoryStack to) // ref
    {
        if (item.ID != to.ItemID)
        {
            Debug.LogWarning("Different items.");
            return -1;
        }

        var amountTo = to.GetQuantity();
        var amountAll = amountFrom + amountTo;

        if (amountAll <= to.MaxStack)
        {
            to.SetQuantity(amountAll);
            return 0;
        }
        else
        {
            to.SetQuantity(to.MaxStack);
            return amountAll - to.MaxStack;
        }
    }

    private bool TryGetSameNotCompleteSlot(string itemID, out InventorySlot slot)
    {
        slot = null;

        if (_stackMap.TryGetValue(itemID, out var slots))
        {
            slot = slots.FirstOrDefault(s => s.GetStack().IsFull == false);
            return true;
        }

        return false;
    }

    private bool TryGetFirstEmptySlot(out InventorySlot slot)
    {
        slot = _slots.FirstOrDefault(s => !s.HasStack());

        return slot != null;
    }

    private InventoryStack CreateStack(ItemSO item, int amount = 1)
    {
        var newStack = Instantiate(_stackPrefab, _canvas.transform);
        newStack.Init(item);
        newStack.SetQuantity(amount);

        _stacks.Add(newStack);

        if (!_stackMap.ContainsKey(item.ID))
        {
            _stackMap[item.ID] = new List<InventorySlot>();
        }

        return newStack;
    }
}
