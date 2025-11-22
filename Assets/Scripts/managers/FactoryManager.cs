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

    [SerializeField] private FactoryDefinition factoryDefinition;

    public Factory Factory { get; private set; }

    private void Start()
    {
        Vector3Int factoryPosition = worldManager.World.GetRandomPosition();
        Factory = new Factory(factoryPosition, factoryDefinition);

        GameObject factoryGameObject = GameObject.Instantiate(factoryDefinition.FactoryPrefab);
        factoryGameObject.transform.position = worldManager.CellToWorld(factoryPosition) + new Vector3(0, 0.5f, 0);

        vehicleManager.CreateVehicle(0);
        vehicleManager.CreateVehicle(0);
        vehicleManager.CreateVehicle(0);
        vehicleManager.CreateVehicle(0);
        vehicleManager.CreateVehicle(0);
        vehicleManager.CreateVehicle(0);

        var store = storeManager.CreateStore(worldManager.World.GetRandomPositionAt(Factory.Position, 50), 0);
        var store2 = storeManager.CreateStore(worldManager.World.GetRandomPositionAt(Factory.Position, 50), 0);
        var store3 = storeManager.CreateStore(worldManager.World.GetRandomPositionAt(Factory.Position, 50), 0);
        var store4 = storeManager.CreateStore(worldManager.World.GetRandomPositionAt(Factory.Position, 50), 0);
        var store5 = storeManager.CreateStore(worldManager.World.GetRandomPositionAt(Factory.Position, 50), 0);

        vehicleManager.StartDelivery(Factory, store);
        vehicleManager.StartDelivery(Factory, store2);
        vehicleManager.StartDelivery(Factory, store3);
        vehicleManager.StartDelivery(Factory, store4);
        vehicleManager.StartDelivery(Factory, store5);
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
