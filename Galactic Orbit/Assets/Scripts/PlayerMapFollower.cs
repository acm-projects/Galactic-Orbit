using UnityEngine;


public class PlayerMapFollower : MonoBehaviour


{
    [SerializeField] private float rotationSpeed = 8f;   // Smooth turning speed
    [SerializeField] private Animator animator;

    private Vector3 _targetDirection = Vector3.zero;
    private bool _isWalking = false;
    private float _currentSpeed = 0f;
    
    void OnEnable()
    {
        var mapFollower = FindFirstObjectByType<MapFollowerSmooth>();
        if (mapFollower != null)
            mapFollower.OnVelocityCalculated += HandleVelocity;
    }

    void OnDisable()
    {
        var mapFollower = FindFirstObjectByType<MapFollowerSmooth>();
        if (mapFollower != null)
            mapFollower.OnVelocityCalculated -= HandleVelocity;
    }

    void Update()
    {
        if (_isWalking && _targetDirection != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(_targetDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    void HandleVelocity(Vector3 velocity)
    {
        _currentSpeed = velocity.magnitude;
        _isWalking = _currentSpeed > 0.05f;

        if (_isWalking)
        {
            _targetDirection = velocity.normalized;
            _targetDirection.y = 0f;
        }
        else
        {
            _targetDirection = Vector3.zero;
        }

        animator.SetBool("IsWalking", _isWalking);
        //animator.SetFloat("Speed", _currentSpeed);
    }
}
