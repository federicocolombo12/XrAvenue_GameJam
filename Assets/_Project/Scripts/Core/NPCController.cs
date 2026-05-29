using UnityEngine;
using System;
using System.Collections;

namespace AvenueXR.Core
{
    public class NPCController : MonoBehaviour
    {
        public Transform deskPosition;
        public Transform startPosition;
        public float moveSpeed = 2.0f;
        
        private bool _isDelivering = false;

        public void DeliverObject(WasteType type, Action onArrivalAtDesk)
        {
            StartCoroutine(DeliveryRoutine(onArrivalAtDesk));
        }

        private IEnumerator DeliveryRoutine(Action onArrival)
        {
            _isDelivering = true;

            // 1. Cammina verso la scrivania
            yield return StartCoroutine(MoveTo(deskPosition.position));

            // 2. Arrivato! Notifica il manager per far apparire l'oggetto
            onArrival?.Invoke();
            
            // 3. Aspetta un attimo (animazione di posa oggetto)
            yield return new WaitForSeconds(0.5f);

            // 4. Torna indietro
            yield return StartCoroutine(MoveTo(startPosition.position));

            _isDelivering = false;
        }

        private IEnumerator MoveTo(Vector3 target)
        {
            while (Vector3.Distance(transform.position, target) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                // Qui potresti ruotare l'NPC verso il target
                transform.LookAt(target);
                yield return null;
            }
        }
    }
}
