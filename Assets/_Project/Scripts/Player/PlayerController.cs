using UnityEngine;
using UnityEngine.InputSystem;

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
        public float mouseSensitivity = 0.1f;
        public float minPitch = -80f;
        public float maxPitch = 80f;
        public bool lockCursorOnStart = true;

        private CharacterController _characterController;
        private float _cameraPitch = 0f;
        private Vector3 _velocity;
        private bool _isGrounded;
        private Vector3 _spawnPosition;

        public bool IsMoving { get; private set; }
        public float CurrentSpeed { get; private set; }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _spawnPosition = transform.position;

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
            var mouse = Mouse.current;
            if (mouse == null) return;

            Vector2 mouseDelta = mouse.delta.ReadValue() * mouseSensitivity;

            // Rotazione orizzontale del player
            transform.Rotate(Vector3.up * mouseDelta.x);

            // Rotazione verticale della telecamera (Pitch con clamp)
            _cameraPitch -= mouseDelta.y;
            _cameraPitch = Mathf.Clamp(_cameraPitch, minPitch, maxPitch);

            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            _isGrounded = _characterController.isGrounded;
            if (_isGrounded)
            {
                if (_velocity.y < 0)
                {
                    _velocity.y = -2f; // Mantieni il contatto a terra
                }
            }
            else
            {
                // Gravità applicata solo quando non si tocca terra
                _velocity.y += gravity * Time.deltaTime;
            }

            var keyboard = Keyboard.current;
            float moveX = 0f;
            float moveZ = 0f;
            bool isSprinting = false;

            if (keyboard != null)
            {
                if (keyboard.dKey.isPressed) moveX += 1f;
                if (keyboard.aKey.isPressed) moveX -= 1f;
                if (keyboard.wKey.isPressed) moveZ += 1f;
                if (keyboard.sKey.isPressed) moveZ -= 1f;

                isSprinting = keyboard.leftShiftKey.isPressed;
            }

            Vector3 moveDirection = (transform.right * moveX + transform.forward * moveZ).normalized;
            float speed = isSprinting ? sprintSpeed : walkSpeed;

            CurrentSpeed = moveDirection.magnitude * speed;
            IsMoving = moveDirection.magnitude > 0.1f && _isGrounded;

            _characterController.Move(moveDirection * speed * Time.deltaTime);

            _characterController.Move(_velocity * Time.deltaTime);

            // Salvaguardia se si cade nel vuoto (es. durante caricamenti ritardati di scene)
            if (transform.position.y < -10f)
            {
                _characterController.enabled = false;
                transform.position = _spawnPosition;
                _velocity = Vector3.zero;
                _characterController.enabled = true;
            }
        }

        private void HandleCursorToggle()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                SetCursorLocked(Cursor.lockState != CursorLockMode.Locked);
            }
            else if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
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
