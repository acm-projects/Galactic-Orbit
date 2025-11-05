using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;

[RequireComponent(typeof(CharacterController))]
public class GPSCharacterController : MonoBehaviour
{
    [Header("Mapbox")]
    public AbstractMap map;

    [Header("GPS Settings")]
    public Vector2d targetLocation;       // The GPS coordinate you want to move to
    public float moveSpeed = 5f;          // Movement speed in Unity units

    [Header("Rotation")]
    public float rotationSpeed = 720f;    // degrees per second

    [Header("Animator (Optional)")]
    public Animator animator;

    private CharacterController controller;
    private Vector3 targetWorldPos;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Initialize target position in world space
        targetWorldPos = map.GeoToWorldPosition(targetLocation, true);
    }

    void Update()
    {
        if (map == null) return;

        // Recalculate world target each frame to account for map panning/zooming
        targetWorldPos = map.GeoToWorldPosition(targetLocation, true);

        // Compute movement vector toward the target
        Vector3 direction = targetWorldPos - transform.position;
        direction.y = 0; // No vertical movement

        float distance = direction.magnitude;
        Vector3 move = Vector3.zero;

        if (distance > 0.1f) // Prevent jitter when very close
        {
            move = direction.normalized * moveSpeed * Time.deltaTime;
            controller.Move(move);

            // Smooth rotation toward movement direction
            if (move != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        // Animator updates
        if (animator)
        {
            float speedPercent = move.magnitude / (moveSpeed * Time.deltaTime);
            animator.SetFloat("Speed", speedPercent);
            animator.SetBool("IsWalking", distance > 0.1f);
            animator.SetBool("IsGrounded", true); // always grounded here
            animator.SetFloat("VerticalVelocity", 0f);
        }
    }

    /// <summary>
    /// Call this to set a new GPS target for the player to move toward.
    /// </summary>
    public void SetTargetLocation(Vector2d newLocation)
    {
        targetLocation = newLocation;
    }
}
