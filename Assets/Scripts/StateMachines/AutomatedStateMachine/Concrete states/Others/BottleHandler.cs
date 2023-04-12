using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BottleHandler : MonoBehaviour
{
    public event Action OnBottleCollision;

    public void EnableCollider(bool enabled)
    {
        GetComponent<Collider>().enabled = enabled;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ExtensionMethods.LayerMaskContainsLayer(other.gameObject, LayerMask.GetMask("Ground", "Default")))
        {
            //Debug.Log("Other collider: " + other.gameObject.name);
            OnBottleCollision?.Invoke();
        }
    }
}