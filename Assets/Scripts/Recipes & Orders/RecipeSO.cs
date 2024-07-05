using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu()]
public class RecipeSO : ScriptableObject
{
	public string name;
	public Sprite sprite;
	public List<IngredientSO> ingredients;
	public float maxTimeToFinish;
}