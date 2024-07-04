using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeCardUI : MonoBehaviour
{
	[Header("SETTINGS")]
	[SerializeField] float fadeDuration = 1;

	[Header("REFERENCES")]
	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] TextMeshProUGUI textRecipe;
	[SerializeField] Image imageRecipe;
	[SerializeField] Transform ingredientContainer;
	[SerializeField] Image ingredientPrefab;

	public void SetCard(RecipeSO recipe)
	{
		canvasGroup.alpha = 0;

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
}