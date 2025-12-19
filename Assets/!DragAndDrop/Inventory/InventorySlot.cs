using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<InventorySlot, PointerEventData> SlotPointerEnter;
    public event Action<InventorySlot, PointerEventData> SlotPointerExit;

    [SerializeField] private Image _highlight;
    [SerializeField] private RectTransform _itemContainer;
    [SerializeField] private TMP_Text _index;

    private InventoryStack _containedStack;

    public void SetIndex(int index) => _index.text = index.ToString();
    public bool HasStack() => _containedStack != null;
    public void RemoveStack() => _containedStack = null;
    public InventoryStack GetStack() => _containedStack;

    public void AddStack(InventoryStack item)
    {
        if (HasStack()) Debug.LogWarning("Already contain stack.");

        item.Rect.SetParent(_itemContainer);
        item.Rect.localPosition = Vector3.zero;
        _containedStack = item;
    }

    #region ==== Handlers ====

    public void OnPointerEnter(PointerEventData eventData)
    {
        _highlight.enabled = true;
        SlotPointerEnter?.Invoke(this, eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _highlight.enabled = false;
        SlotPointerExit?.Invoke(this, eventData);
    }

    #endregion
}
