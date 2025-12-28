using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryStack : MonoBehaviour, IDisposable
{
    public event Action<InventoryStack> OnEmpty;

    [SerializeField] private TMP_Text _quantityText;
    [SerializeField] private int _quantity;
    [SerializeField] private Image _icon;

    private ItemSO _item;

    public bool IsDestroyed { get; private set; }
    public RectTransform Rect { get; private set; }

    public int MaxStack => _item.MaxStack;
    public string ItemID => _item.ID;
    public ItemSO Item => _item;

    #region ==== Init ====

    public void Init(ItemSO item)
    {
        _item = item;
        _icon.sprite = item.Icon;
        IsDestroyed = false;
    }

    public void Dispose()
    {
        IsDestroyed = true;
    }

    #endregion

    #region ==== Unity API ====

    private void Awake()
    {
        Rect = GetComponent<RectTransform>();
    }

    #endregion

    public bool IsFull() => _quantity >= _item.MaxStack;
    public bool IsEmpty() => _quantity <= 0;
    public int GetQuantity() => _quantity;

    public void SetQuantity(int amount)
    {
        _quantity = Mathf.Clamp(amount, 0, MaxStack);
        UpdateVisual();


        if (amount <= 0)
            OnEmpty?.Invoke(this);
    }

    private void UpdateVisual()
    {
        _quantityText.text = _quantity > 1 ? _quantity.ToString() : string.Empty;
    }
}
