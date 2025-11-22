using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "FloorDefinition", menuName = "delivery/FloorDefinition")]
public class FloorDefinition : ScriptableObject
{
    [Header("Gameplay")]
    public float Poid = 1.0f;
    public float Speed = 1.0f;

    public Tile Tile;

    [Header("Debug")]
    [Tooltip("Couleur utilisée pour l'aperçu et le visualiseur (debug).")]
    public Color debugColor = Color.white;
}
