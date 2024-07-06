using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Item : NetworkBehaviour
{
    Collider collider;
    MeshRenderer meshRenderer;
    Rigidbody rigidbody;

    [SyncVar(hook = nameof(IsTaken)), HideInInspector] public bool isTaken = false;


    private void Start()
    {
        collider = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (isServer)
            rigidbody = gameObject.AddComponent<Rigidbody>();
    }

    void IsTaken(bool oldValue, bool newValue)
    {
        collider.enabled = !newValue;
        meshRenderer.enabled = !newValue;

        if (isServer)
            rigidbody.isKinematic = newValue;
    }
}
