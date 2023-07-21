using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using GameModules.Networking;
using Photon.Pun;
using Photon.Realtime;
using ExitHashtable = ExitGames.Client.Photon.Hashtable;
using UniRx;

namespace GameModules.Networking.Implementations.PhotonV2
{
	public class TDMRoomState : IRoomState,IDisposable
	{
		private RoomState _roomState;
		private ITimer<double> _timer;
		private IScoring<int> _score;
		private double _warmupEnd;
		private double _roundEnd;
		private double _cooldownEnd;
		private bool _gameEnded;
		public event Action<RoomState> onRoomStateUpdate;

		public TDMRoomState(ITimer<double> timer,IScoring<int> scoring)
		{
			_roomState = RoomState.NOT_READY;
			_timer = timer;
			PhotonTimer photonTimer = (PhotonTimer)timer;
			_warmupEnd  = photonTimer.Warmup;
			_roundEnd   = photonTimer.Warmup  + photonTimer.RoundTime;
			_cooldownEnd = photonTimer.Warmup + photonTimer.RoundTime + photonTimer.Cooldown;
			_score = scoring;
			_score.onTeamScoreUpdate += OnScoreUpdate;
			_timer.onRoundEnd += OnRoundEnd;
		}

		#region PUBLIC METHODS
	
		public void Start()
		{
			MainThreadDispatcher.StartUpdateMicroCoroutine(Update());
		}

		IEnumerator Update()
		{
			while (_roomState != RoomState.END)
			{
				var roomState = RoomState.NOT_READY;

				if (_timer == null || _score == null)
				{
					UnityEngine.Debug.LogErrorFormat(" [TDMRoomState] Timer Null = {0}  OR  Scoring Null = {1} ",
						(_timer == null),
						(_score == null));
					_roomState = RoomState.NOT_READY;
					yield return null;
				}

				switch (_timer.State)
				{
					case TimerState.Complete:
					{
						roomState = RoomState.END;
						break;
					}
					case TimerState.Running:
					{
						if (_timer.TimeElapsed < _warmupEnd)
						{
							roomState = RoomState.WARMUP;
						}
						else if (_timer.TimeElapsed < _roundEnd)
						{
							roomState = RoomState.IN_GAME;
						}

						break;
					}				
					case TimerState.Paused:
					{
						roomState = RoomState.PAUSED;
						break;
					}
					case TimerState.None:
					{
						roomState = RoomState.NOT_READY;
						break;
					}
				}

				if(roomState != _roomState)
				{
					_roomState = roomState;
					UnityEngine.Debug.LogErrorFormat("Room State Changed : {0}",roomState);
					onRoomStateUpdate?.Invoke(roomState);
				}
				yield return null;
			}
		}


		public RoomState Get()
		{
			return _roomState;
		}

		public void Reset()
		{
			_roomState = RoomState.NOT_READY;
		}

		public void Dispose()
		{
			_timer = null;
			_score.onTeamScoreUpdate -= OnScoreUpdate;
			_timer.onRoundEnd -= OnRoundEnd;
			onRoomStateUpdate = null;
			_score = null;
			_warmupEnd = 0;
			_cooldownEnd = 0;
			_roundEnd = 0;
			_roomState = RoomState.NOT_READY;
		}
		#endregion
		
		#region PRIVATE METHODS

		void OnScoreUpdate(int id, int score, bool result)
		{
			if (result)
			{
				_timer.Stop();
			}
		}

		void OnRoundEnd()
		{
			if(_roomState != RoomState.END)
			{
				DebugInfo.AppendLog($"RoundEnd TimerState {_timer.State}");
				_roomState = RoomState.END;
				onRoomStateUpdate?.Invoke(_roomState);
			}
		}
		#endregion

	}
}
