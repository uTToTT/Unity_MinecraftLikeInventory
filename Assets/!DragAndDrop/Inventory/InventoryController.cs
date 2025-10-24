using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

using Mathf = UnityEngine.Mathf;


// TODO - object pool
public class InventoryController : MonoBehaviour, IDisposable
{
    private const float CLICK_TRESHOLD = 0.2f;

    [SerializeField] private InventoryStack _stackPrefab;
    [SerializeField] private Transform _slotsParent;
    [SerializeField] private Transform _inventoryParent;
    [SerializeField] private Canvas _canvas;

    private readonly List<InventorySlot> _slots = new();
    private readonly List<InventoryStack> _stacks = new();
    private readonly Dictionary<string, List<InventoryStack>> _stackMap = new();

    private readonly HashSet<InventorySlot> _onLMBDragSelectedSlots = new();
    private readonly HashSet<InventorySlot> _onRMBDragSelectedSlots = new();
    private readonly HashSet<InventorySlot> _emptyStacksBuffer = new();

    private InventoryStack _selectedStack;
    private InventorySlot _selectedSlot;

    private float _doubleClickTimer;
    private bool _onLMBDrag;
    private bool _onRMBDrag;

    private int _stackQuantityBuffer;

    private void OnDragLMBCancaled()
    {
        Debug.Log("Drag LMB cancaled");
        _onLMBDrag = false;
        _onLMBDragSelectedSlots.Clear();
    }

    private void OnDragRMBCancaled()
    {
        Debug.Log("Drag RMB cancaled");
        _onRMBDrag = false;
        _onRMBDragSelectedSlots.Clear();
    }

    private void OnLMBDown()
    {
        Debug.Log("Drag LMB perf");
        _onLMBDrag = true;

        if (_selectedStack != null &&
            _selectedStack.IsDestroyed == false)
        {
            _stackQuantityBuffer = _selectedStack.GetQuantity();

            if (_onLMBDragSelectedSlots.Add(_selectedSlot) == false) return;

        }
    }

    private void OnRMBDown()
    {
        Debug.Log("Drag RMB perf");
        _onRMBDrag = true;

        if (_selectedStack != null &&
            _selectedStack.IsDestroyed == false)
        {
            _stackQuantityBuffer = _selectedStack.GetQuantity();
        }
    }

    public void Init()
    {
        _slots.AddRange(_slotsParent.GetComponentsInChildren<InventorySlot>());
        InputManager.Instance.Handler.LMBClickUp += OnLMBClickUp;
        InputManager.Instance.Handler.RMBClickUp += OnRMBClickUp;
        InputManager.Instance.Handler.LMBClickDown += OnLMBDown;
        InputManager.Instance.Handler.RMBClickDown += OnRMBDown;

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
        if (_doubleClickTimer > 0)
        {
            _doubleClickTimer -= Time.deltaTime;
        }

        if (_selectedStack == null) return;

        var rawPos = InputManager.Instance.Handler.GetPointerPosition();
        var worldPos = InputManager.Instance.PointerService.ScreenToWorld(rawPos);
        _selectedStack.Rect.position = worldPos;
    }

    #endregion

    public bool AddItem(ItemSO item, int amount = 1)
    {
        if (!TryGetSameNotCompleteStack(item.ID, out var notCompleteStack))
        {
            Debug.Log("Not found same slot.");
        }

        if (!TryGetFirstEmptySlot(out var emptySlot))
        {
            Debug.Log("All slots are full!");
            return false;
        }


        if (notCompleteStack != null)
        {
            amount = FillStack(item, amount, notCompleteStack);

            if (amount <= 0)
            {
                return true;
            }
        }

        var newStack = CreateStack(item, amount);

        emptySlot.AddStack(newStack);
        _stackMap[item.ID].Add(newStack);
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

        if (_selectedStack != null &&
            _selectedStack.IsDestroyed == false)
        {
            var stackInSlot = slot.GetStack();
            var stackInSlotQuantity = stackInSlot?.GetQuantity();

            if (_onLMBDrag)
            {
                if (stackInSlot == null ||
                    stackInSlot.IsDestroyed)
                {
                    if (_onLMBDragSelectedSlots.Add(slot) == false) return;

                    DivideEquallyStacks(_onLMBDragSelectedSlots, _selectedStack);
                }

                return;
            }

            if (_onRMBDrag)
            {
                if (_onRMBDragSelectedSlots.Add(slot) == false) return;


                return;
            }
        }
    }

    private void OnSlotExit(InventorySlot slot, PointerEventData eventData)
    {
        _selectedSlot = null;
    }

    #endregion

    private void OnLMBClickUp()
    {
        OnDragLMBCancaled();

        if (_selectedStack == null)
        {
            _doubleClickTimer = CLICK_TRESHOLD;
        }


        if (_doubleClickTimer > 0 &&
            _selectedStack != null)
        {
            var selectedStackQuantity = _selectedStack.GetQuantity();

            if (selectedStackQuantity >= _selectedStack.MaxStack) return;


            if (_stackMap.TryGetValue(_selectedStack.ItemID, out var stacks) == false)
                return;

            var stacksCount = stacks.Count;

            for (int i = 0; i < stacksCount; i++) // ref
            {
                var stack = stacks.FirstOrDefault(s => s.IsDestroyed == false && s != _selectedStack);

                FillStack(stack, _selectedStack);

                if (_selectedStack.IsFull()) return;
            }

            return;
        }

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
                    stackInSlot.IsFull() == false)
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

    private void OnRMBClickUp()
    {
        OnDragRMBCancaled();

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
                    DestroyStack(_selectedStack);
                }
            }
        }
    }

    private void DivideEquallyStacks(HashSet<InventorySlot> slots, InventoryStack stack) // ref
    {
        //if (slots.Count <= 1) return;

        var startStackQuantity = _stackQuantityBuffer;

        var stacksCount = slots.Count;
        var equalQuantity = (int)(startStackQuantity / stacksCount);
        var k = startStackQuantity % stacksCount;

        Debug.Log($"Equal: {equalQuantity}");
        foreach (var slot in slots)
        {
            var stackInSlot = slot.GetStack();

            if (stackInSlot == null ||
                stackInSlot.IsDestroyed)
            {
                var newStack = CreateStack(stack.Item);
                AddStackToSlot(newStack, slot);
                stackInSlot = newStack;
            }

            stackInSlot.SetQuantity(equalQuantity);
        }

        stack.SetQuantity(k);
    }

    private void SelectStack(InventoryStack stack)
    {
        _selectedStack = stack;
        _selectedStack.Rect.SetParent(_inventoryParent);
        _selectedStack.Rect.SetAsLastSibling();

        _stackMap[_selectedStack.ItemID].Remove(stack);
    }

    private void SelectStackFromSlot(InventoryStack stack, InventorySlot slot)
    {
        SelectStack(stack);

        slot.RemoveStack();
    }

    private void AddStackToSlot(InventoryStack stack, InventorySlot slot)
    {
        _stackMap[stack.ItemID].Add(stack);
        slot.AddStack(stack);
    }

    private void SwapStack(InventoryStack from, InventoryStack to, InventorySlot inSlot)
    {
        InventoryStack tmp = from;
        SelectStackFromSlot(to, inSlot);
        AddStackToSlot(tmp, inSlot);
    }

    private void FillStack(InventoryStack from, InventoryStack to, int fillAmount = -1)
    {
        if (from == null || from.IsDestroyed ||
            to == null || to.IsDestroyed)
        {
            Debug.LogWarning("Stack already destroyed.");
            return;
        }

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
                DestroyStack(from);
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
                DestroyStack(from);
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

    private bool TryGetSameNotCompleteStack(string itemID, out InventoryStack stack)
    {
        stack = null;

        if (_stackMap.TryGetValue(itemID, out var stacks))
        {
            stack = stacks.FirstOrDefault(s => s.IsFull() == false);
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
        newStack.OnEmpty += OnStackEmpty;
        newStack.SetQuantity(amount);

        _stacks.Add(newStack);

        if (!_stackMap.ContainsKey(item.ID))
        {
            _stackMap[item.ID] = new List<InventoryStack>();
        }

        _stackMap[item.ID].Add(newStack);

        return newStack;
    }

    private void DestroyStack(InventoryStack stack)
    {
        stack.Dispose();
        _stackMap[stack.ItemID].Remove(stack);
        Destroy(stack.gameObject);
    }


    private void OnStackEmpty(InventoryStack stack)
    {
        DestroyStack(stack);
    }
}
