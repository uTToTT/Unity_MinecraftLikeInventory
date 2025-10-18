using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var item = eventData.pointerDrag.transform;
        item.SetParent(transform);
    }
}
