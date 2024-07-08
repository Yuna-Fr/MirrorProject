using UnityEngine;

[CreateAssetMenu()]
public class ItemSO : ScriptableObject
{
	public ItemType itemType;
	public bool isComestible = false;
	public Sprite sprite;
	public Mesh mesh;
	public Material material;
	public ItemType nextItemType;

	public enum ItemType
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
		Plate,
	}
}