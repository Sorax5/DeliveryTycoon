using UnityEngine;

public class FactoryEffects : MonoBehaviour
{
    [SerializeField] private ParticleSystem moneyParticleSystem;
    
    public void PlayMoneyEffect()
    {
        if (moneyParticleSystem != null)
        {
            moneyParticleSystem.Play();
        }
    }
}
