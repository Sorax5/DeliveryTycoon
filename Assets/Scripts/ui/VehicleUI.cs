using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VehicleUI : MonoBehaviour
{
    private Vehicle vehicle;

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

    private void Start()
    {
        algorithmDropdown.ClearOptions();
        algorithmDropdown.AddOptions(Enum.GetNames(typeof(AlgorithmeEnum)).ToList());  

        algorithmDropdown.onValueChanged.AddListener((int value) =>
        {
            vehicle.Algorithme = (AlgorithmeEnum)value;
        });
    }

    private void updateUI() 
    {
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
}
