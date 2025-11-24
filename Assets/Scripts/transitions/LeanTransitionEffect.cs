using System.Collections;
using UnityEngine;

public class LeanTransitionEffect : TransitionEffect
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float duration = 1.2f;

    public override IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        LeanTween.alphaCanvas(canvasGroup, 1f, duration)
            .setEaseOutQuad();

        yield return new WaitForSeconds(duration);
    }

    public override IEnumerator FadeOut()
    {
        canvasGroup.alpha = 1f;
        LeanTween.alphaCanvas(canvasGroup, 0f, duration)
            .setEaseInQuad();

        yield return new WaitForSeconds(duration);
    }
}
