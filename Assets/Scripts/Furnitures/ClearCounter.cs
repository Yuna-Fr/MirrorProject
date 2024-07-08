using Mirror;
using UnityEngine;

public class ClearCounter : Furniture
{
	[SerializeField] Transform itemContainer;
	[SerializeField] MeshRenderer fakeItemVisual;
	[SerializeField] MeshFilter fakeItemVisualFilter;
	[SerializeField] Plate fakePlate;

	[SyncVar(hook = nameof(Hook_SetDroppedItem)), HideInInspector] public GameObject droppedItem;

	public override void OnAction1(PlayerController player)
	{
		base.OnAction1(player);

		if (!player.IsHoldingItem()) //Joueur main vide
		{
			if (droppedItem == null)
				return;
			else
				PlayerTakeItem(player);
		}
		else if (player.IsHoldingItem() && !player.IsHoldingPlate()) //Joueur a un ingrédient
		{
			if (droppedItem != null && droppedItem.GetComponent<Item>().GetItemSO().itemType == ItemSO.ItemType.Plate)
			{
				if(droppedItem.GetComponent<Plate>().TryAddItemOnPlate(player.takenItem.GetComponent<Item>()))
					player.TakeDropItemFromClearCounter(null);
			}
			else if (droppedItem != null)
				return;
			else
				PlayerDropItem(player);
		}
		else if (player.IsHoldingItem() && player.IsHoldingPlate()) //Joueur a une assiette
		{
			if (droppedItem == null)
				PlayerDropItem(player);

			else if (player.GetFakePlate().TryAddItemOnPlate(droppedItem.GetComponent<Item>()))
				RPC_SetDroppedItem(null);
		}
	}

	public override void OnAction2(PlayerController player)
	{
		base.OnAction2(player);
	}

	void PlayerTakeItem(PlayerController player)
	{
		player.TakeDropItemFromClearCounter(droppedItem);
		RPC_SetDroppedItem(null);
	}

	void PlayerDropItem(PlayerController player)
	{
		RPC_SetDroppedItem(player.takenItem);
		player.TakeDropItemFromClearCounter(null);
	}

	[Command(requiresAuthority = false)] void RPC_SetDroppedItem(GameObject item)
	{
		droppedItem = item;
	}

	public void Hook_SetDroppedItem(GameObject oldValue, GameObject newValue)
	{
		if (newValue != null)
		{
			Item item = newValue.GetComponent<Item>();
			fakeItemVisual.sharedMaterial = item.GetItemSO().material;
			fakeItemVisualFilter.sharedMesh = item.GetItemSO().mesh;

			if (item.itemType == ItemSO.ItemType.Plate)
				fakePlate.SetItemsVisuals(item.plateScript.GetItemsList());
		}
		
		if (oldValue != null && oldValue.GetComponent<Item>().itemType == ItemSO.ItemType.Plate)
			fakePlate.ResetVisuals();

		fakeItemVisual.enabled = (newValue == null) ? false : true;
	}
}