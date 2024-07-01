using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
	[SerializeField] List<RecipeSO> waitingRecipes;

	void OnStartTimer()
	{
		StartCoroutine(Timer());
	}

	void LaunchNewRecipe()
	{
		if (waitingRecipes.Count <= 0)
			return;

		//Affiche waitingRecipes[0]
		waitingRecipes.Remove(waitingRecipes[0]);
	}

	IEnumerator Timer()
	{
		yield return new WaitForSeconds(2);
		LaunchNewRecipe();
	}
}