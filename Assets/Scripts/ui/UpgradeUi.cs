using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUi : MonoBehaviour
{
    [SerializeField] public UpgradeDefinition upgradeDefinition;

    [SerializeField] private TextMeshProUGUI upgradeNameText;
    [SerializeField] private TextMeshProUGUI upgradeDescriptionText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private Button applyUpgradeButton;

    private Vehicle vehicle;
    private bool hasInitialised = false;
    public Vehicle Vehicle
    {
        get => vehicle;
        set
        {
            vehicle = value;
            updateUI();
        }
    }

    public void ApplyUpgrade()
    {
        if (vehicle != null && upgradeDefinition != null)
        {
            upgradeDefinition.ApplyUpgrade(vehicle);
            updateUI();
        }
    }

    private void updateUI()
    {
        if(!hasInitialised)
        {
            applyUpgradeButton.onClick.AddListener(ApplyUpgrade);
            hasInitialised = true;
        }

        upgradeNameText.text = upgradeDefinition.Name;
        upgradeDescriptionText.text = upgradeDefinition.Description;
        upgradeCostText.text = $"Cost: {upgradeDefinition.Cost}";
        applyUpgradeButton.interactable = vehicle != null && vehicle.Speed >= 0; // Example condition

    }
}
