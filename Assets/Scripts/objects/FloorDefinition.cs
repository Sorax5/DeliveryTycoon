using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "FloorDefinition", menuName = "delivery/FloorDefinition")]
public class FloorDefinition : ScriptableObject
{
    [Header("Gameplay")] 
    public string Id;
    public float Poid = 1.0f;
    public float Speed = 1.0f;

    public List<Tile> Variants = new List<Tile>();

    [Header("Debug")]
    [Tooltip("Couleur utilisée pour l'aperçu et le visualiseur (debug).")]
    public Color debugColor = Color.white;

    [CanBeNull]
    public Tile GetRandomVariant()
    {
        if (Variants.Count == 0)
        {
            return null;
        }
        var index = Random.Range(0, Variants.Count);
        return Variants[index];
    }
}
