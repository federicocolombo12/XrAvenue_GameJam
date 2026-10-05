using System;
using UnityEngine;

public class ButterSample_EnemyAI : MonoBehaviour
{
    [SerializeField] private ButterSample_EnemyAIRuntimeSet _runtimeSet;

    private void OnEnable()
    {
        _runtimeSet.Add(this);
    }

    private void OnDisable()
    {
        _runtimeSet.Remove(this);
    }

    private void Awake()
    {
        // Do stuff on awake
    }

    private void Update()
    {
        // Do stuff on Update
    }
}
