using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private InventoryController _controller;

    [Header("Coal")]
    [SerializeField] private ItemSO _coal;
    [Space]
    [SerializeField, Min(1)] private int _amountCoal = 1;
    [SerializeField] private Button _addCoalButton;

    [Header("Iron")]
    [SerializeField] private ItemSO _iron;
    [Space]
    [SerializeField, Min(1)] private int _amountIron = 1;
    [SerializeField] private Button _addIronButton;

    public void Init()
    {
        _controller.Init();

        _addCoalButton.onClick.RemoveAllListeners();
        _addIronButton.onClick.RemoveAllListeners();

        _addCoalButton.onClick.AddListener(AddCoal);
        _addIronButton.onClick.AddListener(AddIron);
    }

    private void OnValidate()
    {
        SetText(_addCoalButton, "Coal", _amountCoal);
        SetText(_addIronButton, "Iron", _amountIron);
    }

    private void AddCoal() => AddItem(_coal, _amountCoal);
    private void AddIron() => AddItem(_iron, _amountIron);
    private void SetText(Button button,string item, int amount)
    {
        var tmp = button.GetComponentInChildren<TMP_Text>();
        tmp.text = $"{item} +{amount.ToString()}" ;
    }

    private void AddItem(ItemSO item, int amount = 1) => _controller.AddItem(item, amount);
}
