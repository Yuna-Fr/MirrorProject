using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Item : NetworkBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] MeshFilter meshFilter;
    [SerializeField] Collider collider;

    ItemSO.ItemType itemType;
    ItemSO itemSO;
    Rigidbody rigidbody;

    [SyncVar(hook = nameof(Hook_IsTaken)), HideInInspector] public bool isTaken = false;


    private void Start()
    {
        SetItem(ItemSO.ItemType.Tomato);

        if (isServer)
            rigidbody = gameObject.AddComponent<Rigidbody>();
    }

    public void SetItem(ItemSO.ItemType itemType)
    {
        this.itemType = itemType;
        itemSO = Resources.Load<ItemSO>($"Items/{itemType}");
        meshRenderer.sharedMaterial = itemSO.material;
        meshFilter.sharedMesh = itemSO.mesh;
        ((MeshCollider)collider).sharedMesh = meshFilter.mesh;
    }

    public ItemSO GetItemSO()
    {
        return itemSO;
    }

    void Hook_IsTaken(bool oldValue, bool newValue)
    {
        collider.enabled = !newValue;
        meshRenderer.enabled = !newValue;

        if (isServer)
            rigidbody.isKinematic = newValue;
    }
}
