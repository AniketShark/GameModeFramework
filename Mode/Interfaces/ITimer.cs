using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameModules
{
	public enum TimerState
	{
		None,
		Running,
		Paused,
		Complete 
	}

	public interface ITimer<T>
	{
		event System.Action<T,T> onSecondTick;
		event System.Action onRoundEnd;

		void Start(T startTime);
		void Stop();
		void Reset();
		void Resume();
		void Pause();
		T TimeElapsed { get; }
		T TimeRemaining { get; }
		T CurrentTimestamp { get; }
		TimerState State { get; }
	}
}
