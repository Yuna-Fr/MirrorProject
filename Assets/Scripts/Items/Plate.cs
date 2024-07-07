using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour
{
	[Header("Visuals")]
	[SerializeField] GameObject completeVisual;
	[SerializeField] GameObject bread;
	[SerializeField] GameObject cheese;
	[SerializeField] GameObject salad;
	[SerializeField] GameObject tomato;

	List<ItemSO> items = new();

	public void Reset()
	{
		items.Clear();
		completeVisual.SetActive(false);
		bread.SetActive(false);
		cheese.SetActive(false);
		salad.SetActive(false);
		tomato.SetActive(false);
	}

	public List<ItemSO> GetItemsList()
	{
		return items;
	}

	public bool TryAddItemOnPlate(Item item)
	{
		ItemSO itemSO = item.GetItemSO();

		if (items.Contains(itemSO) || !itemSO.isComestible)
			return false;

		SetItemVisual(itemSO.itemType);
		items.Add(itemSO);

		return true;
	}

	public void SetItemsVisuals(List<ItemSO> itemList)
	{
		items = itemList;

		if (items.Count == 0)
			Reset();
		else
		{
			completeVisual.SetActive(true);

			foreach (ItemSO item in items)
				SetItemVisual(item.itemType);
		}
	}

	void SetItemVisual(ItemSO.ItemType itemType)
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
