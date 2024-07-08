using Mirror;
using System.Collections.Generic;
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

	List<ItemSO> items = new();

	public void ResetVisuals()
	{
		bread.SetActive(false);
		cheese.SetActive(false);
		salad.SetActive(false);
		tomato.SetActive(false);
    }

	public List<ItemSO> GetItemsList()
	{
		return items;
	}

	public void AddItemInPlate(Item itemToAdd, Item plateItem)
	{
		SetItemVisual(itemToAdd.itemType);
		Plate plate = plateItem.GetComponent<Plate>();
        plate.SetItemVisual(itemToAdd.itemType);
		plate.GetItemsList().Add(itemToAdd.GetItemSO());
		Destroy(itemToAdd.gameObject);
	}

	public bool TryAddItemOnPlate(Item item)
	{
		ItemSO itemSO = item.GetItemSO();

		if (items.Contains(itemSO) || !itemSO.isComestible)
			return false;

		//completeVisual.SetActive(true);
		//SetItemVisual(itemSO.itemType);
		//items.Add(itemSO);

		return true;
	}

	public void SetItemsVisuals(List<ItemSO> itemList)
	{
		items = itemList;

		if (items.Count == 0)
			ResetVisuals();
		else
		{
			completeVisual.SetActive(true);

			foreach (ItemSO item in items)
				SetItemVisual(item.itemType);
		}
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
}