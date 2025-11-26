using System.Collections.Generic;
using UnityEngine;

public class VehicleEffect : MonoBehaviour
{
    [SerializeField] private List<Sprite> Sprites;
    [SerializeField] private float angleOffset = 360f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        if (Sprites.Count == 0)
        {
            Debug.LogWarning("No sprites assigned to VehicleEffect.");
            return;
        }

        float angle = transform.eulerAngles.z;
        float step = 360f / Sprites.Count;

        int startIndex = 36;
        int index = (Mathf.FloorToInt(angle / step) + startIndex) % Sprites.Count;

        spriteRenderer.sprite = Sprites[index];

    }
}
