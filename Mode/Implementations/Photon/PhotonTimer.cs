using UniRx;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using System;
using Photon.Realtime;

namespace GameModules.Networking.Implementations.PhotonV2
{
	public class PhotonTimer : ITimer<double>,IDisposable
	{
		private double _warmupTime;
		private double _roundTime;
		private double _cooldownTime;
		private double _totalTime;
		private double _startTime;
		private double _endTime;
		private double _timeElapsed;
		private double _timeRemaining;
		private TimerState _state;
		private double _currentTime;
		private IDisposable _timeTicker;
		private PhotonCallbackHandler _photonCallbackHandler;
		public event Action<double,double> onSecondTick;
		public event Action onRoundEnd;


		//This change Should be pulled in raw modes branch
		#region CONSTRUCTOR
		private PhotonTimer()
		{	
		}
		public PhotonTimer(ICallbackHandler photonCallbackHanlder,params double[] times)
		{
			_warmupTime = times[0];
			_roundTime = times[1];
			_cooldownTime = times[2];
			_totalTime = _warmupTime + _roundTime + _cooldownTime;
			_startTime = PhotonNetwork.Time;
			_currentTime = PhotonNetwork.Time;
			_photonCallbackHandler = (PhotonCallbackHandler)photonCallbackHanlder;
			_photonCallbackHandler.onRoomPropertiesUpdate += OnRoomPropertiesUpdate;
		}
		#endregion

		#region PROPERTIES

		public double Warmup { get { return _warmupTime; } }
		public double RoundTime { get { return _roundTime; } }
		public double Cooldown { get { return _cooldownTime; } }
		public double TimeElapsed{ get { return _timeElapsed; } }
		public double TimeRemaining	{ get {	return _totalTime - _timeElapsed; }}
		public double CurrentTimestamp { get { return _currentTime; }}
		public TimerState State { get {	return _state; } }

		#endregion

		#region PUBLIC METHODS

		public void Start(double startTime)
		{
			if (PhotonNetwork.IsMasterClient)
			{
				_startTime = PhotonNetwork.Time;
				PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable()
					{{RoomProperties.StartTime, _startTime}});
				DebugInfo.AppendLog($"[Master] RoomStartTime  {_startTime}");
			}
			else
			{ 
				_startTime =  startTime;
				DebugInfo.AppendLog($"[Client] RoomStartTime {_startTime}"); 
			}


			_endTime = _startTime + _totalTime;
			_state = TimerState.Running;

			if (_timeTicker != null)
				_timeTicker.Dispose();

			_timeTicker = Observable.FromCoroutine(Ticker).Subscribe((next) =>
			{
				Debug.LogError("Timer Ticking");
			},
			() =>
			{
				_timeTicker.Dispose();
				_timeTicker = null;
			});

			//Debug.LogErrorFormat("[PhotonTimer] Start Time : {0}  End Time : {1}  Diff : {2} Photon Time : {3} Total Time {4}",
			//	_startTime, _endTime, (_endTime - _startTime), PhotonNetwork.Time,_totalTime);
		}

		public void Stop()
		{
			_state = TimerState.Complete;
			_startTime = PhotonNetwork.Time;
			_timeElapsed = _totalTime;
			_timeTicker?.Dispose();
			onRoundEnd?.Invoke();
		}

		public void Reset()
		{
			_timeTicker?.Dispose();
			_state = TimerState.None;
			_startTime = 0d;
			_endTime = 0d;
			_photonCallbackHandler.onRoomPropertiesUpdate -= OnRoomPropertiesUpdate;
		}

		public void Pause()
		{
			if (_state == TimerState.Running)
				_state = TimerState.Paused;
		}

		public void Resume()
		{
			if (_state == TimerState.Paused)
			{
				_state = TimerState.Running;
				_startTime = PhotonNetwork.Time - _timeElapsed;

				if (PhotonNetwork.IsMasterClient)
				{
					PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { RoomProperties.StartTime, _startTime } });
				}
			}
		}

		#endregion

		#region PRIVATE METHODS

		private IEnumerator Ticker()
		{
			while (_startTime <= 0d)
			{
				if (PhotonNetwork.IsMasterClient)
				{
					DebugInfo.AppendLog($"[Master] Resetting {PhotonNetwork.Time} " );
					Start(PhotonNetwork.Time);
				}
				else
				{
					if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomProperties.StartTime))
					{
						double startTime = (double)PhotonNetwork.CurrentRoom.CustomProperties[RoomProperties.StartTime];
						DebugInfo.AppendLog($"[Client]FetchStartTime :{startTime} ");
						if(startTime > 0d)
						{
							DebugInfo.AppendLog($"[Client]RoomStartTimeTicker {startTime}");
							Start(startTime);
						}
					}
				}
				yield return null;
			}

			while (_state != TimerState.Complete)
			{
				_timeElapsed = PhotonNetwork.Time - _startTime;
				_currentTime = PhotonNetwork.Time;

				if (_state == TimerState.Running)
				{
					if (_timeElapsed > _totalTime)
					{
						DebugInfo.AppendLog($"RoundEnd Elp {_timeElapsed} Ttl {_totalTime}");
						_state = TimerState.Complete;
						onRoundEnd?.Invoke();
						yield break;
					}
				}

				yield return new WaitForSeconds(1);
				onSecondTick?.Invoke(_warmupTime -_timeElapsed,TimeRemaining);
			}
			_timeElapsed = _totalTime;
			_currentTime = PhotonNetwork.Time;
		}

		public void Dispose()
		{
			_photonCallbackHandler.onRoomPropertiesUpdate -= OnRoomPropertiesUpdate;
		}

		private void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable properties)
		{
			if(!PhotonNetwork.IsMasterClient)
			{
				if (properties.ContainsKey(RoomProperties.StartTime))
				{
					double updatedStartTime = (double)properties[RoomProperties.StartTime];
					DebugInfo.AppendLog($"[Client] UpdatedRoomStartTime {updatedStartTime}");
					Start(updatedStartTime);
				}
			}
		}

		#endregion
	}
}
