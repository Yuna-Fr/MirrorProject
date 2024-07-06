using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class RecipeSO : ScriptableObject
{
	public string shownName;
	public Sprite sprite;
	public List<IngredientSO> ingredients;
	public float maxTimeToFinish;
}