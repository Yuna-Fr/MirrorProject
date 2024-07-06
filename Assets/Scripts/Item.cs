using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Item : NetworkBehaviour
{
    IngredientSO.Ingredients ingredient;
    IngredientSO ingredientSO;
    Collider collider;
    MeshRenderer meshRenderer;
    Rigidbody rigidbody;

    [SyncVar(hook = nameof(Hook_IsTaken)), HideInInspector] public bool isTaken = false;


    private void Start()
    {
        if (isServer)
            rigidbody = gameObject.AddComponent<Rigidbody>();
    }

    public void SetItem(IngredientSO.Ingredients ingredient)
    {
        this.ingredient = ingredient;
        Resources.
    }

    void Hook_IsTaken(bool oldValue, bool newValue)
    {
        collider.enabled = !newValue;
        meshRenderer.enabled = !newValue;

        if (isServer)
            rigidbody.isKinematic = newValue;
    }
}
