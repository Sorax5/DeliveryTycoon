using UnityEngine;
using System.Collections;

public class Factory
{
    public Vector3Int Position { get; private set; }
    public FactoryDefinition Definition { get; private set; }

    public Factory(Vector3Int position, FactoryDefinition definition)
    {
        this.Position = position;
        this.Definition = definition;
    }
}