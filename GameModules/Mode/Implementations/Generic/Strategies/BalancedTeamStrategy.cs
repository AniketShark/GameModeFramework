using ExitGames.Client.Photon;
using GameModules.Networking.Implementations.PhotonV2;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameModules.Teams
{
	public class BalancedTeamStrategy : ITeamJoinStrategy,IDisposable
	{
		private List<Team> _teamList;
		private PhotonCallbackHandler photonCallbackHandler;
		private int _totalNumberOfSlots;
		private Action<Team, int> OnTeamAssignedCallback; 
		private bool _searchingSlot;
		private int _currentTeamIndex;
		private int _currentSlotIndex;
		private string _currentUserId;

		#region PUBLIC METHODS

		public BalancedTeamStrategy(ICallbackHandler callbackHandler)
		{
			photonCallbackHandler = (PhotonCallbackHandler)callbackHandler;
			photonCallbackHandler.onRoomPropertiesUpdate += OnRoomPropertiesUpdate;
			PhotonNetwork.NetworkingClient.OpResponseReceived += OnCASFailed;
		}

		private void OnRoomLeft()
		{

		}

		public void Init(List<Team> teamList)
		{
			_teamList = teamList;
			teamList.ForEach((team) => { _totalNumberOfSlots += team.MaxSlots; });
		}

		public void Dispose()
		{
			if(photonCallbackHandler != null)
				photonCallbackHandler.onRoomPropertiesUpdate  -= OnRoomPropertiesUpdate;
			PhotonNetwork.NetworkingClient.OpResponseReceived -= OnCASFailed;
		}

		public bool IsTeamJoinValid(string userId,Team team,int slot)
		{
			var roomProperties = PhotonNetwork.CurrentRoom.CustomProperties; 
			if(roomProperties.ContainsValue(userId))
			{
				return true;
			}
			return false;
		}

		public void GetAvailableTeam(string userId, Action<Team, int> callback)
		{
			OnTeamAssignedCallback = callback;
			_currentSlotIndex = -1;
			_currentTeamIndex = 0;
			_searchingSlot = true;
			_currentUserId = userId;
			ReturnAvailableTeam(userId, _currentTeamIndex, _currentSlotIndex);
		}

		private void ReturnAvailableTeam(string userId,int teamIndex,int slotIndex)
		{
			if (!_searchingSlot)
				return;

			_currentTeamIndex = teamIndex % _teamList.Count;
			if (_currentTeamIndex == 0)
				_currentSlotIndex += 1;

			if (_currentSlotIndex >= _teamList[_currentTeamIndex].MaxSlots)
				return;

			string slotKey = _teamList[_currentTeamIndex].GetSlotKey(_currentSlotIndex);
			PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { slotKey, userId } }, new Hashtable { { slotKey, "~" } });
		}

		public bool ReleaseSlot(string playerId, Team team, int slot)
		{
			string slotKey = team.GetSlotKey(slot);
			if (PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable {{ slotKey, "~" }}, new Hashtable {{ slotKey, playerId}}))
			{
				Debug.LogError($" [BalancedTeamStrategy] slot : {team} index : {slot} ");
				return true;
			}
			return false;
		}

		#endregion

		#region PHOTON CALLBACKS

		private void OnCASFailed(OperationResponse opResponse)
		{
			if (opResponse.OperationCode == OperationCode.SetProperties && 
				opResponse.ReturnCode == ErrorCode.InvalidOperation)
			{
				Debug.LogError(opResponse.ToStringFull());
				if(_searchingSlot)
				{
					Debug.LogError("Continue Searching For Slot");
					_currentTeamIndex++;
					ReturnAvailableTeam(_currentUserId, _currentTeamIndex, _currentSlotIndex);
				}
			}
		}

		private void OnRoomPropertiesUpdate(Hashtable changedProperties)
		{
			//Debug.LogError($"[BalancedTeamStrategy] Chanegd Properties {changedProperties}");
			if (changedProperties.ContainsValue(PhotonNetwork.LocalPlayer.UserId))
			{
				_searchingSlot = false;
				//Debug.LogError($"[BalancedTeamStrategy] Stopping Slot Search {changedProperties}");

				string userId = PhotonNetwork.LocalPlayer.UserId;
				foreach (var property in changedProperties)
				{
					if (property.Value.Equals(userId))
					{
						string[] keyValue = property.Key.ToString().Split('_');
						if (OnTeamAssignedCallback != null)
						{
							Team team = _teamList[int.Parse(keyValue[0])];
							int slot = int.Parse(keyValue[1]);
							Debug.LogError($"[BalancedTeamStrategy] Callback OnTeamAssignedCallback({team.code}, {slot}) ");
							OnTeamAssignedCallback(team, slot);
							OnTeamAssignedCallback = null;
							break;
						}
					}
				}
			}
		}



		#endregion
	}
}
