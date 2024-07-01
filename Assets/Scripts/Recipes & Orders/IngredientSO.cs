using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class IngredientSO : ScriptableObject
{
	[SerializeField] string name;
	[SerializeField] Sprite image;
}
