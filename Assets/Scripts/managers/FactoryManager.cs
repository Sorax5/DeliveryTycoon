using System;
using UnityEngine;

public class FactoryManager : MonoBehaviour
{
    #region singleton
    public static FactoryManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    [SerializeField] private WorldManager worldManager;
    [SerializeField] private VehicleManager vehicleManager;
    [SerializeField] private StoreManager storeManager;
    [SerializeField] private MoneyUI moneyUI;

    [SerializeField] private FactoryDefinition factoryDefinition;

    public Factory Factory { get; private set; }
    public FactoryEffects FactoryEffects { get; private set; }

    private void Start()
    {
        Vector3Int factoryPosition = worldManager.World.GetRandomPosition();
        Factory = new Factory(factoryPosition, factoryDefinition);

        GameObject factoryGameObject = GameObject.Instantiate(factoryDefinition.FactoryPrefab);
        factoryGameObject.transform.position = worldManager.CellToWorld(factoryPosition) + new Vector3(0, 0.5f, 0);
        FactoryEffects = factoryGameObject.GetComponent<FactoryEffects>();

        int amountOfVehicles = 20;
        for (int i = 0; i < amountOfVehicles; i++)
        {
            vehicleManager.CreateVehicle(0);
        }

        int amountOfStores = 8;
        for (int i = 0; i < amountOfStores; i++)
        {
            storeManager.CreateStore(worldManager.World.GetRandomPositionAt(Factory.Position, 50), 0);
        }

        moneyUI.Factory = Factory;

        vehicleManager.OnAdventureBackUpEnded += VehicleManager_OnAdventureBackUpEnded;
    }

    private void VehicleManager_OnAdventureBackUpEnded(Vehicle obj)
    {
        Factory.Money += 10;
        FactoryEffects.PlayMoneyEffect();
    }

    private void Update()
    {
        if (vehicleManager.HasAvailableVehicle())
        {
            var randomStore = storeManager.GetRandomStore();
            vehicleManager.StartDelivery(Factory, randomStore);
        }
    }
}
