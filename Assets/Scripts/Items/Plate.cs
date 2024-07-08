using Mirror;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Plate : NetworkBehaviour
{
	[SerializeField] public bool isFakePlate = false;

	[Header("Visuals")]
	[SerializeField] GameObject completeVisual;
	[SerializeField] GameObject bread;
	[SerializeField] GameObject cheese;
	[SerializeField] GameObject salad;
	[SerializeField] GameObject tomato;

	[SyncVar(hook = nameof(Hook_ItemList))] List<ItemSO.ItemType> items = new();

	public void ResetVisuals()
	{
		bread.SetActive(false);
		cheese.SetActive(false);
		salad.SetActive(false);
		tomato.SetActive(false);
    }

	public List<ItemSO.ItemType> GetItemsList()
	{
		return items;
	}

	public List<ItemSO> GetItemSOList()
	{
		List<ItemSO> itemsSO = new();

		foreach (ItemSO.ItemType item in items)
			itemsSO.Add(Resources.Load<ItemSO>($"Items/{item}"));

		return itemsSO;
	}

	public void AddItemInPlate(ItemSO.ItemType addedItem)
    {
		List<ItemSO.ItemType> newList = new();

		items.ForEach(item => newList.Add(item));

		newList.Add(addedItem);

		RPC_UpdatePlateVisual(newList);
    }

    public bool CanAddItemOnPlate(Item item)
	{
		ItemSO itemSO = item.GetItemSO();

		if (items.Contains(item.itemType) || !itemSO.isComestible)
			return false;

		return true;
	}

	public void SetItemsVisuals(List<ItemSO.ItemType> itemList)
	{
		items = itemList;

		if (items.Count == 0)
			ResetVisuals();
		else
			foreach (ItemSO.ItemType item in items)
				SetItemVisual(item);
	}

	public void ActiveCompleteVisual(bool isActive)
	{
		completeVisual.SetActive(isActive);
	}

	public void SetItemVisual(ItemSO.ItemType itemType)
	{
		GameObject target = itemType switch
		{
			ItemSO.ItemType.Bread => bread,
			ItemSO.ItemType.CheeseCut => cheese,
			ItemSO.ItemType.SaladCut => salad,
			ItemSO.ItemType.TomatoCut => tomato,
			_ => null,
		};

		if (target)
			target.SetActive(true);
	}

	[Command(requiresAuthority = false)]
	void RPC_UpdatePlateVisual(List<ItemSO.ItemType> newList)
	{
        items = newList;
    }

	void Hook_ItemList(List<ItemSO.ItemType> oldValue, List<ItemSO.ItemType> newValue)
	{
		SetItemsVisuals(newValue);
	}
}