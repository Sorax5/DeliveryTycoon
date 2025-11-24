using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VehicleUI : MonoBehaviour
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

    [SerializeField] private TextMeshProUGUI labelName;
    [SerializeField] private TextMeshProUGUI uniqueId;
    [SerializeField] private TextMeshProUGUI speed;
    [SerializeField] private TMP_Dropdown algorithmDropdown;
    [SerializeField] private Toggle availabilityToggle;

    private void updateUI() 
    {
        if (!hasBeenInitialised)
        {
            algorithmDropdown.ClearOptions();
            algorithmDropdown.AddOptions(Enum.GetNames(typeof(AlgorithmeEnum)).ToList());
            algorithmDropdown.onValueChanged.AddListener(OnValueChange);
            hasBeenInitialised = true;
        }
        

        labelName.text = vehicle.Definition.Name;
        uniqueId.text = vehicle.UniqueId.ToString();
        speed.text = speedFormat(vehicle.Speed);
        algorithmDropdown.value = (int)vehicle.Algorithme;
        availabilityToggle.isOn = vehicle.IsAvailable;

        vehicle.OnAvailabilityChanged += () =>
        {
            availabilityToggle.isOn = vehicle.IsAvailable;
        };

        vehicle.OnSpeedMultiplierChanged += () =>
        {
            speed.text = speedFormat(vehicle.Speed);
        };
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
