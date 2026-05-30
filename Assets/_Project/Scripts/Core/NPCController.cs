using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AvenueXR.Core
{
    public enum NPCState { Idle, Walking, Interacting }

    public class NPCController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 2.0f;
        public float rotationSpeed = 5.0f;
        public float arrivalThreshold = 0.1f;
        public float interactionDuration = 1.0f;

        [Header("Path Waypoints")]
        public List<Transform> waypoints = new List<Transform>();

        // --- Animation Hooks ---
        public event Action<NPCState> OnStateChanged;
        public event Action<float> OnSpeedChanged; // Passa la velocità attuale (0 se fermo, moveSpeed se cammina)

        public NPCState CurrentState { get; private set; } = NPCState.Idle;
        public float CurrentSpeed { get; private set; } = 0f;
        
        private bool _isMoving = false;

        public void DeliverObject(WasteType type, Action onArrivalAtDesk)
        {
            if (CurrentState != NPCState.Idle) return;
            if (waypoints == null || waypoints.Count < 2)
            {
                Debug.LogWarning("[NPCController] Waypoints list is empty or has less than 2 points.");
                onArrivalAtDesk?.Invoke();
                return;
            }

            StartCoroutine(DeliveryRoutine(onArrivalAtDesk));
        }

        private IEnumerator DeliveryRoutine(Action onArrival)
        {
            // 1. Inizia a camminare verso la scrivania
            UpdateState(NPCState.Walking);
            for (int i = 1; i < waypoints.Count; i++)
            {
                yield return StartCoroutine(MoveTo(waypoints[i].position));
            }

            // 2. Arrivato alla scrivania -> Inizia Interazione
            UpdateState(NPCState.Interacting);
            onArrival?.Invoke();
            
            // Aspetta il tempo dell'animazione di consegna
            yield return new WaitForSeconds(interactionDuration);

            // 3. Torna indietro
            UpdateState(NPCState.Walking);
            for (int i = waypoints.Count - 2; i >= 0; i--)
            {
                yield return StartCoroutine(MoveTo(waypoints[i].position));
            }

            // 4. Finito!
            UpdateState(NPCState.Idle);
        }

        private IEnumerator MoveTo(Vector3 target)
        {
            _isMoving = true;
            UpdateSpeed(moveSpeed);

            while (Vector3.Distance(transform.position, target) > arrivalThreshold)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                
                Vector3 direction = (target - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }

                yield return null;
            }
            
            transform.position = target;
            _isMoving = false;
            UpdateSpeed(0f);
        }

        private void UpdateState(NPCState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
            
            // Se entriamo in uno stato non-walking, la velocità è forzata a 0
            if (newState != NPCState.Walking) UpdateSpeed(0f);
        }

        private void UpdateSpeed(float speed)
        {
            CurrentSpeed = speed;
            OnSpeedChanged?.Invoke(speed);
        }

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Count == 0) return;
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawSphere(waypoints[i].position, 0.2f);
                if (i < waypoints.Count - 1 && waypoints[i+1] != null)
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
            }
        }
    }
}
