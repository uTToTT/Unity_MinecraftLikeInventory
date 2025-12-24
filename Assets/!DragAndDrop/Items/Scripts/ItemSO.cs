using UnityEngine;

public abstract class ItemSO : ScriptableObject
{
    [Header("Base info")]
    public string ID;
    public string DisplayName;
    public Sprite Icon;


    [Header("Stacking")]
    public bool IsStackable = true;
    public int MaxStack = 64;
}
