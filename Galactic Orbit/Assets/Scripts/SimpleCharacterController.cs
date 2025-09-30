using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f; // degrees per second
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Animator")]
    public Animator animator;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        Debug.Log("Horizontal: " + Input.GetAxis("Horizontal") + ", Vertical: " + Input.GetAxis("Vertical"));

        // Check if grounded
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f; // small downward force to keep grounded

        // Movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        bool isWalking = horizontalVelocity.magnitude > 0.01f;

        Vector3 move = new Vector3(horizontal, 0, vertical);
        if (move.magnitude > 1f) move.Normalize();

        // Move relative to camera
        //move = Camera.main.transform.TransformDirection(move);
        //move.y = 0f;

        controller.Move(move * moveSpeed * Time.deltaTime);

        // Rotate character toward movement direction
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Update Animator
        if (animator)
        {
            float speedPercent = move.magnitude;
            animator.SetFloat("Speed", speedPercent);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsWalking", isWalking);
            animator.SetFloat("VerticalVelocity", velocity.y);
        }
    }
}
