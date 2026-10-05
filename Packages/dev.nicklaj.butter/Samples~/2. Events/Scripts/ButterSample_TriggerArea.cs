using System;
using Dev.Nicklaj.Butter;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ButterSample_TriggerArea : MonoBehaviour
{
    [SerializeField] private IntEvent _triggeredEvent;
    public int MaxEntities = 1;
    [SerializeField] private int _currentEntities = 0;

    private void OnTriggerEnter(Collider other)
    {
        if((++_currentEntities) >= MaxEntities) 
            _triggeredEvent.Raise(_currentEntities);
    }
}
