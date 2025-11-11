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

    private InventoryStack _selectedStack;
    private InventorySlot _selectedSlot;

    private float _doubleClickTimer;
    private bool _onLMBDrag;
    private bool _onRMBDrag;

    private int _stackQuantityBuffer;


    public bool IsStackSelected() => IsStackSelected(_selectedStack);
    public bool IsStackSelected(InventoryStack stack) => stack != null && stack.IsDestroyed == false;
    public bool IsSlotSelected() => IsSlotSelected(_selectedSlot);
    public bool IsSlotSelected(InventorySlot slot) => slot != null;
    public bool IsSlotHasStack() => IsSlotHasStack(_selectedSlot);
    public bool IsSlotHasStack(InventorySlot slot) => slot != null && slot.HasStack();

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
            amount = FillStack(notCompleteStack, amount);

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
        bool has = false;



        return has;
    }

    #region ==== Handlers ====

    public void OnSlotEnter(InventorySlot slot, PointerEventData eventData)
    {
        _selectedSlot = slot;

        if (IsStackSelected())
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

    private void OnDragLMBCancaled()
    {
        _onLMBDrag = false;
        _onLMBDragSelectedSlots.Clear();
    }

    private void OnDragRMBCancaled()
    {
        _onRMBDrag = false;
        _onRMBDragSelectedSlots.Clear();
    }

    private void OnLMBDown()
    {
        _onLMBDrag = true;

        if (IsStackSelected())
        {
            _stackQuantityBuffer = _selectedStack.GetQuantity();

            if (_onLMBDragSelectedSlots.Add(_selectedSlot) == false) return;

        }
    }

    private void OnRMBDown()
    {
        _onRMBDrag = true;

        if (IsStackSelected())
        {
            _stackQuantityBuffer = _selectedStack.GetQuantity();
        }
    }

    private void OnLMBClickUp() // ref
    {
        OnDragLMBCancaled();

        if (IsStackSelected() == false)
        {

            _doubleClickTimer = CLICK_TRESHOLD;
        }


        if (_doubleClickTimer > 0 && IsStackSelected())
        {
            if (_selectedStack.GetQuantity() >= _selectedStack.MaxStack) return;

            if (_stackMap.TryGetValue(_selectedStack.ItemID, out var stacks) == false)
                return;

            for (int i = stacks.Count; i >= 0; i--) // ref
            {
                var stack = stacks.FirstOrDefault(s => s.IsDestroyed == false && s != _selectedStack);

                if (stack == null || stack.IsDestroyed)
                {
                    continue;
                }

                int extra = FillStack(_selectedStack, stack.GetQuantity());
                stack.SetQuantity(extra);

                if (_selectedStack.IsFull()) return;
            }

            return;
        }

        if (IsSlotSelected() &&
            IsStackSelected() == false &&
            IsSlotHasStack())
        {
            SelectStackFromSlot(_selectedSlot);
            return;
        }

        if (IsStackSelected() &&
            IsSlotSelected() &&
            IsSlotHasStack() == false)
        {
            AddStackToSlot(_selectedStack, _selectedSlot);
            _selectedStack = null;

            return;
        }

        if (IsStackSelected() &&
            IsSlotSelected() &&
            IsSlotHasStack())
        {
            var stackInSlot = _selectedSlot.GetStack();

            if (stackInSlot.ItemID == _selectedStack.ItemID &&
                stackInSlot.IsFull() == false)
            {
                int extra = FillStack(stackInSlot, _selectedStack.GetQuantity());
                _selectedStack.SetQuantity(extra);

                return;
            }
            else
            {
                SwapStack(_selectedStack, _selectedSlot);
                return;
            }
        }
    }

    private void OnRMBClickUp() 
    {
        OnDragRMBCancaled();

        if (IsStackSelected() == false &&
            IsSlotSelected() &&
            IsSlotHasStack())
        {
            var stack = _selectedSlot.GetStack();
            var total = _selectedSlot.GetStack().GetQuantity();

            if (total <= 1)
            {
                SelectStackFromSlot(_selectedSlot);
                return;
            }

            var half = total / 2;
            int newStackAmount = (total % 2 == 0) ? half : half + 1;

            var newStack = CreateStack(stack.Item, newStackAmount);
            SelectStack(newStack);

            stack.SetQuantity(half);

            return;
        }

        if (IsStackSelected() &&
            IsSlotSelected() &&
            IsSlotHasStack() == false)
        {
            _selectedSlot.AddStack(CreateStack(_selectedStack.Item));
            _selectedStack.SetQuantity(_selectedStack.GetQuantity() - 1);

            return;
        }

        if (IsStackSelected() &&
            IsSlotSelected() &&
            IsSlotHasStack())
        {
            FillStack(_selectedSlot.GetStack(), 1);
            _selectedStack.SetQuantity(_selectedStack.GetQuantity() - 1);

            return;
        }
    }

    #endregion

    private void DivideEquallyStacks(HashSet<InventorySlot> slots, InventoryStack stack) // ref // error
    {
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

    private void SelectStackFromSlot(InventorySlot slot)
    {
        var stack = slot.GetStack();
        SelectStack(stack);

        slot.RemoveStack();
    }

    private void AddStackToSlot(InventoryStack stack, InventorySlot slot)
    {
        if (_stackMap[stack.ItemID] != null &&
            _stackMap[stack.ItemID].Contains(stack) == false)
        {
            _stackMap[stack.ItemID].Add(stack);
        }
        slot.AddStack(stack);
    }

    private void SwapStack(InventoryStack from, InventorySlot inSlot)
    {
        SelectStackFromSlot(inSlot);
        AddStackToSlot(from, inSlot);
    }



    #region ==== Stack control ====

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stack"></param>
    /// <param name="amount"></param>
    /// <returns>Extra</returns>
    private int FillStack(InventoryStack stack, int amount)
    {
        if (stack == null)
            throw new ArgumentNullException(nameof(stack));
        if (amount <= 0)
            return 0;

        int total = stack.GetQuantity() + amount;
        int clamped = Mathf.Min(total, stack.MaxStack);

        stack.SetQuantity(clamped);

        return Mathf.Max(0, total - stack.MaxStack);
    }

    #endregion

    #region ==== Get stack ====

    private bool TryGetSameNotCompleteStack(string itemID, out InventoryStack stack)
    {
        stack = null;

        if (_stackMap.TryGetValue(itemID, out var stacks))
        {
            stack = stacks.FirstOrDefault(s => s.IsFull() == false);
        }

        return stack != null;
    }

    private bool TryGetFirstEmptySlot(out InventorySlot slot)
    {
        slot = _slots.FirstOrDefault(s => !s.HasStack());

        return slot != null;
    }

    #endregion

    #region ==== Stack factory ====

    private InventoryStack CreateStack(ItemSO item, int amount = 1)
    {
        var newStack = Instantiate(_stackPrefab, _canvas.transform); // ref
        newStack.Init(item);
        newStack.OnEmpty += OnStackEmpty;
        newStack.SetQuantity(amount);

        if (_stackMap.ContainsKey(item.ID) == false)
        {
            _stackMap[item.ID] = new List<InventoryStack>();
        }

        _stacks.Add(newStack);
        _stackMap[item.ID].Add(newStack);

        return newStack;
    }

    private void DestroyStack(InventoryStack stack)
    {
        if (_stackMap.ContainsKey(stack.ItemID) == false)
            throw new ArgumentException(nameof(stack.ItemID));

        stack.Dispose();

        _stacks.Remove(stack);
        _stackMap[stack.ItemID].Remove(stack);

        Destroy(stack.gameObject); // ref
    }


    private void OnStackEmpty(InventoryStack stack)
    {
        DestroyStack(stack);
    }

    #endregion
}
