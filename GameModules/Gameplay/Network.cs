using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System;
using System.Collections;
using ExitGames.Client.Photon;
using GameModules.Utils.PhotonPun;
using Hashtable = System.Collections.Hashtable;

namespace GameModules.Networking.Implementations.PhotonV2
{
	public class Network : IConnectionCallbacks,IDisposable
	{
		private string _gameVersion;
		private Action _onNetworkConnected;
		private Action _onNetworkDisconnected;

		#region CONSTRUCTORS
		private Network()
		{

		}
		public Network(string version,Action onNetworkConnected, Action onNetworkDisconnected)
		{
			_gameVersion = version;
			_onNetworkConnected    = onNetworkConnected;
			_onNetworkDisconnected = onNetworkDisconnected;
			PhotonNetwork.AddCallbackTarget(this);
		}
		#endregion

		#region PROPERTIES

		public bool IsConnected => PhotonNetwork.IsConnected;
		public bool IsMasterClient => PhotonNetwork.IsMasterClient;
		public double Time => PhotonNetwork.Time;

		#endregion

		#region PUBLIC METHODS

		public void SetLocalPlayerProperties(Hashtable customProperties)
		{
			PhotonNetwork.LocalPlayer.SetCustomProperties(PhotonUtils.SystemToPhotonHashtable(customProperties));
		}

		public void SetNickName(string nickName)
		{
			PhotonNetwork.NickName = nickName;
		}

		public void Connect()
		{
			PhotonNetwork.AutomaticallySyncScene = true;
			PhotonNetwork.GameVersion = _gameVersion;
			PhotonNetwork.ConnectUsingSettings();
		}

		public void Disconnect()
		{
			PhotonNetwork.RemovePlayerCustomProperties(null);
			PhotonNetwork.Disconnect();
		}

		#endregion

		#region CONECTION CALLBACKS

		public void OnConnected()
		{
			_onNetworkConnected?.Invoke();
			Debug.LogError("[Network] OnConnected");
		}

		public void OnConnectedToMaster()
		{
			Debug.LogErrorFormat("[Network] OnConnectedToMaster Region {0}", PhotonNetwork.NetworkingClient.CloudRegion);
		}

		public void OnDisconnected(DisconnectCause cause)
		{
			_onNetworkDisconnected?.Invoke();

			//Debug.LogErrorFormat("[Network] OnDisconnected : Cause : " , cause);
		}

		public void OnRegionListReceived(RegionHandler regionHandler)
		{
			//Debug.LogErrorFormat("[Network] OnRegionListReceived : Regions :\n{0} ", regionHandler.GetResults());
		}

		public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
		{
			//Debug.LogErrorFormat("[Network] OnCustomAuthenticationResponse : CustomAuthentication : \n{0}" ,data);
		}

		public void OnCustomAuthenticationFailed(string debugMessage)
		{
			//Debug.LogErrorFormat("[Network] OnCustomAuthenticationFailed : Failed Reason : {0}", debugMessage);
		}

		public void Dispose()
		{ 
			_gameVersion = "";
			PhotonNetwork.RemoveCallbackTarget(this);
		}

		#endregion

	}
}
