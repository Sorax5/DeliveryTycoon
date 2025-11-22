using UnityEngine;

[CreateAssetMenu(fileName = "NoiseDefinition", menuName = "delivery/NoiseDefinition")]
public class NoiseDefinition : ScriptableObject
{
    [Header("General")]
    public FastNoiseLite.NoiseType NoiseType = FastNoiseLite.NoiseType.OpenSimplex2;
    public float Frequency = 0.01f;

    [Header("Fractal")] 
    public FastNoiseLite.FractalType FractalType = FastNoiseLite.FractalType.None;
    public int Octaves = 3;
    public float Lacunarity = 2.0f;
    public float Gain = 0.5f;
    public float WeightedStrength = 0.0f;

    [Header("Cellular")]
    public FastNoiseLite.CellularDistanceFunction CellularDistanceFunction = FastNoiseLite.CellularDistanceFunction.Euclidean;
    public FastNoiseLite.CellularReturnType CellularReturnType = FastNoiseLite.CellularReturnType.CellValue;
    public float CellularJitter = 0.45f;

    public FastNoiseLite CreateNoiseMaker()
    {
        FastNoiseLite noiseMaker = new FastNoiseLite();
        noiseMaker.SetNoiseType(NoiseType);
        noiseMaker.SetFrequency(Frequency);
        if (FractalType != FastNoiseLite.FractalType.None)
        {
            noiseMaker.SetFractalType(FractalType);
            noiseMaker.SetFractalOctaves(Octaves);
            noiseMaker.SetFractalLacunarity(Lacunarity);
            noiseMaker.SetFractalGain(Gain);
            noiseMaker.SetFractalWeightedStrength(WeightedStrength);
        }

        if (NoiseType == FastNoiseLite.NoiseType.Cellular)
        {
            noiseMaker.SetCellularDistanceFunction(CellularDistanceFunction);
            noiseMaker.SetCellularReturnType(CellularReturnType);
            noiseMaker.SetCellularJitter(CellularJitter);
        }
        return noiseMaker;
    }


}
