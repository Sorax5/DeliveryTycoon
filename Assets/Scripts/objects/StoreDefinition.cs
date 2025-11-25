using Unity.Cinemachine;
using UnityEngine;

[CreateAssetMenu(fileName = "StoreDefinition", menuName = "delivery/StoreDefinition")]
public class StoreDefinition : ScriptableObject
{
    public GameObject StorePrefab;

    [MinMaxRangeSlider(0, 100)]
    public Vector2Int RewardRange;

    public float OrderChance = 0.7f;
}
