using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[Serializable]
public class FloorRange
{
    public FloorDefinition FloorDefinition;

    [MinMaxRangeSlider(-1.1f, 1f)]
    public Vector2 NoiseRange;
}


[CreateAssetMenu(fileName = "MapDefinition", menuName = "delivery/MapDefinition")]
public class MapDefinition : ScriptableObject
{
    public int Width;
    public int Height;

    public NoiseDefinition NoiseDefinition;
    public List<FloorRange> FloorRanges;

    [CanBeNull]
    public FloorRange GetFloorRangeByNoise(float noise)
    {
        if (FloorRanges == null || FloorRanges.Count == 0)
        {
            return null;
        }
        var match = FloorRanges.Find(fd => noise >= fd.NoiseRange.x && noise < fd.NoiseRange.y);

        if (match == null)
        {
            match = FloorRanges.Find(fd => Mathf.Approximately(noise, fd.NoiseRange.y));
        }

        return match;
    }
}
