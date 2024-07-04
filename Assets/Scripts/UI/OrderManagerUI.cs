using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManagerUI : MonoBehaviour
{
    [SerializeField] Transform container;
    [SerializeField] RecipeCardUI recipeCardPrefab;

	void Start()
	{
        OrderManager.OnRecipeAdded += AddCard;
	}

    void OnDestroy()
	{
        OrderManager.OnRecipeAdded -= AddCard;
	}

	public void AddCard(RecipeSO recipe)
    {
        if(recipe)

        Instantiate(recipeCardPrefab, container).SetCard(recipe);
    }

    public void RemoveCard()
    {

    }
}
