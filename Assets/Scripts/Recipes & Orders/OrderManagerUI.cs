using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManagerUI : MonoBehaviour
{
    [SerializeField] Transform container;
    [SerializeField] Transform recipeCardPrefab;

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
        Instantiate(recipeCardPrefab, container);
    }

    public void RemoveCard()
    {

    }
}
