using Mirror;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class Plate : NetworkBehaviour
{
	[SerializeField] public bool isFakePlate = false;

	[Header("Visuals")]
	[SerializeField] GameObject completeVisual;
	[SerializeField] GameObject cheese;
	[SerializeField] GameObject salad;
	[SerializeField] GameObject tomato;

	List<ItemSO.ItemType> items = new();

	
}