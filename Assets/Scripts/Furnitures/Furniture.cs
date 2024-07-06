using UnityEngine;
using static IngredientSO;

public class Furniture : MonoBehaviour
{
	[SerializeField] bool neverAvailable = false;
	[SerializeField] Transform itemContainer;

	Ingredients itemContained = Ingredients.None;

	public bool IsAvailable()
	{
		if (!neverAvailable || itemContained != Ingredients.None)
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
