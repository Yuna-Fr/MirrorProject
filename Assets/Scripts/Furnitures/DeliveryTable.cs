using Mirror;
using UnityEngine;

public class DeliveryTable : Furniture
{
	[SyncVar(hook = nameof(Hook_AnalysePlate)), HideInInspector] public GameObject droppedPlate;

	public override void OnAction1(PlayerController player)
	{
		base.OnAction1(player);

		if (!player.IsHoldingPlate())
			return;

		droppedPlate = player.DropItemOnDeliveryTable();
	}

	public void Hook_AnalysePlate(GameObject oldPlate, GameObject newPlate)
	{
		OrderManager.Instance.DeliveryCheck(newPlate);
	}
}