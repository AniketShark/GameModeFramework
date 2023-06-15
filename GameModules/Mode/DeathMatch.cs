using System;
using System.Collections;
using System.Collections.Generic;
using GameModules;
using GameModules.Networking;
using GameModules.Networking.Implementations.PhotonV2;
using GameModules.Teams;
using UnityEngine;

public class DeathMatch : Mode
{
	public DeathMatch(Hashtable modeParameters)
		: base(modeParameters)
	{
		// All photon callbacks handled by this object and propogated to 
		// member components of this (Mode).
		CallbackHandler = new PhotonCallbackHandler();

		int maxTeams = (int)modeParameters[ModeProps.MaxTeams];
		int maxPlayersPerTeam = (int)modeParameters[ModeProps.MaxPlayersPerTeam];

		TeamAssigner = new DMTeamAssigner(CallbackHandler,
			maxTeams,
			maxPlayersPerTeam,
			new BalancedTeamStrategy(CallbackHandler));

		Timer = new PhotonTimer(CallbackHandler,
			(int)modeParameters[ModeProps.WarmupTime],
			(int)modeParameters[ModeProps.RoundTime],
			0);

		Spawner = new DMSpawner(CallbackHandler, new DMSpawnStrategy());
		Scoring = new DMKillsToWin(CallbackHandler);
		State = new DMRoomState(Timer, Scoring);
	}

	public override void Start()
	{
		CallbackHandler.Listen();
	}

	public override void JoinMatch(Hashtable arguments)
	{
		Scoring.Init(arguments);
		Timer.Start((double)arguments[RoomProperties.StartTime]);
		State.Start();
	}

	public override void Reset()
	{
		CallbackHandler.Ignore();
		Scoring.Reset();
		Timer.Stop();
		State.Reset();
	}
}

