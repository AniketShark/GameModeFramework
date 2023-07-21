using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameModules.Networking
{
	public enum RoomState
	{
		NOT_READY,
		START,
		WARMUP,
		IN_GAME,
		COOLDOWN,
		END,
		PAUSED,
		LONG_PAUSED
	}

	public interface IRoomState
	{
	    event Action<RoomState> onRoomStateUpdate;

		void Start();
		RoomState Get();
		void Reset();
	}
}
