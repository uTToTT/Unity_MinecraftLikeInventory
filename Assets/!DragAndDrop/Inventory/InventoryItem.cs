using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItem : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private AnimationCurve _animationCurve;

    private const float MOVE_SPEED = 5f;

    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Canvas _canvas;

    private bool _isMoving;
    private Vector3 _targetPosition;

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (_isMoving)
        {
            _rectTransform.MoveTowards(_targetPosition, MOVE_SPEED);

            if (_rectTransform.IsReach(_targetPosition))
            {
                _rectTransform.localPosition = Vector3.zero;
                _isMoving=false;
            }
        }
    }

    private void MoveToPosition(Vector3 pos)
    {
        _targetPosition = pos;
        _isMoving= true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var slot = _rectTransform.parent;
        slot.SetAsLastSibling();
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        MoveToPosition(_rectTransform.parent.position);
        _canvasGroup.blocksRaycasts = true;
    }
}
