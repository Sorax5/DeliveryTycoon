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
