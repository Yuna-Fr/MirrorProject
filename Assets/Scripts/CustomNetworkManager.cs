using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomNetworkManager : NetworkManager
{
	public static CustomNetworkManager Instance;

	public UnityAction StartGame;
	public UnityAction PauseGame;

	public override void Awake()
	{
		base.Awake();

		if (Instance == null)
			Instance = this;
	}

	public override void OnServerAddPlayer(NetworkConnectionToClient conn)
	{
		base.OnServerAddPlayer(conn);
		
		//if (numPlayers == 2) // replace this with UI and ready button
			StartGame.Invoke();

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