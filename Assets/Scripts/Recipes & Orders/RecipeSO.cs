using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu()]
public class RecipeSO : ScriptableObject
{
	public string name;
	public Image image;
	public List<IngredientSO> ingredients;
}