using System;
using System.Collections;
using UnityEngine;
using GameModules.Networking.Implementations.PhotonV2;
using Network = GameModules.Networking.Implementations.PhotonV2.Network;
using GameModules;

public class NetworkHandler
{
	private static NetworkHandler _instance;
	public static NetworkHandler Instance {
		get
		{
			if(_instance == null)_instance = new NetworkHandler();
			return _instance;
		}
	}

	[SerializeField] private string _gameVersion;
	private Network _network;
	private Room _roomManager;
	public static Room Room {
		get { return Instance._roomManager; }
	}
	public static Network NetworkManager {
		get { return Instance._network; }
	}
	
	public static event Action onConnectedToNetwork;
	public static event Action onDisconnectedFromNetwork;

	private NetworkHandler()
	{
		_instance = this;
		_network     = new Network(_gameVersion, OnConnected, OnDisconnected);
		_roomManager = new Room();
	}

	void OnDestroy()
	{
	
		_network.Dispose();
		_roomManager.Dispose();
		_network = null;
		_roomManager = null;
	}

	#region ROOM

	public static void JoinRoom(string roomId)
	{
		//Debug.Log(" [NetworkHandler] JoinRoom : " + roomId);
		Instance._roomManager.JoinRoom(roomId);
	}

	public static void LeaveRoom()
	{
		//Debug.LogError(" [NetworkHandler] LeaveRoom ");
		Instance._roomManager.LeaveRoom();
	}

	#endregion

	#region NETWORK

	public static void Disconnect()
	{
		Instance._network.Disconnect();
	}

	public static void Connect()
	{
		Instance._network.Connect();
	}

	public void OnDisconnected()
	{
		onDisconnectedFromNetwork?.Invoke();
	}

	public void OnConnected()
	{
		onConnectedToNetwork?.Invoke();
	}

	#endregion

}
