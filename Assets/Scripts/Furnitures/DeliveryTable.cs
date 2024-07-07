using Mirror;
using UnityEngine;

public class DeliveryTable : Furniture
{
	[SyncVar(hook = nameof(Hook_SetItem)), HideInInspector] public GameObject droppedItem;

	public override void OnAction1()
	{
		base.OnAction1();


	}

	public void Hook_SetItem(GameObject oldValue, GameObject newValue)
	{

	}
}
