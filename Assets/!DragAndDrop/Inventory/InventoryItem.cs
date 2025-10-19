using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private ItemSO _item;
    [SerializeField] private InventoryController _controller; // tmp
    [SerializeField] private TMP_Text _quantityText;
    [SerializeField] private int _quantity;

    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;

    public RectTransform Rect => _rectTransform;
    public CanvasGroup CanvasGroup => _canvasGroup;
    public ItemSO Item => _item;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Init(InventoryController controller)
    {
        _controller = controller;
    }

    public int GetQuantity() => _quantity;

    public void SetQuantity(int amount)
    {
        if (amount <= 0)
            throw new System.ArgumentException("Quantity must be greater than zero!");

        _quantity = amount;

        if (amount > 1)
            _quantityText.text = amount.ToString();
        else
            _quantityText.text = string.Empty;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                _controller.OnItemClickHandler(this, eventData);
                break;
            case PointerEventData.InputButton.Right:
                break;
            default:
                break;
        }
    }
}
