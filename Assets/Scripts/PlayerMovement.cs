using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    public const string MOVE_INPUT = "Move";

    [Header("Parameters")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private PolygonCollider2D movementBounds; // Losange (diamond) bounds
    [SerializeField] private bool snapToGrid = true;
    [SerializeField] private float gridSize = 1f / 32f;

    [Header("Options")] 
    [SerializeField] private bool restrictToBounds = true; // Empêcher de sortir

    private PlayerInput playerInput;
    private Rigidbody2D rb;
    private InputAction moveAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        moveAction = playerInput.actions[MOVE_INPUT];
    }

    private void FixedUpdate()
    {
        if (moveAction == null) return;

        Vector2 inputMove = moveAction.ReadValue<Vector2>();
        if (inputMove.sqrMagnitude > 1f) // Normaliser diagonale
            inputMove = inputMove.normalized;

        Vector2 targetPos = rb.position + Time.fixedDeltaTime * inputMove * moveSpeed;

        if (snapToGrid)
            targetPos = Snap(targetPos);

        if (restrictToBounds && movementBounds != null)
            targetPos = KeepInsideBounds(targetPos);

        rb.MovePosition(targetPos);
    }

    private Vector2 Snap(Vector2 pos)
    {
        if (gridSize <= 0f) return pos;
        float x = Mathf.Round(pos.x / gridSize) * gridSize;
        float y = Mathf.Round(pos.y / gridSize) * gridSize;
        return new Vector2(x, y);
    }

    private Vector2 KeepInsideBounds(Vector2 desired)
    {
        // Si déjà dedans, ok
        if (movementBounds.OverlapPoint(desired))
            return desired;

        // Projection au point le plus proche sur le contour, puis re-snap si besoin
        Vector2 clamped = movementBounds.ClosestPoint(desired);
        if (snapToGrid)
            clamped = Snap(clamped);
        return clamped;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (movementBounds != null)
        {
            Gizmos.color = Color.yellow;
            // Affiche les points du losange
            var pts = movementBounds.points;
            var t = movementBounds.transform;
            for (int i = 0; i < pts.Length; i++)
            {
                Vector2 a = t.TransformPoint(pts[i]);
                Vector2 b = t.TransformPoint(pts[(i + 1) % pts.Length]);
                Gizmos.DrawLine(a, b);
            }
        }
    }
#endif
}
