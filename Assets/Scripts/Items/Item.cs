using Mirror;
using UnityEngine;

public class Item : NetworkBehaviour
{
	[SerializeField] MeshRenderer meshRenderer;
	[SerializeField] MeshFilter meshFilter;
	[SerializeField] Collider collider;
	[SerializeField] Plate plateScript;

	ItemSO itemSO;
	Rigidbody rigidBody;

	[SyncVar(hook = nameof(Hook_IsTaken)), HideInInspector] public bool isTaken = false;
	[SyncVar(hook = nameof(Hook_SetItem)), HideInInspector] public ItemSO.ItemType itemType;

	private void Start()
	{
		if (isServer)
			rigidBody = gameObject.AddComponent<Rigidbody>();
	}

	public void SetItem(ItemSO.ItemType itemType)
	{
		this.itemType = itemType;
		itemSO = Resources.Load<ItemSO>($"Items/{itemType}");
		meshRenderer.sharedMaterial = itemSO.material;
		meshFilter.sharedMesh = itemSO.mesh;
		(collider as MeshCollider).sharedMesh = meshFilter.mesh;

		if (itemType == ItemSO.ItemType.Plate)
		{
			plateScript.enabled = true;
			plateScript.ShowVisuals(true);
		}
		else if (plateScript.enabled)
		{
			plateScript.Reset();
			plateScript.enabled = false;
		}
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
	}

	void Hook_SetItem(ItemSO.ItemType oldValue, ItemSO.ItemType newValue)
	{
		SetItem(newValue);
	}
}
