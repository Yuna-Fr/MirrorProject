using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

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

	[SerializeField] Transform ingredientContainer;
	[SerializeField] Image ingredientPrefab;

	float maxTimeToFinish;
	float currentFillAmount = 60;

	public void SetCard(RecipeSO recipe)
	{
		canvasGroup.alpha = 0;
		maxTimeToFinish = recipe.maxTimeToFinish;
		currentFillAmount = recipe.maxTimeToFinish;

		if (recipe.image != null)
		{
			imageRecipe.gameObject.SetActive(true);
			imageRecipe = recipe.image;
		}
		else
		{
			textRecipe.gameObject.SetActive(true);
			textRecipe.text = recipe.name;
		}

		foreach (IngredientSO ingredient in recipe.ingredients)
		{
			Instantiate(ingredientPrefab, ingredientContainer).sprite = ingredient.image;
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