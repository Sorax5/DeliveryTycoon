using UnityEngine;
using System.Collections;

public class Floor
{
    private float poidsMultiplier = 1.0f;
    private float speedMultiplier = 1.0f;

    public Vector3Int Position { get; private set; }
    public FloorDefinition Definition { get; private set; }
    public float Poids => poidsMultiplier * Definition.Poid;
    public float Speed => speedMultiplier * Definition.Speed;

    public Floor(Vector3Int position, FloorDefinition definition)
    {
        this.Position = position;
        this.Definition = definition;
    }

}