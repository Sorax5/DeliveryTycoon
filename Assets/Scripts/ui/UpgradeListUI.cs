using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeListUI : MonoBehaviour
{
    private Vehicle vehicle;
    private bool hasBeenInitialised = false;

    public Vehicle Vehicle
    {
        get => vehicle;
        set
        {
            vehicle = value;
            updateUI();
        }
    }

    [SerializeField] private GameObject child;
    [SerializeField] private TextMeshProUGUI labelName;
    [SerializeField] private TextMeshProUGUI uniqueId;
    [SerializeField] private TextMeshProUGUI speed;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Dropdown algorithmDropdown;
    [SerializeField] private Toggle availabilityToggle;

    [SerializeField] private List<UpgradeDefinition> upgrades;
    [SerializeField] private GameObject upgradeUiPrefab;
    [SerializeField] private List<UpgradeUi> upgradeUi;

    public void ActivateUi(Vehicle vehicle)
    {
        child.SetActive(true);
        this.Vehicle = vehicle;
    }

    private void updateUI()
    {
        if (!hasBeenInitialised)
        {
            algorithmDropdown.ClearOptions();
            algorithmDropdown.AddOptions(Enum.GetNames(typeof(AlgorithmeEnum)).ToList());
            algorithmDropdown.onValueChanged.AddListener(OnValueChange);
            hasBeenInitialised = true;

            foreach (var upgradeDefinition in upgrades)
            {
                var ui = Instantiate(upgradeUiPrefab, transform).GetComponent<UpgradeUi>();
                ui.Vehicle = vehicle;
                ui.upgradeDefinition = upgradeDefinition;
            }
        }

        labelName.text = vehicle.Definition.Name;
        uniqueId.text = vehicle.UniqueId.ToString();
        speed.text = speedFormat(vehicle.Speed);
        algorithmDropdown.value = (int)vehicle.Algorithme;
        availabilityToggle.isOn = vehicle.IsAvailable;
        icon.sprite = Sprite.Create(vehicle.Definition.Sprite, new Rect(0, 0, vehicle.Definition.Sprite.width, vehicle.Definition.Sprite.height), new Vector2(0.5f, 0.5f));

        vehicle.OnAvailabilityChanged += () =>
        {
            availabilityToggle.isOn = vehicle.IsAvailable;
        };

        vehicle.OnSpeedMultiplierChanged += () =>
        {
            speed.text = speedFormat(vehicle.Speed);
        };

        foreach (var ui in upgradeUi)
        {
            ui.Vehicle = vehicle;
        }
    }

    private string speedFormat(float speed)
    {
        return $"{speed} km/h";
    }

    private void OnValueChange(int value)
    {
        vehicle.Algorithme = (AlgorithmeEnum)value;
    }
}
