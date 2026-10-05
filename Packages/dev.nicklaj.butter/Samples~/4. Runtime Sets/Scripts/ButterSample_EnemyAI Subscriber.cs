using System;
using UnityEngine;

[RequireComponent(typeof(ButterSample_EnemyAI))]
public class ButterSample_EnemyAISubscriber : MonoBehaviour
{
    [SerializeField] private ButterSample_EnemyAIRuntimeSet _runtimeSet;
    [SerializeField] private ButterSample_EnemyAI _enemyAI;

    private void Awake() => _enemyAI ??= GetComponent<ButterSample_EnemyAI>();

    private void OnEnable()
    {
        _runtimeSet.Add(_enemyAI);
    }

    private void OnDisable()
    {
        _runtimeSet.Remove(_enemyAI);
    }
}
