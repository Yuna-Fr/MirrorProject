using Mirror;
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

		if (numPlayers == 2) // replace this with UI and ready button
			StartGame.Invoke();
	}
}