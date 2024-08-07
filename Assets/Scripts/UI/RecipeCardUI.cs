using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipeCardUI : MonoBehaviour
{
	[Header("SETTINGS")]
	[SerializeField] float shakeDuration = 1;
	[SerializeField] float fadeDuration = 1;
	[SerializeField] Color greenColor = Color.green;
	[SerializeField] Color yellowColor = Color.yellow;
	[SerializeField] Color redColor = Color.red;
	[SerializeField] Color originalCardColor;

	[Header("REFERENCES")]
	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] Image cardBackground;

	[SerializeField] Image timeBar;
	[SerializeField] TextMeshProUGUI textRecipe;
	[SerializeField] Image imageRecipe;

	[SerializeField] List<Image> ingredientSlots;

	RecipeSO recipeRef = null;
	float maxTimeToFinish;
	float currentFillAmount = 60;
	bool alreadyFinishing = false;

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
		else
			RecipeFailedEffect();
	}

	public void SetCard(RecipeSO recipe)
	{
		canvasGroup.alpha = 0;
		recipeRef = recipe;
		maxTimeToFinish = recipe.maxTimeToFinish;
		currentFillAmount = recipe.maxTimeToFinish;
		cardBackground.color = originalCardColor;
		timeBar.color = greenColor;

		if (recipe.sprite != null)
		{
			imageRecipe.gameObject.SetActive(true);
			imageRecipe.sprite = recipe.sprite;
		}
		else
		{
			textRecipe.gameObject.SetActive(true);
			textRecipe.text = recipe.shownName;
		}

		for (int i = 0; i < recipe.ingredients.Count; i++)
		{
			ingredientSlots[i].gameObject.SetActive(true);
			ingredientSlots[i].sprite = recipe.ingredients[i].sprite;
		}

		canvasGroup.DOFade(1, fadeDuration);
	}

	public RecipeSO GetRecipeRef() { return recipeRef; }

	public void RecipeSucceedEffect()
	{
		cardBackground.DOColor(greenColor, shakeDuration / 2);
		canvasGroup.DOFade(0, fadeDuration)
			.OnComplete(() => { CloseRecipeCard(); });
	}

	public void RecipeFailedEffect()
	{
		if (alreadyFinishing == true)
			return;

		alreadyFinishing = true;
		cardBackground.DOColor(redColor, shakeDuration / 2);
		canvasGroup.DOFade(1, fadeDuration);
		transform.DOShakePosition(shakeDuration)
			.OnComplete(() => { CloseRecipeCard(); });
	}

	void CloseRecipeCard()
	{
		OrderManager.RecipeFinished.Invoke(recipeRef, false);
		alreadyFinishing = false;
		gameObject.SetActive(false);
	}
}