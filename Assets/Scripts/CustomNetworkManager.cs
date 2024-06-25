using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
	public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	{
		base.OnServerAddPlayer(conn);

		Debug.Log("A player joined !");
	}
}
