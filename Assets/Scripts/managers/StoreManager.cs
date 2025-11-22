using System.Collections.Generic;
using UnityEngine;

public class StoreManager : MonoBehaviour
{
    [SerializeField] private WorldManager worldManager;
    [SerializeField] private List<StoreDefinition> storeDefinitions;

    public List<Store> Stores { get; private set; } = new List<Store>();

    public Store CreateStore(Vector3Int position, int index)
    {
        StoreDefinition storeDefinition = storeDefinitions[index];
        Store newStore = new Store(position, storeDefinition);
        Stores.Add(newStore);

        GameObject storeGameObject = GameObject.Instantiate(storeDefinition.StorePrefab);
        storeGameObject.transform.position = worldManager.CellToWorld(newStore.Position) + new Vector3(0,0.5f,0);

        return newStore;
    }
}
