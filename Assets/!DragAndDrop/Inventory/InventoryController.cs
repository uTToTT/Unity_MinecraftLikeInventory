using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Transform _slotsParent;
    [SerializeField] private Canvas _canvas;
    private readonly List<InventorySlot> _slots = new();

    private RectTransform _rectItem;
    private Vector3 _targetPosition;
    private bool _isMoving;

    public void Init()
    {
        _slots.AddRange(_slotsParent.GetComponentsInChildren<InventorySlot>());
    }

    #region ==== Unity API ====

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        if (_isMoving)
        {
            _rectItem.MoveTowards(_targetPosition, _moveSpeed);

            if (_rectItem.IsReach(_targetPosition))
            {
                _rectItem.localPosition = Vector3.zero;
                _isMoving = false;
            }
        }
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

    public void OnItemBeginDragHandler(RectTransform rect, PointerEventData eventData)
    {
        var slot = rect.parent;
        slot.SetAsLastSibling();
    }

    public void OnItemDragHandler(RectTransform rect, PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public void OnItemEndDragHandler(RectTransform rect, PointerEventData eventData)
    {
        MoveToPosition(rect, rect.parent.position);
    }

    #endregion

    private void MoveToPosition(RectTransform rect, Vector3 pos)
    {
        _rectItem = rect;   
        _targetPosition = pos;
        _isMoving = true;
    }
}
