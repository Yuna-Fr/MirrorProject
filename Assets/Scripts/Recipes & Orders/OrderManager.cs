using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OrderManager : NetworkBehaviour
{
	public static UnityAction<RecipeSO> RecipeAdded;
	public static UnityAction<RecipeSO, bool> RecipeFinished;

	[SerializeField] int timerUpdateSec = 4;
	[SerializeField] int maxwaitingRecipes = 4;
	[SerializeField] List<RecipeSO> recipes;

	List<RecipeSO> waitingRecipes;

	void Start()
	{
		CustomNetworkManager.Instance.StartGame += OnStartTimer;
	}

	void OnDestroy()
	{
		if (isServer)
			RecipeFinished -= OnRecipeFinished;

		CustomNetworkManager.Instance.StartGame -= OnStartTimer;
	}

	void OnStartTimer()
	{
		if (!isServer)
			return;

		RecipeFinished += OnRecipeFinished;
		waitingRecipes = new();
		StartCoroutine(Timer());
	}

	void OnRecipeFinished(RecipeSO recipe, bool withSuccess)
	{
		waitingRecipes.Remove(recipe);
	}

	void LaunchNewRecipe()
	{
		if (recipes.Count <= 0)
			return;

		if (waitingRecipes.Count < maxwaitingRecipes)
		{
			var newRecipe = recipes[Random.Range(0, recipes.Count)];
			waitingRecipes.Add(newRecipe);
			RecipeAdded.Invoke(newRecipe);
		}

		StartCoroutine(Timer());
	}

	IEnumerator Timer()
	{
		yield return new WaitForSeconds(timerUpdateSec);
		LaunchNewRecipe();
	}

	//public void DeliveryCheck(string plate)//add real paramater
	//{
	//foreach (RecipeSO recipe in waitingRecipes)
	//{
	//	if (recipe.ingredients.Count == plate.ingredients.Count)
	//	{
	//		foreach (IngredientSO ingredient in recipe.ingredients)
	//		{
	//			if (!plate.ingredients.Contains(ingredient))
	//			{
	//				Debug.Log($"the {ingredient} was not found on the plate !");
	//				break;
	//			}
	//		}


	//	}
	//}
	//}
}