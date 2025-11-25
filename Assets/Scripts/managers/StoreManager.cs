using System;
using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    public event Action<Store> OnStoreOrderCreated;

    [SerializeField] private WorldManager worldManager;
    [SerializeField] private List<StoreDefinition> storeDefinitions;

    public List<Store> Stores { get; private set; } = new List<Store>();

    public Store CreateStore(Vector3Int position, int index)
    {
        StoreDefinition storeDefinition = storeDefinitions[index];
        Store newStore = new Store(position, storeDefinition);
        Stores.Add(newStore);
        newStore.OnOrderCreated += (order) => OnStoreOrderCreated?.Invoke(newStore);

        GameObject storeGameObject = GameObject.Instantiate(storeDefinition.StorePrefab);
        storeGameObject.transform.position = worldManager.CellToWorld(newStore.Position) + new Vector3(0,0.5f,0);

        return newStore;
    }

    /*private void Update()
    {
        foreach (var store in Stores)
        {
            if (store.CanCreateOrder())
            {
                store.CreateOrder();
            }
        }

    }*/

    public Store GetRandomStore()
    {
        if (Stores.Count == 0)
        {
            throw new InvalidOperationException("No stores available.");
        }
        var randomIndex = UnityEngine.Random.Range(0, Stores.Count);
        return Stores[randomIndex];
    }
}
