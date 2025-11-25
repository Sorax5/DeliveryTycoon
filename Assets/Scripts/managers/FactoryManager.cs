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

    [SerializeField] private GameObject player;

    [SerializeField] private MoneyUI moneyUI;

    [SerializeField] private FactoryDefinition factoryDefinition;

    public Factory Factory { get; private set; }
    public FactoryEffects FactoryEffects { get; private set; }

    public const int AmountOfVehicles = 10;
    public const int AmountOfStores = 8;

    private void Start()
    {
        Vector3Int factoryPosition = worldManager.World.GetRandomPosition();
        Factory = new Factory(factoryPosition, factoryDefinition);
        player.transform.position = worldManager.CellToWorld(factoryPosition) + new Vector3(0, 0.5f, 0);

        GameObject factoryGameObject = GameObject.Instantiate(factoryDefinition.FactoryPrefab);
        factoryGameObject.transform.position = worldManager.CellToWorld(factoryPosition) + new Vector3(0, 0.5f, 0);
        FactoryEffects = factoryGameObject.GetComponent<FactoryEffects>();

        for (var i = 0; i < AmountOfVehicles; i++)
        {
            vehicleManager.CreateVehicle(0);
        }

        for (var i = 0; i < AmountOfStores; i++)
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
        if (!vehicleManager.HasAvailableVehicle())
        {
            return;
        }

        var randomStore = storeManager.GetRandomStore();
        vehicleManager.StartDelivery(Factory, randomStore);
    }
}
