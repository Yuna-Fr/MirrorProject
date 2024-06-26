using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
	[Header("CUSTOM")]
	[SerializeField] List<Transform> spawnList;

	public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	{
		base.OnServerAddPlayer(conn);
		
		//Transform spawn = null;

		//if (spawnList.Count > 0)
		//{
		//	if (numPlayers <= spawnList.Count)
		//		spawn = spawnList[numPlayers - 1];
		//	else
		//		spawn = spawnList[0];
		//}

		//GameObject player = spawn ? Instantiate(playerPrefab, spawn.position, spawn.rotation) : Instantiate(playerPrefab);

		//player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";//Change "Player(clone)" => to connectionId (for debugging)
		//NetworkServer.AddPlayerForConnection(conn, player);
	}
}