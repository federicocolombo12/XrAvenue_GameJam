using UnityEngine;
using UnityEngine.InputSystem;
using AvenueXR.Core;

namespace AvenueXR.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Raycast Settings")]
        public Camera playerCamera;
        public float reachDistance = 3.0f;
        public LayerMask interactionLayers = ~0; // Tutti i layer per default

        [Header("Carrying Settings")]
        public Transform holdPoint;
        public float carrySmoothSpeed = 16f;
        public float softDropForce = 2.5f;
        public float throwForce = 7.5f;

        [Header("Crank Smash Settings")]
        [Tooltip("Gradi di rotazione impartiti ad ogni pressione (tasto Spazio / Click).")]
        public float crankImpulsePerSmash = 55f;

        private WasteItem _carriedItem;
        private Rigidbody _carriedRb;
        private XRPhysicalCrank _hoveredCrank;
        private BinCrusher _hoveredCrusher;

        public bool IsCarrying => _carriedItem != null;
        public GameObject CurrentHoverObject { get; private set; }
        public bool IsHoveringInteractable { get; private set; }

        // Dati contestuali per l'HUD (CrosshairUI)
        public string CurrentActionPrompt { get; private set; } = string.Empty;
        public float ActiveCrushProgress { get; private set; } = 0f;
        public bool HasActiveProgress { get; private set; } = false;

        private void Awake()
        {
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null) playerCamera = Camera.main;
            }

            if (holdPoint == null && playerCamera != null)
            {
                // Crea holdPoint centrato e ribassato per una visuale pulita sul mirino
                GameObject hp = new GameObject("DefaultHoldPoint");
                hp.transform.SetParent(playerCamera.transform);
                hp.transform.localPosition = new Vector3(0.0f, -0.28f, 0.65f);
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
            _hoveredCrank = null;
            _hoveredCrusher = null;

            if (Physics.Raycast(ray, out hit, reachDistance, interactionLayers))
            {
                CurrentHoverObject = hit.collider.gameObject;

                WasteItem item = hit.collider.GetComponentInParent<WasteItem>();
                _hoveredCrank = hit.collider.GetComponentInParent<XRPhysicalCrank>();
                _hoveredCrusher = hit.collider.GetComponentInParent<BinCrusher>();

                // Se stiamo mirando il cestino, prendiamo la sua manovella associata
                if (_hoveredCrank == null && _hoveredCrusher != null)
                {
                    _hoveredCrank = _hoveredCrusher.targetCrank;
                }
                else if (_hoveredCrusher == null && _hoveredCrank != null)
                {
                    _hoveredCrusher = _hoveredCrank.GetComponentInParent<BinCrusher>();
                    if (_hoveredCrusher == null)
                    {
                        // Cerca tra i BinCrusher di scena quello che usa questa crank
                        BinCrusher[] allCrushers = Object.FindObjectsByType<BinCrusher>(FindObjectsSortMode.None);
                        foreach (var c in allCrushers)
                        {
                            if (c.targetCrank == _hoveredCrank)
                            {
                                _hoveredCrusher = c;
                                break;
                            }
                        }
                    }
                }

                if (item != null || _hoveredCrank != null || _hoveredCrusher != null)
                {
                    IsHoveringInteractable = true;
                }
            }

            // Aggiornamento prompt e barre UI
            UpdateContextualPrompts();
        }

        private void UpdateContextualPrompts()
        {
            if (IsCarrying)
            {
                CurrentActionPrompt = "[Click SX / E] Deposita   •   [Click DX] Lancia";
                HasActiveProgress = false;
                ActiveCrushProgress = 0f;
            }
            else if (_hoveredCrank != null || _hoveredCrusher != null)
            {
                if (_hoveredCrusher != null && _hoveredCrusher.IsPending)
                {
                    CurrentActionPrompt = "[SPAZIO] Premi a raffica per tritare!";
                    HasActiveProgress = true;
                    ActiveCrushProgress = _hoveredCrusher.CrushProgress;
                }
                else
                {
                    CurrentActionPrompt = "[SPAZIO] Gira manovella";
                    HasActiveProgress = false;
                    ActiveCrushProgress = 0f;
                }
            }
            else if (CurrentHoverObject != null && CurrentHoverObject.GetComponentInParent<WasteItem>() != null)
            {
                CurrentActionPrompt = "[E / Click SX] Raccogli";
                HasActiveProgress = false;
                ActiveCrushProgress = 0f;
            }
            else
            {
                CurrentActionPrompt = string.Empty;
                HasActiveProgress = false;
                ActiveCrushProgress = 0f;
            }
        }

        private void HandleInput()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;

            bool interactPressed = (keyboard != null && keyboard.eKey.wasPressedThisFrame) ||
                                   (mouse != null && mouse.leftButton.wasPressedThisFrame);

            bool smashPressed = (keyboard != null && keyboard.spaceKey.wasPressedThisFrame) ||
                                interactPressed;

            bool throwPressed = mouse != null && mouse.rightButton.wasPressedThisFrame;

            // 1. GESTIONE OGGETTO TRASPORTATO
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

            // 2. GESTIONE SMASH MANOVELLA
            if (_hoveredCrank != null && smashPressed)
            {
                _hoveredCrank.ApplyImpulse(crankImpulsePerSmash);
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

            Vector3 dropVelocity = Vector3.zero;
            if (playerCamera != null)
            {
                // Soft Drop mirato verso il punto esatto del mirino (apertura cestino / ripiano tavolo)
                Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                Vector3 targetPoint;
                if (Physics.Raycast(ray, out RaycastHit hit, 4.0f, interactionLayers))
                {
                    targetPoint = hit.point;
                }
                else
                {
                    targetPoint = ray.GetPoint(2.5f);
                }

                Vector3 direction = (targetPoint - _carriedItem.transform.position).normalized;
                dropVelocity = (direction + Vector3.up * 0.15f).normalized * softDropForce;
            }

            if (_carriedRb != null)
            {
                _carriedRb.isKinematic = false;
                _carriedRb.useGravity = true;
                _carriedRb.linearVelocity = dropVelocity;
                _carriedRb.angularVelocity = Vector3.zero;
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
