using UnityEngine;
using AvenueXR.Core;

namespace AvenueXR.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Raycast Settings")]
        public Camera playerCamera;
        public float reachDistance = 2.8f;
        public LayerMask interactionLayers = ~0; // Tutti i layer per default

        [Header("Carrying Settings")]
        public Transform holdPoint;
        public float carrySmoothSpeed = 15f;
        public float throwForce = 7.0f;

        [Header("Crank Settings")]
        [Tooltip("Gradi al secondo quando si tiene premuto il tasto sulla manovella.")]
        public float crankHoldSpeed = 220f;
        [Tooltip("Sensibilità alla rotazione tramite movimento mouse.")]
        public float crankMouseSensitivity = 120f;

        private WasteItem _carriedItem;
        private Rigidbody _carriedRb;
        private XRPhysicalCrank _activeCrank;

        public bool IsCarrying => _carriedItem != null;
        public GameObject CurrentHoverObject { get; private set; }
        public bool IsHoveringInteractable { get; private set; }

        private void Awake()
        {
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null) playerCamera = Camera.main;
            }

            if (holdPoint == null && playerCamera != null)
            {
                // Crea automaticamente un holdPoint di default se non assegnato
                GameObject hp = new GameObject("DefaultHoldPoint");
                hp.transform.SetParent(playerCamera.transform);
                hp.transform.localPosition = new Vector3(0.25f, -0.25f, 0.75f);
                hp.transform.localRotation = Quaternion.identity;
                holdPoint = hp.transform;
            }
        }

        private void Update()
        {
            UpdateHoverRaycast();
            HandleInput();
        }

        private void FixedUpdate()
        {
            if (_carriedItem != null && holdPoint != null)
            {
                UpdateCarriedPosition();
            }
        }

        private void UpdateHoverRaycast()
        {
            if (playerCamera == null) return;

            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            CurrentHoverObject = null;
            IsHoveringInteractable = false;

            if (Physics.Raycast(ray, out hit, reachDistance, interactionLayers))
            {
                CurrentHoverObject = hit.collider.gameObject;

                WasteItem item = hit.collider.GetComponentInParent<WasteItem>();
                XRPhysicalCrank crank = hit.collider.GetComponentInParent<XRPhysicalCrank>();

                if (item != null || crank != null)
                {
                    IsHoveringInteractable = true;
                }
            }
        }

        private void HandleInput()
        {
            bool interactPressed = Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0);
            bool interactHeld = Input.GetKey(KeyCode.E) || Input.GetMouseButton(0);
            bool throwPressed = Input.GetMouseButtonDown(1); // Click destro

            // 1. GESTIONE MANOVELLA (Se non stiamo portando un oggetto)
            if (_carriedItem == null)
            {
                if (interactHeld && CurrentHoverObject != null)
                {
                    XRPhysicalCrank crank = CurrentHoverObject.GetComponentInParent<XRPhysicalCrank>();
                    if (crank != null)
                    {
                        _activeCrank = crank;
                        float mouseDelta = (Input.GetAxis("Mouse X") + Input.GetAxis("Mouse Y")) * crankMouseSensitivity;
                        float holdDelta = crankHoldSpeed * Time.deltaTime;
                        float totalDelta = holdDelta + (Mathf.Abs(mouseDelta) > 0.01f ? mouseDelta : 0f);

                        _activeCrank.RotateManual(totalDelta);
                        return; // Non raccogliere oggetti se stiamo girando la manovella
                    }
                }
                else
                {
                    _activeCrank = null;
                }
            }

            // 2. GESTIONE OGGETTO TRASPORTATO
            if (_carriedItem != null)
            {
                if (throwPressed)
                {
                    ThrowCarriedItem();
                }
                else if (interactPressed)
                {
                    DropCarriedItem();
                }
                return;
            }

            // 3. RACCOLTA NUOVO OGGETTO
            if (interactPressed && CurrentHoverObject != null)
            {
                WasteItem item = CurrentHoverObject.GetComponentInParent<WasteItem>();
                if (item != null && !item.IsCarried)
                {
                    PickUpItem(item);
                }
            }
        }

        private void PickUpItem(WasteItem item)
        {
            _carriedItem = item;
            _carriedRb = item.GetComponent<Rigidbody>();

            if (_carriedRb != null)
            {
                _carriedRb.isKinematic = true;
                _carriedRb.useGravity = false;
            }

            item.OnPickedUp();
        }

        private void DropCarriedItem()
        {
            if (_carriedItem == null) return;

            if (_carriedRb != null)
            {
                _carriedRb.isKinematic = false;
                _carriedRb.useGravity = true;
                _carriedRb.linearVelocity = Vector3.zero;
            }

            _carriedItem.OnDropped();
            _carriedItem = null;
            _carriedRb = null;
        }

        private void ThrowCarriedItem()
        {
            if (_carriedItem == null) return;

            Vector3 force = playerCamera != null 
                ? (playerCamera.transform.forward * throwForce + Vector3.up * 1.5f) 
                : (transform.forward * throwForce);

            _carriedItem.OnThrown(force);
            _carriedItem = null;
            _carriedRb = null;
        }

        private void UpdateCarriedPosition()
        {
            Vector3 targetPos = holdPoint.position;
            Quaternion targetRot = holdPoint.rotation;

            _carriedItem.transform.position = Vector3.Lerp(_carriedItem.transform.position, targetPos, Time.fixedDeltaTime * carrySmoothSpeed);
            _carriedItem.transform.rotation = Quaternion.Slerp(_carriedItem.transform.rotation, targetRot, Time.fixedDeltaTime * carrySmoothSpeed);
        }
    }
}
