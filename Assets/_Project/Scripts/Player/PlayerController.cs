using UnityEngine;

namespace AvenueXR.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 3.0f;
        public float sprintSpeed = 5.0f;
        public float gravity = -9.81f;

        [Header("Mouse Look")]
        public Transform cameraTransform;
        public float mouseSensitivity = 2.0f;
        public float minPitch = -80f;
        public float maxPitch = 80f;
        public bool lockCursorOnStart = true;

        private CharacterController _characterController;
        private float _cameraPitch = 0f;
        private Vector3 _velocity;
        private bool _isGrounded;

        public bool IsMoving { get; private set; }
        public float CurrentSpeed { get; private set; }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            if (cameraTransform == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null) cameraTransform = cam.transform;
            }
        }

        private void Start()
        {
            if (lockCursorOnStart)
            {
                SetCursorLocked(true);
            }
        }

        private void Update()
        {
            HandleCursorToggle();
            
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                HandleMouseLook();
            }

            HandleMovement();
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            // Rotazione orizzontale del player
            transform.Rotate(Vector3.up * mouseX);

            // Rotazione verticale della telecamera (Pitch con clamp)
            _cameraPitch -= mouseY;
            _cameraPitch = Mathf.Clamp(_cameraPitch, minPitch, maxPitch);

            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            _isGrounded = _characterController.isGrounded;
            if (_isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f; // Mantieni il contatto a terra
            }

            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");

            Vector3 moveDirection = (transform.right * moveX + transform.forward * moveZ).normalized;

            bool isSprinting = Input.GetKey(KeyCode.LeftShift);
            float speed = isSprinting ? sprintSpeed : walkSpeed;

            CurrentSpeed = moveDirection.magnitude * speed;
            IsMoving = moveDirection.magnitude > 0.1f && _isGrounded;

            _characterController.Move(moveDirection * speed * Time.deltaTime);

            // Gravità
            _velocity.y += gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        private void HandleCursorToggle()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetCursorLocked(Cursor.lockState != CursorLockMode.Locked);
            }
            else if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
            {
                SetCursorLocked(true);
            }
        }

        public void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }
}
