using System;
using Dev.Nicklaj.Butter;
using UnityEngine;

public class PoolSample_ReturnOnCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        ReturnToPool rtp = this.GetComponent<ReturnToPool>();
        if (rtp != null && other.collider.tag == "Player")
        {
            Debug.Log("I got killed");
            rtp.Release();
        }
    }
}
