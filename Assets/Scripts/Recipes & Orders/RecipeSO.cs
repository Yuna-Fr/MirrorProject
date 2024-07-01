using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu()]
public class RecipeSO : ScriptableObject
{
	[SerializeField] string name;
	[SerializeField] Image image;
	[SerializeField] List<IngredientSO> ingredients;
}
