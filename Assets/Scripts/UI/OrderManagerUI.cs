using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrderManagerUI : NetworkBehaviour
{
	[SyncVar(hook = nameof(Hook_AddCard))]
	string recipeName;

	[SerializeField] List<Transform> cardSlots;

	Dictionary<string, RecipeSO> allRecipeSODictionary = new();

	void Start()
	{
		OrderManager.RecipeAdded += AddNewRecipeToCard;
		OrderManager.RecipeFinished += RemoveRecipeCard;

		LoadAllRecipeSO();
	}

	void OnDestroy()
	{
		OrderManager.RecipeAdded -= AddNewRecipeToCard;
		OrderManager.RecipeFinished -= RemoveRecipeCard;
	}

	#region CardDiplay
	void AddNewRecipeToCard(RecipeSO recipe)
	{
		recipeName = recipe.shownName;
	}

	void RemoveRecipeCard(RecipeSO recipe, bool isSuccessful)
	{
		if (recipe == null) return;

		List<Transform> activeCards = cardSlots.Where(obj => obj != null && obj.gameObject.activeSelf).ToList();

		foreach (Transform card in activeCards)
		{
			RecipeCardUI recipeCard = card.gameObject.GetComponent<RecipeCardUI>();
			if (recipeCard != null && recipeCard.GetRecipeRef() == recipe)
			{
				if (isSuccessful)
					recipeCard.RecipeSucceedEffect();
				else
					recipeCard.RecipeFailedEffect();

				return;
			}
		}
	}

	void Hook_AddCard(string recipeOld, string recipe)
	{
		RecipeCardUI availableCard = null;
		Transform availableTransform = cardSlots.FirstOrDefault(obj => obj != null && !obj.gameObject.activeSelf);

		if (availableTransform != null)
		{
			availableCard = availableTransform.GetComponent<RecipeCardUI>();
		}

		if (availableCard != null)
		{
			availableCard.gameObject.SetActive(true);
			availableCard.SetCard(GetRecipeSO(recipeName));
		}
		else
		{
			Debug.LogWarning("No available card slot found");
		}
	}

	void LoadAllRecipeSO()
	{
		RecipeSO[] recipes = Resources.LoadAll<RecipeSO>("Recipes");
		foreach (var recipe in recipes)
		{
			if (!allRecipeSODictionary.ContainsKey(recipe.shownName))
			{
				allRecipeSODictionary[recipe.shownName] = recipe;
			}
		}
	}

	RecipeSO GetRecipeSO(string recipeSOName)
	{
		if (allRecipeSODictionary.TryGetValue(recipeSOName, out RecipeSO recipeSO))
		{
			return recipeSO;
		}

		Debug.LogError($"No recipe found with name {recipeSOName}");
		return null;
	}
	#endregion
}
