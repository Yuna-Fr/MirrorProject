using UnityEngine;

[CreateAssetMenu()]
public class IngredientSO : ScriptableObject
{
	public Ingredients ingredientType;
	public Sprite sprite;
	public Mesh mesh;
	public Material material;

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