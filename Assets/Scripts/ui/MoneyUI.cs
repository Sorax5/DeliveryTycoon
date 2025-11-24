using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    private Factory _factory;
    public Factory Factory
    {
        get => _factory;
        set
        {
            if (_factory != null)
            {
                _factory.OnMoneyChanged -= UpdateMoneyUI;
            }
            _factory = value;
            if (_factory != null)
            {
                _factory.OnMoneyChanged += UpdateMoneyUI;
                UpdateMoneyUI(_factory.Money);
            }
        }
    }

    [SerializeField] private TextMeshProUGUI moneyText;

    public void UpdateMoneyUI(int newAmount)
    {
        moneyText.text = $"{newAmount}";

    }
}
