using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using GameModules.Teams;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine;
using PunPlayer = global::Photon.Realtime.Player;
using Random = System.Random;

namespace GameModules.Networking.Implementations.PhotonV2
{
	/// <summary>
	/// This class is the extended implementation of the PhotonTeamManager class
	/// It implements GameModules ITeamAssigner to provide required team assign
	/// functionality and act a wrapper for photon specific implementation
	/// </summary>
	public class TDMTeamAssigner : ITeamAssigner
	{
		public event Action<IDictionary<string,object>> onPlayerJoinedTeam;
		public event Action<IDictionary<string, object>> onPlayerLeftTeam;
		public event Action<IList<object>> onPlayerListRefresh;

		private IDictionary<int, Team> _teamsByCode;
        private IDictionary<string, Team> _teamsByName;
		private IDictionary<int, HashSet<PunPlayer>> _playersPerTeam;
		private ITeamJoinStrategy _strategy;
		private List<Team> _teamsList;
		private int _maxTeams;
		private int _maxPlayersPerTeam;
		private int[] _allTeams;
		private PhotonCallbackHandler _callbackHandler;

		#region CONSTRUCTORS

		public TDMTeamAssigner (
			ICallbackHandler callbackHandler,
			int maxTeams, 
			int maxPlayersPerTeam,
			ITeamJoinStrategy strategy)
		{
			_maxTeams = maxTeams;
			_maxPlayersPerTeam = maxPlayersPerTeam;
			_teamsList         = new List<Team>(maxTeams);
			_teamsByCode       = new Dictionary<int, Team>(maxTeams);
			_teamsByName       = new Dictionary<string, Team>(maxTeams);
			_playersPerTeam    = new Dictionary<int, HashSet<PunPlayer>>(maxTeams);
			var itEnumerator   = TeamColors.Colors.GetEnumerator();
			itEnumerator.MoveNext();

			for (int i = 0; i < maxTeams; i++)
			{
				_teamsList.Add(new Team(itEnumerator.Current.Key,i,maxPlayersPerTeam,itEnumerator.Current.Value));
				_teamsList[i].UpdateMaxPlayers(maxPlayersPerTeam);
				_teamsList[i].ResetOpenSlots();
				_teamsByCode[_teamsList[i].code]    = _teamsList[i];
				_teamsByName[_teamsList[i].name]    = _teamsList[i];
				_playersPerTeam[_teamsList[i].code] = new HashSet<PunPlayer>();
				itEnumerator.MoveNext();
			}

			itEnumerator.Dispose();
			strategy.Init(_teamsList);
			_strategy = strategy;

			_callbackHandler = (PhotonCallbackHandler) callbackHandler;
			_callbackHandler.onJoinedRoom             += OnJoinedRoom;
			_callbackHandler.onLeftRoom               += OnLeftRoom;
			_callbackHandler.onPlayerEnteredRoom      += OnPlayerEnteredRoom;
			_callbackHandler.onPlayerLeftRoom         += OnPlayerLeftRoom;
			_callbackHandler.onPlayerEnteredRoom      += OnPlayerEnteredRoom;
			_callbackHandler.onPlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
		}
		
		#endregion

		#region PRIVATE METHODS

		private void UpdateTeams()
		{
			this.ClearTeams();
			IList<object> data = new List<object>();
			//Debug.LogError($"Player List Count : {PhotonNetwork.PlayerList.Length}");
			for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
			{
				PunPlayer player = PhotonNetwork.PlayerList[i];
				int teamId = player.GetTeam();
				//Debug.LogError($"Player Name {player.UserId} Team Id: {player.CustomProperties}");

				if (_teamsByCode.ContainsKey(teamId))
				{
					Team playerTeam = _teamsByCode[teamId];
					if (playerTeam != null)
					{
						//Debug.LogError($"Team Found");
						if (playerTeam.DecreamentSlots())
						{
							//Debug.LogError($"Adding Player : {player.ActorNumber}");
							var playerData = new Dictionary<string, object>() {
									{PlayerProps.PlayerId,player.GetPlayerId()},
									{PlayerProps.PlayerName,player.GetPlayerName()},
									{PlayerProps.PlayerTeam,playerTeam},
									{PlayerProps.IsLocal,player.IsLocal}
							 };
							 data.Add(playerData);
							_playersPerTeam[playerTeam.code].Add(player);
						}
					}
				}
			}

			onPlayerListRefresh?.Invoke(data);
		}

		private void ClearTeams()
		{
			foreach (var key in _playersPerTeam.Keys)
			{
				if (_teamsByCode.ContainsKey(key))
				{
					Team playerTeam = _teamsByCode[key];
					playerTeam.ResetOpenSlots();
					_playersPerTeam[key].Clear();
				}
			}
		}

		#endregion

		#region PUBLIC METHODS

		public void JoinTeam(Team team,int slot, Action<bool> callback)
		{
			lock (_strategy)
			{
				bool teamJoined = false;
				var player = PhotonNetwork.LocalPlayer;
				int teamId = player.GetTeam();

				if(teamId != -1)
				{
					UnityEngine.Debug.LogErrorFormat("JoinTeam failed: player ({0}) is already joined to a team ({1}), call SwitchTeam instead", player, team);
					callback?.Invoke(teamJoined);
					return;
				}

				if (slot == -1)
				{
					UnityEngine.Debug.LogErrorFormat("JoinTeam failed: Slot ({0}) is not a valid slot in team ({1})", slot, team);
					callback?.Invoke(teamJoined);
					return;
				}

				bool join = true;
				// We go through strategy and check if joining to this
				// team is still valid or not.
				join &= _strategy.IsTeamJoinValid(player.UserId,team, slot);
				if (join)
				{
					UnityEngine.Debug.LogErrorFormat($"Team Join Valid {team.code} slot {slot}");
					teamJoined = player.SetCustomProperties(new Hashtable {
						{ TeamProps.TeamCode, team.code },
						{ TeamProps.TeamName, team.name },
						{ TeamProps.TeamSlot, slot }
					});

					callback?.Invoke(teamJoined);
				}
				else
				{
					UnityEngine.Debug.LogErrorFormat("JoinTeam failed: No open slots in a team ({0}), or failed the joining criteria", team);
					callback?.Invoke(teamJoined);
				}
			}
		}

		public void LeaveTeam(Team team, Action<bool> callback)
		{
			bool teamLeft = false;
			var player  = PhotonNetwork.LocalPlayer;
			int teamId      = player.GetTeam();
			int teamSlot    = player.GetTeamSlot();
			string teamName = player.GetTeamName();

			if (teamId == -1 || teamSlot == -1)
			{
				UnityEngine.Debug.LogErrorFormat("Leave Current Team failed: player ({0}) was not joined to any team", player);
				callback?.Invoke(teamLeft);
				return;
			}


			if (_strategy.ReleaseSlot(player.UserId,team, teamSlot))
			{

				teamLeft = player.SetCustomProperties(
									new Hashtable {
									{ TeamProps.TeamCode, null },
									{ TeamProps.TeamName, null },
									{ TeamProps.TeamSlot, -1},
									});
			}

			//UnityEngine.Debug.LogErrorFormat("Team Left : {0} Team Id : {1}" ,teamLeft ,teamId);
			callback?.Invoke(teamLeft);
		}

		public void SwitchTeam(Team team,int slot, Action<bool> callback)
		{
			bool teamSwitch = false;
			var player = PhotonNetwork.LocalPlayer;

			int teamId      = player.GetTeam();
			string teamName = player.GetTeamName();

			if (teamId == -1)
			{
				UnityEngine.Debug.LogErrorFormat("Switch Team failed: player ({0}) was not joined to any team call JoinTeam first", player);
				callback?.Invoke(teamSwitch);
				return;
			}
			else if(teamId == team.code)
			{
				UnityEngine.Debug.LogErrorFormat("Switch Team failed: player ({0}) is already joined to the same team {1}", player, team);
				callback?.Invoke(teamSwitch);
				return; 
			}

			if (slot == -1)
			{
				UnityEngine.Debug.LogErrorFormat("Switch Team failed: Slot ({0}) is not a valid slot in team ({1})", slot, team);
				callback?.Invoke(teamSwitch);
				return;
			}

			bool join = true;
			// We go through strategy and check if joining to this
			// team is still valid or not.
			join &= _strategy.IsTeamJoinValid(player.UserId,team, slot);

			if (join)
			{
				teamSwitch = player.SetCustomProperties(
									new Hashtable {
										{ TeamProps.TeamCode, team.code },
										{ TeamProps.TeamName, team.name },
										{ TeamProps.TeamSlot, -1 }
									},
									new Hashtable {
										{ TeamProps.TeamCode, teamId },
										{ TeamProps.TeamName, teamName },
										{ TeamProps.TeamSlot, -1 }
									});
				callback?.Invoke(teamSwitch);
			}
			else
			{
				UnityEngine.Debug.LogErrorFormat("Switch Team failed: No open slots in a team ({0}), or failed the joining cr iteria", team);
				callback?.Invoke(teamSwitch);
			}
		}

		public void GetAvailableTeam(Action<Team,int> callback)
		{
			int slotIndex = -1;
			var player  = PhotonNetwork.LocalPlayer;
			_strategy.GetAvailableTeam(player.UserId, callback);
			//callback?.Invoke(team,slotIndex);
		}

		public List<Team> GetAllTeams()
		{
			return _teamsList;
		}

		public bool GetTeamByName(string name, out Team team)
		{
			return _teamsByName.TryGetValue(name,out team);
		}

		public bool GetTeamByCode(int code, out Team team)
		{
			return _teamsByCode.TryGetValue(code,out team);
		}

		public void Dispose()
		{
			onPlayerJoinedTeam = null;
			onPlayerLeftTeam= null;
			onPlayerListRefresh = null;

			_callbackHandler.onJoinedRoom -= OnJoinedRoom;
			_callbackHandler.onLeftRoom -= OnLeftRoom;
			_callbackHandler.onPlayerEnteredRoom -= OnPlayerEnteredRoom;
			_callbackHandler.onPlayerLeftRoom -= OnPlayerLeftRoom;
			_callbackHandler.onPlayerEnteredRoom -= OnPlayerEnteredRoom;
			_callbackHandler.onPlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;

			//PhotonNetwork.RemoveCallbackTarget(this);
			ClearTeams();
		}

		#endregion

		#region PHOTON CALLBACKS

		void OnJoinedRoom()
		{
			UpdateTeams();
		}

		void OnLeftRoom()
		{
			ClearTeams();
		}

		void OnPlayerEnteredRoom(PunPlayer newPlayer)
		{
			int teamId = newPlayer.GetTeam();

			if (teamId == -1)
			{
				return;
			}

			Team team = _teamsByCode[teamId];

			if (_playersPerTeam[team.code].Contains(newPlayer))
			{
				return;
			}

			IDictionary<string, object> data = new Dictionary<string, object>() {
									{PlayerProps.PlayerId,newPlayer.GetPlayerId() },
									{PlayerProps.PlayerName,newPlayer.GetPlayerName()},
									{PlayerProps.IsLocal,newPlayer.IsLocal}
			};

			foreach (var key in _teamsByCode.Keys)
			{
				if (_playersPerTeam[key].Remove(newPlayer))
				{
					break;
				}
			}
			if (!_playersPerTeam[team.code].Add(newPlayer))
			{
				UnityEngine.Debug.LogWarningFormat("Unexpected situation while adding player {0} who joined to team {1}, updating teams for all", newPlayer, team);
				// revert to 'brute force' in case of unexpected situation
				this.UpdateTeams();
			}
			else
			{
				team.DecreamentSlots();
				data.Add(PlayerProps.PlayerTeam, team);
				onPlayerJoinedTeam?.Invoke(data);
			}
		}

		void OnPlayerLeftRoom(PunPlayer otherPlayer)
		{
			if (otherPlayer.IsInactive)
				return;

			int teamId = otherPlayer.GetTeam();
			int teamSlot = otherPlayer.GetTeamSlot();
			if (teamId == -1)
				return;

			Team team = _teamsByCode[teamId];
			IDictionary<string, object> data = new Dictionary<string, object>() {
									{PlayerProps.PlayerId,otherPlayer.GetPlayerId() },
									{PlayerProps.PlayerName,otherPlayer.GetPlayerName()},
									{PlayerProps.IsLocal,otherPlayer.IsLocal}
								};


			_strategy.ReleaseSlot(otherPlayer.UserId, team, teamSlot);

			//PhotonTeam team = otherPlayer.GetPhotonTeam();
			if (team != null && !_playersPerTeam[team.code].Remove(otherPlayer))
			{
				UnityEngine.Debug.LogWarningFormat("Unexpected situation while removing player {0} who left from team {1}, updating teams for all", otherPlayer, team);
				// revert to 'brute force' in case of unexpected situation
				this.UpdateTeams();
			}
			else
			{
				data.Add(PlayerProps.PlayerTeam, team);
				team.IncreamentSlots();
				onPlayerLeftTeam?.Invoke(data);
			}
		}

		void OnPlayerPropertiesUpdate(PunPlayer targetPlayer, Hashtable changedProps)
		{ 
			object temp;
			if (changedProps.TryGetValue(TeamProps.TeamCode, out temp))
			{
				IDictionary<string, object> data = new Dictionary<string, object>() {
									{PlayerProps.PlayerId,targetPlayer.GetPlayerId() },
									{PlayerProps.PlayerName,targetPlayer.GetPlayerName()},
									{PlayerProps.IsLocal,targetPlayer.IsLocal}
				};

				if (temp == null)
				{
					foreach (int code in _playersPerTeam.Keys)
					{
						if (_playersPerTeam[code].Remove(targetPlayer))
						{
							_teamsByCode[code].IncreamentSlots(); 
							if (onPlayerLeftTeam != null)
							{
								data.Add(PlayerProps.PlayerTeam, _teamsByCode[code]);
								//Debug.LogErrorFormat("Player {0} Left Team {1}",targetPlayer.UserId, code);
								onPlayerLeftTeam(data);
							}
							break;
						}
					}
				}
				else if (temp is int)
				{
					int teamCode = (int)temp;
					// check if player switched teams, remove from previous team 
					foreach (int code in _playersPerTeam.Keys)
					{
						if (code == teamCode)
						{
							continue;
						}
						if (_playersPerTeam[code].Remove(targetPlayer))
						{
							_teamsByCode[code].IncreamentSlots();
							if (onPlayerLeftTeam != null)
							{
								data.Add(PlayerProps.PlayerTeam, _teamsByCode[code]);
								Debug.LogErrorFormat("Player {0} Left Team {1}",targetPlayer.ActorNumber, code);
								onPlayerLeftTeam(data);
							}
							break;
						}
					}

					Team team = _teamsByCode[teamCode];

					if (!_playersPerTeam[teamCode].Add(targetPlayer))
					{
						UnityEngine.Debug.LogWarningFormat("Unexpected situation while setting team {0} for player {1}, updating teams for all", team, targetPlayer);
						this.UpdateTeams();
					}
					else
					{
						team.DecreamentSlots();
					}

					if (onPlayerJoinedTeam != null)
					{
						if(data.ContainsKey(PlayerProps.PlayerTeam))
							data[PlayerProps.PlayerTeam] = team;
						else
							data.Add(PlayerProps.PlayerTeam, team);
						//Debug.LogErrorFormat("Joining Player {0} to Team {1}",targetPlayer.ActorNumber, team.code);
						onPlayerJoinedTeam(data);
					}
				}
				else
				{
					Debug.LogErrorFormat("Unexpected: custom property key {0} should have of type byte, instead we got {1} of type {2}. Player: {3}",
						TeamProps.TeamCode, temp, temp.GetType(), targetPlayer);
				}
			}
		}

		#endregion
	}

	
}
