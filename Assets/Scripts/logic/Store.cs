using UnityEngine;
using System.Collections;

public class Store
{
    public Vector3Int Position { get; private set; }
    public StoreDefinition Definition { get; private set; }

    public Store(Vector3Int position, StoreDefinition definition)
    {
        this.Position = position;
        this.Definition = definition;
    }
}