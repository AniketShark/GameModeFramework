using System;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = System.Collections.Hashtable;
using ExitHashtable = ExitGames.Client.Photon.Hashtable;
using PunPlayer = Photon.Realtime.Player;

namespace GameModules.Networking.Implementations.PhotonV2
{
	public class ScoreObject
	{
		public int id;
		public int score;
	}

	public class TDMScoreToWin : IScoring<int>
	{
		public event Action<int,int,bool> onTeamScoreUpdate;
		public event Action<int, int, bool> onPersonalScoreUpdate;

		protected int _maxScore;
		protected Dictionary<int,int> _teamToScoreMap;
		protected PhotonCallbackHandler _callbackHandler;

		public TDMScoreToWin(ICallbackHandler callbackHandler)
		{
			_callbackHandler = (PhotonCallbackHandler)callbackHandler;
			_teamToScoreMap = new Dictionary<int, int>();
		}

		#region PUBLIC METHODS

		public int GetScoreFor(int teamId)
		{
			if(_teamToScoreMap.ContainsKey(teamId))
				return _teamToScoreMap[teamId];
			return -1;
		}

		public int GetLeader()
		{
			var sortedList = new List<ScoreObject>();
			foreach (var pair in _teamToScoreMap)
				sortedList.Add(new ScoreObject(){score = pair.Value,id = pair.Key});
			sortedList.Sort((t1, t2) => { return t2.score.CompareTo(t1.score);});
			var first = sortedList[0];
			var second = sortedList[1];
			return first.score.Equals(second.score) ? -1 : first.id;
		}

		public void Init(Hashtable data)
		{
			_maxScore    = (int)data[ScoringProps.MaxScore];
			var scoreMap = data[ScoringProps.ScoreMap] as IDictionary<int, int>;

			foreach (var team in scoreMap)
			{
				_teamToScoreMap.Add(team.Key,team.Value);
				onTeamScoreUpdate?.Invoke(team.Key, team.Value, team.Value >= _maxScore);
			}

			if (PhotonNetwork.IsMasterClient)
			{
				ExitHashtable scoringProps = new ExitHashtable();
				scoringProps.Add(ScoringProps.MaxScore,_maxScore);
				scoringProps.Add(ScoringProps.ScoreMap,scoreMap);
				PhotonNetwork.CurrentRoom.SetCustomProperties(scoringProps);
			}

			_callbackHandler.onPlayerPropertiesUpdate += OnPlayerPropertiesUpdate;
		}

		public void AddScore(int id,int score)
		{
			if (PhotonNetwork.CurrentRoom != null)
			{
				var killer = PhotonNetwork.CurrentRoom.GetPlayer(id);
				UnityEngine.Debug.Assert(killer != null);
				int personalScore = killer.GetPersonalScoreContribution();
				personalScore += score;
				killer.SetCustomProperties(new ExitHashtable()
				{
					{ ScoringProps.TeamScoreContribution,score},
					{ ScoringProps.PersonalScore,personalScore}
				});
			}
		}

		public void Dispose()
		{
			_maxScore = 0;
			onTeamScoreUpdate = null;
			_teamToScoreMap.Clear();
			_teamToScoreMap = null;
			_callbackHandler.onPlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
			_callbackHandler = null;
		}

		public void Reset()
		{
			_maxScore = 0;
			_teamToScoreMap.Clear();
			_callbackHandler.onPlayerPropertiesUpdate -= OnPlayerPropertiesUpdate;
		}

		#endregion

		#region ROOM CALLBACKS

		public void OnPlayerPropertiesUpdate(PunPlayer targetPlayer, ExitHashtable changedProps)
		{
			int teamId = targetPlayer.GetTeam();

			if (changedProps.ContainsKey(ScoringProps.TeamScoreContribution) &&	_teamToScoreMap.ContainsKey(teamId))
			{

				_teamToScoreMap[teamId] +=(int)changedProps[ScoringProps.TeamScoreContribution];

				// Only Master Client updates Room Properties with current team score
				// that way we never have different players updating the same team score value 
				// simultaneously which causes call drop in score update.
				if(PhotonNetwork.IsMasterClient)
				{
					PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitHashtable() { { ScoringProps.ScoreMap,_teamToScoreMap} });
				}

				onTeamScoreUpdate?.Invoke(teamId, _teamToScoreMap[teamId], _teamToScoreMap[teamId] >= _maxScore);
			}
			
			if (changedProps.ContainsKey(ScoringProps.PersonalScoreContribution))
			{
				onPersonalScoreUpdate?.Invoke(targetPlayer.GetPlayerId(), targetPlayer.GetPersonalScoreContribution(),false);
			}
		}

		#endregion
	}
}
