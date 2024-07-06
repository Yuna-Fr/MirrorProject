using UnityEngine;

public class Furniture : MonoBehaviour
{
	[SerializeField] bool neverAvailable = false;
	[SerializeField] Transform itemContainer;

	GameObject itemContained = null;

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
