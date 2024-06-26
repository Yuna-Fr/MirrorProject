using Mirror;
using UnityEngine;

public class PlayerNetworkSetUp : NetworkBehaviour
{
	[SerializeField] Behaviour[] componentsToDisable;

	void Start()
	{
		if (isLocalPlayer)
			return;

		for (int i = 0; i < componentsToDisable.Length; i++)
			componentsToDisable[i].enabled = false;
	}
}
