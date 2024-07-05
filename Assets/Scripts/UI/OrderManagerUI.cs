using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrderManagerUI : NetworkBehaviour
{
	[SyncVar(hook = nameof(AddCard))]
	RecipeSO recipe;

	[SerializeField] List<Transform> cardSlots;

	void Start()
	{
		OrderManager.OnRecipeAdded += AddNewRecipeToCard;
	}

	void OnDestroy()
	{
		OrderManager.OnRecipeAdded -= AddNewRecipeToCard;
	}

	public void AddNewRecipeToCard(RecipeSO recipe)
	{
		this.recipe = recipe;
	}

	void AddCard(RecipeSO recipeOld, RecipeSO recipe)
	{
		RecipeCardUI availableCard = cardSlots.FirstOrDefault(obj => obj.gameObject != null && !obj.gameObject.activeSelf).GetComponent<RecipeCardUI>();
		if (availableCard != null)
		{
			availableCard.gameObject.SetActive(true);
			availableCard.SetCard(recipe);
		}
	}
}
