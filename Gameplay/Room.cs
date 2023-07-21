using System;
using System.Collections.Generic;
using GameModules.Utils.PhotonPun;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = System.Collections.Hashtable;
using ExitHashTable = ExitGames.Client.Photon.Hashtable;
using PunPlayer = Photon.Realtime.Player;

namespace GameModules.Networking.Implementations.PhotonV2
{
	public class RoomProperties
	{
		public static readonly string RoomId = "id";
		public static readonly string MaxPlayers = "mpl";
		public static readonly string MaxScore = "msc";
		public static readonly string ScoreMap = "scm";
		public static readonly string RoomState = "rs";
		public static readonly string StartTime = "st";
		public static readonly string Map = "m";
		public static readonly string TeamLayout = "tmly";
	}

	//GM_NORMAL: Change Room into and interface which is implemented by
	//network (Photon, SmartFox) or custom solution wrappers
	public class Room : IMatchmakingCallbacks,IInRoomCallbacks,IDisposable
	{
		public event Action<Hashtable> onRoomJoined;
		public event Action<short, string> onRoomJoinedFailed;
		public event Action<short, string> onCreateRoomFailed;
		public event Action<Hashtable> onPlayerEnteredRoom;
		public event Action<Hashtable> onPlayerLeftRoom;
		public event Action onRoomLeave;

		private Photon.Realtime.Room _room;
		private int _roomMaxPlayers;
		private Dictionary<int,PlayerController> _playersConnected;
		private Hashtable _customProperties;


		#region CONSTRUCTORS

		public Room()
		{
			_playersConnected = new Dictionary<int, PlayerController>();
			PhotonNetwork.AddCallbackTarget(this);
		}

		#endregion

		#region PUBLIC METHODS

		public bool SetRoomProperties(Hashtable roomProperties)
		{
			return _room.SetCustomProperties(PhotonUtils.SystemToPhotonHashtable(roomProperties));
		}

		public Hashtable GetCustomProperties()
		{
			return PhotonUtils.ExitToSystemHashtable(_room.CustomProperties);
		}

		public void CreateRoom(Hashtable roomProperties)
		{
			int maxPlayers = (int) roomProperties[RoomProperties.MaxPlayers];
			string roomName = roomProperties[RoomProperties.RoomId].ToString();
			var exitHash = PhotonUtils.SystemToPhotonHashtable(roomProperties);
			//Debug.Log($"[Room] Creating Room With Properties : {exitHash}");
			PhotonNetwork.CreateRoom(roomName,new RoomOptions()
			{
				MaxPlayers = Convert.ToByte(maxPlayers),
				CustomRoomProperties =  exitHash,
				IsOpen = true,
				IsVisible = true,
				CleanupCacheOnLeave = true,
				BroadcastPropsChangeToAll = false,
				DeleteNullProperties = true,
				PublishUserId = true,
				PlayerTtl = 3,
				EmptyRoomTtl = 1
			});
		}

		public void JoinRoom(string roomId)
		{
			if (string.IsNullOrEmpty(roomId))
			{
				PhotonNetwork.JoinRandomRoom(null,0);
				DebugInfo.AppendLog("[Room] Join Random Room");
			}
			else
			{
				DebugInfo.AppendLog($"[Room] Join Room With Id : {roomId}");
				PhotonNetwork.JoinRoom(roomId);
			}
		}

		public void LeaveRoom()
		{
			PhotonNetwork.LeaveRoom();
		}

		public void CloseRoom()
		{
			if(PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
			{
				PhotonNetwork.CurrentRoom.IsOpen = false;
				PhotonNetwork.CurrentRoom.IsVisible = false;
			}
		}

		public void Dispose()
		{
			Debug.LogFormat("[Room] Dispose");
			_room = null;
			onRoomJoined = null;
			onRoomJoinedFailed = null;
			onRoomLeave = null;
			onCreateRoomFailed = null;
			onPlayerEnteredRoom = null;
			onPlayerLeftRoom = null;
			PhotonNetwork.RemoveCallbackTarget(this);
		}

		#endregion

		#region CALLBACKS

		public void OnMasterClientSwitched(PunPlayer newMasterClient)
		{
			DebugInfo.AppendLog("[Room] New Master : " + newMasterClient.GetPlayerName());
		}

		public void OnPlayerEnteredRoom(PunPlayer newPlayer)
		{
			if(onPlayerEnteredRoom != null)
			{
				Hashtable props = new Hashtable();
				foreach (var prop in newPlayer.CustomProperties)
				{
					props.Add(prop.Key, prop.Value);
				}
				onPlayerEnteredRoom?.Invoke(props);
			}
		}

		public void OnPlayerLeftRoom(PunPlayer otherPlayer)
		{
			//DebugInfo.AppendLog("[Room] OnPlayerLeftRoom : " + otherPlayer.ToStringFull());
			if (onPlayerLeftRoom != null)
			{
				Hashtable props = new Hashtable();
				foreach (var prop in otherPlayer.CustomProperties)
				{
					props.Add(prop.Key, prop.Value);
				}

				onPlayerLeftRoom?.Invoke(props);
			}
		}

		public void OnPlayerPropertiesUpdate(PunPlayer targetPlayer, ExitHashTable changedProps)
		{
			//Debug.LogError("[Room] OnPlayerPropertiesUpdate : " + targetPlayer.NickName + " Changed Properties : " + changedProps.ToStringFull());
		}

		public void OnRoomPropertiesUpdate(ExitHashTable propertiesThatChanged)
		{

		}

		public void OnCreatedRoom()
		{
			DebugInfo.AppendLog(string.Format(" [Room] OnCreatedRoom : Master = {0}", PhotonNetwork.CurrentRoom.MasterClientId));
		}

		public void OnCreateRoomFailed(short returnCode, string message)
		{
			DebugInfo.AppendLog(string.Format(" [Room] OnCreateRoomFailed : returnCode = {0} , message = {1}", returnCode,message));
			onCreateRoomFailed?.Invoke(returnCode,message);
		}

		public void OnFriendListUpdate(List<FriendInfo> friendList)
		{
			//Debug.LogError("[Room] OnFriendListUpdate");
		}

		public void OnJoinedRoom()
		{
			if (PhotonNetwork.InRoom)
			{
				_room = PhotonNetwork.CurrentRoom;
				var roomProperties = new Hashtable();
				ExitHashTable customProp = PhotonNetwork.CurrentRoom.CustomProperties;

				foreach (var key in customProp.Keys)
				{
					roomProperties.Add(key, customProp[key]);
				}
				onRoomJoined?.Invoke(roomProperties);
			}
		}

		public void OnLeftRoom()
		{
			Debug.LogError(" [Room] OnLeftRoom ");
			onRoomLeave?.Invoke();
			_room = null;
		}

		public void OnJoinRandomFailed(short returnCode, string message)
		{
			DebugInfo.AppendLog(string.Format(" [Room] OnJoinRandomFailed : returnCode = {0} , message = {1}", returnCode,message));
			onRoomJoinedFailed?.Invoke(returnCode, message);
		}

		public void OnJoinRoomFailed(short returnCode, string message)
		{
			DebugInfo.AppendLog(string.Format("[Room] OnJoinRoomFailed : returnCode = {0} , message = {1}", returnCode,message));
			onRoomJoinedFailed?.Invoke(returnCode, message);
		}

		#endregion

	}
}
