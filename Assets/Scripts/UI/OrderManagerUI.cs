using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrderManagerUI : NetworkBehaviour
{
	[SyncVar(hook = nameof(AddCard))]
	string recipeName;

	[SerializeField] List<Transform> cardSlots;

	Dictionary<string, RecipeSO> recipeSODictionary = new();

	void Start()
	{
		OrderManager.OnRecipeAdded += AddNewRecipeToCard;
		LoadAllRecipeSO();
	}

	void OnDestroy()
	{
		OrderManager.OnRecipeAdded -= AddNewRecipeToCard;
	}

	private void LoadAllRecipeSO()
	{
		RecipeSO[] recipes = Resources.LoadAll<RecipeSO>("Recipes");
		foreach (var recipe in recipes)
		{
			if (!recipeSODictionary.ContainsKey(recipe.shownName))
			{
				recipeSODictionary[recipe.shownName] = recipe;
			}
		}
	}

	public void AddNewRecipeToCard(RecipeSO recipe)
	{
		recipeName = recipe.shownName;
	}

	void AddCard(string recipeOld, string recipe)
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

	RecipeSO GetRecipeSO(string recipeSOName)
	{
		if (recipeSODictionary.TryGetValue(recipeSOName, out RecipeSO recipeSO))
		{
			return recipeSO;
		}

		Debug.LogError($"No recipe found with name {recipeSOName}");
		return null;
	}
}
