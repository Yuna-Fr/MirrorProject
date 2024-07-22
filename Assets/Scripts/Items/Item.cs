using Mirror;
using UnityEngine;

public class Item : NetworkBehaviour, ICollisionHandler
{
	[Header("INTERACTIONS")]
    [SerializeField] public Plate plateScript;
	[SerializeField] MeshRenderer meshRenderer;
	[SerializeField] MeshFilter meshFilter;
	[SerializeField] Collider collider;

	[Header("NET COLLISIONS")]
	[SerializeField] float dashCollisionNerf = 0.1f;
	[SerializeField] float rigidbodyMass = 0.5f;

    ItemSO itemSO;
    Rigidbody rigidBody;
	NetworkIdentity networkIdentdity;

    [SyncVar(hook = nameof(Hook_IsTaken)), HideInInspector] public bool isTaken = false;
	[SyncVar(hook = nameof(Hook_SetItem)), HideInInspector] public ItemSO.ItemType itemType;

	private void Start()
	{
		networkIdentdity = GetComponent<NetworkIdentity>();

		if (isServer)
		{
            rigidBody = gameObject.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
            rigidBody.mass = rigidbodyMass;
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

		strength = isImpulsion ? strength * dashCollisionNerf : strength;

		rigidBody.AddForce(direction * strength * (isImpulsion ? 1.0f : 2.0f), forceMode);

		TargetRpc_CollisionHasBeenHandled(savedTarget.connectionToClient, savedTarget, networkIdentdity.netId, isImpulsion);
    }

	[TargetRpc]
	void TargetRpc_CollisionHasBeenHandled(NetworkConnection networkConnection, NetworkIdentity savedTarget, uint id, bool wasImpulsion)
	{
		if (wasImpulsion)
		{
            savedTarget.GetComponent<PlayerController>().GetOnDashCollisionIds()[id] = false;
            return;
		}

        savedTarget.GetComponent<PlayerController>().GetOnCollisionIds()[id]--;
    }

    #endregion
}
