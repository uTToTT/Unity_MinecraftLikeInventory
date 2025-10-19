using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    //public event Action<RectTransform, PointerEventData> BeginDrag;
    //public event Action<RectTransform, PointerEventData> Drag;
    //public event Action<RectTransform, PointerEventData> EndDrag;

    [SerializeField] private InventoryController _controller; // tmp

    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _controller.OnItemBeginDragHandler(_rectTransform, eventData);
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _controller.OnItemDragHandler(_rectTransform, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _controller.OnItemEndDragHandler(_rectTransform, eventData);
        _canvasGroup.blocksRaycasts = true;
    }
}
