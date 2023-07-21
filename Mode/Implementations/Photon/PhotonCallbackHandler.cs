using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

namespace GameModules.Networking.Implementations.PhotonV2
{
	public delegate void VoidDelegate();
	public delegate void FailedDelegate(short returnCode, string message);
	public delegate void ObjectDelegate<T>(T obj);
	public delegate void PlayerUpdatedDelegate(Photon.Realtime.Player obj,Hashtable properties);

	public class PhotonCallbackHandler : ICallbackHandler, IMatchmakingCallbacks, IInRoomCallbacks
	{
		public event VoidDelegate onCreateRoom;
		public event VoidDelegate onJoinedRoom;
		public event VoidDelegate onLeftRoom;
		public event FailedDelegate onCreateRoomFailed;
		public event FailedDelegate onJoinRandomFailed;
		public event FailedDelegate onJoinRoomFailed;
		public event ObjectDelegate<Photon.Realtime.Player> onMasterClientSwitched;
		public event ObjectDelegate<Photon.Realtime.Player> onPlayerEnteredRoom;
		public event ObjectDelegate<Photon.Realtime.Player> onPlayerLeftRoom;
		public event ObjectDelegate<List<FriendInfo>> onFriendListUpdate;
		public event ObjectDelegate<Hashtable> onRoomPropertiesUpdate;
		public event PlayerUpdatedDelegate onPlayerPropertiesUpdate;

		public PhotonCallbackHandler()
		{
		}

		public void Listen()
		{
			PhotonNetwork.AddCallbackTarget(this);
		}

		public void Ignore()
		{
			PhotonNetwork.RemoveCallbackTarget(this);
		}

		public void Dispose()
		{
			onCreateRoom = null;
			onJoinedRoom = null;
			onLeftRoom = null;
			onCreateRoomFailed = null;
			onJoinRandomFailed = null;
			onJoinRoomFailed = null;
			onMasterClientSwitched = null;
			onPlayerEnteredRoom = null;
			onPlayerLeftRoom = null;
			onFriendListUpdate = null;
			onRoomPropertiesUpdate = null;
			onPlayerPropertiesUpdate = null;
		}

		#region VOID CALLBACKS
		public void OnCreatedRoom()
		{
			onCreateRoom?.Invoke();
		}
		public void OnJoinedRoom()
		{
			onJoinedRoom?.Invoke();
		}
		public void OnLeftRoom()
		{
			onLeftRoom?.Invoke();
		}
		#endregion

		#region FAILED CALLBACKS

		public void OnCreateRoomFailed(short returnCode, string message)
		{
			onCreateRoomFailed?.Invoke(returnCode,message);
		}

		public void OnJoinRandomFailed(short returnCode, string message)
		{
			onJoinRandomFailed?.Invoke(returnCode,message);
		}

		public void OnJoinRoomFailed(short returnCode, string message)
		{
			onJoinRoomFailed?.Invoke(returnCode,message);
		}

		#endregion

		#region PLAYER CALLBACKS

		public void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
		{
			onMasterClientSwitched?.Invoke(newMasterClient);
		}

		public void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
		{
			onPlayerEnteredRoom?.Invoke(newPlayer);
		}

		public void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
		{
			onPlayerLeftRoom?.Invoke(otherPlayer);
		}

		#endregion

		#region FRIEND UPDATE CALLBACKS

		public void OnFriendListUpdate(List<FriendInfo> friendList)
		{
			onFriendListUpdate?.Invoke(friendList);
		}

		#endregion

		#region PROPERTIES UPDATE CALLBACKS
		public void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
		{
			onPlayerPropertiesUpdate?.Invoke(targetPlayer, changedProps);
		}

		public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
		{
			onRoomPropertiesUpdate?.Invoke(propertiesThatChanged);
		}

		#endregion

		
	}
}
