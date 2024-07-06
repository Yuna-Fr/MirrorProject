using Mirror;
using UnityEngine;

public class Furniture : NetworkBehaviour
{
	public GameObject itemContained = null;

	[SerializeField] bool neverAvailable = false;
	[SerializeField] Transform itemContainer;

	public bool IsAvailable()
	{
		if (!neverAvailable || itemContained != null)
			return false;

		return true;
	}

	public Transform GetItemContainer()
	{
		if (IsAvailable())
			return itemContainer;

		return null;
	}

}
