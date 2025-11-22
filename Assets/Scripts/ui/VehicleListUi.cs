using UnityEngine;
using UnityEngine.UI;

public class VehicleListUI : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject vehicleUIPrefab;

    [SerializeField] private VehicleManager vehicleManager;

    [SerializeField] private Button menuButton;

    private void Awake()
    {
        vehicleManager.OnVehicleAdded += AddVehicleUI;
        menuButton.onClick.AddListener(() =>
        {
            panel.SetActive(!panel.activeSelf);
        });
    }

    private void OnDestroy()
    {
        vehicleManager.OnVehicleAdded -= AddVehicleUI;
    }

    private void AddVehicleUI(Vehicle vehicle)
    {
        GameObject vehicleUIObject = Instantiate(vehicleUIPrefab, content.transform);
        VehicleUI vehicleUI = vehicleUIObject.GetComponent<VehicleUI>();
        vehicleUI.Vehicle = vehicle;
    }
}
