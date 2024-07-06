using UnityEngine;

[CreateAssetMenu()]
public class IngredientSO : ScriptableObject
{
	public Ingredients ingredientType;
	public Sprite sprite;

	public enum Ingredients
	{
		None,
		Bread,
		Cheese,
		CheeseCut,
		Salad,
		SaladCut,
		Steak,
		SteakCooked,
		SteakBurned,
		Tomato,
		TomatoCut,
	}
}