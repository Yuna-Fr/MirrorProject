using Mirror;
using UnityEngine;

public class Item : NetworkBehaviour, ICollisionHandler
{
    [SerializeField] public Plate plateScript;
	[SerializeField] MeshRenderer meshRenderer;
	[SerializeField] MeshFilter meshFilter;
	[SerializeField] Collider collider;

    ItemSO itemSO;
    Rigidbody rigidBody;
	NetworkIdentity networkIdentdity;

    [SyncVar(hook = nameof(Hook_IsTaken)), HideInInspector] public bool isTaken = false;
	[SyncVar(hook = nameof(Hook_SetItem)), HideInInspector] public ItemSO.ItemType itemType;

	private void Start()
	{
		itemType = ItemSO.ItemType.Tomato;

		networkIdentdity = GetComponent<NetworkIdentity>();

		if (isServer)
		{
            rigidBody = gameObject.AddComponent<Rigidbody>();
			//rigidBody.isKinematic = true;
        }
	}

    public NetworkIdentity GetNetworkIdentity()
	{
		return networkIdentdity;
	}

    #region Autodisplay

    public void SetItem(ItemSO.ItemType itemType)
	{
		this.itemType = itemType;
		itemSO = Resources.Load<ItemSO>($"Items/{itemType}");
		meshRenderer.sharedMaterial = itemSO.material;
		meshFilter.sharedMesh = itemSO.mesh;
		(collider as MeshCollider).sharedMesh = meshFilter.mesh;
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
			rigidBody.isKinematic = newValue;

		if (itemType == ItemSO.ItemType.Plate)
		{
			
        }
	}

	void Hook_SetItem(ItemSO.ItemType oldValue, ItemSO.ItemType newValue)
	{
		SetItem(newValue);
	}

    #endregion

    #region Network collisions

    public void OnCollisionReaction(Vector3 direction, float strength, bool isImpulsion, NetworkIdentity savedTarget)
	{
		Cmd_OnCollisionReaction(direction, strength, isImpulsion, savedTarget);
    }

	[Command(requiresAuthority = false)]
	void Cmd_OnCollisionReaction(Vector3 direction, float strength, bool isImpulsion, NetworkIdentity savedTarget)
	{
        ForceMode forceMode = isImpulsion ? ForceMode.Impulse : ForceMode.Force;

        direction = (direction.y < 0) ? new Vector3(direction.x, 0, direction.z) : direction;

        rigidBody.AddForce(direction * strength, forceMode);

		TargetRpc_CollisionHasBeenHandled(savedTarget.connectionToClient, savedTarget, networkIdentdity.netId);
    }

	[TargetRpc]
	void TargetRpc_CollisionHasBeenHandled(NetworkConnection networkConnection, NetworkIdentity savedTarget, uint id)
	{
        savedTarget.GetComponent<PlayerController>().GetOnCollisionIds()[id] = false;
    }

    #endregion
}
