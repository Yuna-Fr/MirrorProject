using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class OrderManager : NetworkBehaviour
{
	public static OrderManager Instance;

	public static UnityAction<RecipeSO> RecipeAdded;
	public static UnityAction<RecipeSO, bool> RecipeFinished;

	[SerializeField] int timerUpdateSec = 4;
	[SerializeField] int maxwaitingRecipes = 4;
	[SerializeField] List<RecipeSO> recipes;

	List<RecipeSO> waitingRecipes = new();

	void Start()
	{
		if (Instance == null)
			Instance = this;

		CustomNetworkManager.Instance.StartGame += OnStartTimer;
	}

	void OnDestroy()
	{
		if (isServer)
			RecipeFinished -= OnRecipeFinished;

		CustomNetworkManager.Instance.StartGame -= OnStartTimer;
	}

	public void DeliveryCheck(GameObject plateGO)
	{
		if (!isServer)
			return;

		Plate plate = plateGO.GetComponent<Plate>();

		if (!plate)
		{
			Debug.LogError($"No plate Componenet found in {plateGO}");
			return;
		}

		if (waitingRecipes.Count != 0)
		{
			List<ItemSO> ingredientsList = plate.GetItemsList();

			foreach (RecipeSO recipe in waitingRecipes)
			{
				if (recipe.ingredients.Count == ingredientsList.Count &&
					!recipe.ingredients.Except(ingredientsList).Any() && !ingredientsList.Except(recipe.ingredients).Any())
				{
					Debug.Log("Success Recipe, BONUS");
					plate.ResetVisuals();
					NetworkServer.Destroy(plateGO);
					RecipeFinished.Invoke(recipe, true);
					return;
				}
			}
		}

		Debug.Log("Fail Recipe, MALUS");
		plate.ResetVisuals();
		NetworkServer.Destroy(plateGO);
		RecipeFinished.Invoke(null, false);
	}

	void OnStartTimer()
	{
		if (!isServer)
			return;

		RecipeFinished += OnRecipeFinished;
		waitingRecipes = new();
		StartCoroutine(Timer());
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

	void OnRecipeFinished(RecipeSO recipe, bool withSuccess)
	{
		if (recipe)
			waitingRecipes.Remove(recipe);
	}

	IEnumerator Timer()
	{
		yield return new WaitForSeconds(timerUpdateSec);
		LaunchNewRecipe();
	}
}