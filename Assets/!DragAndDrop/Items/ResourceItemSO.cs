using UnityEngine;

[CreateAssetMenu(fileName = "Resource", menuName = "Item/Resource")]
public class ResourceItemSO : ItemSO
{
    public ItemType ItemType => ItemType.Resource;
}
