using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryStack : MonoBehaviour
{
    [SerializeField] private TMP_Text _quantityText;
    [SerializeField] private int _quantity;
    [SerializeField] private Image _icon;

    private ItemSO _item;
    private RectTransform _rectTransform;

    public RectTransform Rect => _rectTransform;
    public bool IsFull => _quantity >= _item.MaxStack;
    public int MaxStack => _item.MaxStack;
    public string ItemID => _item.ID;

    public void Init(ItemSO item)
    {
        _item = item;
        _icon.sprite = item.Icon;
    }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public int GetQuantity() => _quantity;

    public void SetQuantity(int amount)
    {
        if (amount <= 0)
            throw new System.ArgumentException("Quantity must be greater than zero!");

        _quantity = amount;

        string quantityText = amount > 1 ? amount.ToString() : string.Empty;

        _quantityText.text = quantityText;
    }
}
