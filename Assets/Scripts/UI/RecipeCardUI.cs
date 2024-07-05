using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeCardUI : MonoBehaviour
{
	[Header("SETTINGS")]
	[SerializeField] float fadeDuration = 1;
	[SerializeField] Color greenColor = Color.green;
	[SerializeField] Color yellowColor = Color.yellow;
	[SerializeField] Color redColor = Color.red;

	[Header("REFERENCES")]
	[SerializeField] CanvasGroup canvasGroup;

	[SerializeField] Image timeBar;
	[SerializeField] TextMeshProUGUI textRecipe;
	[SerializeField] Image imageRecipe;

	[SerializeField] List<Transform> ingredientSlots;

	float maxTimeToFinish;
	float currentFillAmount = 60;

	public void SetCard(RecipeSO recipe)
	{
		canvasGroup.alpha = 0;
		maxTimeToFinish = recipe.maxTimeToFinish;
		currentFillAmount = recipe.maxTimeToFinish;

		if (recipe.sprite != null)
		{
			imageRecipe.gameObject.SetActive(true);
			imageRecipe.sprite = recipe.sprite;
		}
		else
		{
			textRecipe.gameObject.SetActive(true);
			textRecipe.text = recipe.name;
		}

		for (int i = 0; i < recipe.ingredients.Count; i++)
		{
			ingredientSlots[i].gameObject.SetActive(true);
			//ingredientSlots[i].GetComponent<Image>().sprite = recipe.ingredients[i].sprite;
		}

		canvasGroup.DOFade(1, fadeDuration);
	}

	void Update()
	{
		if (currentFillAmount > 0)
		{
			currentFillAmount -= Time.deltaTime;
			timeBar.fillAmount = (currentFillAmount) / (maxTimeToFinish);

			if (timeBar.fillAmount <= 0.6f)
			{
				if (timeBar.fillAmount <= 0.3f)
					timeBar.color = redColor;
				else
					timeBar.color = yellowColor;
			}
		}
		//else 
			//Destroy(gameObject);
	}
}