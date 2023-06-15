using GameModules;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeManager
{
	public static Dictionary<ModeTypes, Hashtable> configs = new Dictionary<ModeTypes, Hashtable>()
	{
		{ModeTypes.TeamDeathMatch,
		new Hashtable(){
				{ModeProps.MaxPlayers,10},
				{ModeProps.MaxTeams,2},
				{ModeProps.MaxPlayersPerTeam,5},
				{ModeProps.WarmupTime,20},
				{ModeProps.RoundTime,300},
				{ModeProps.Cooldown,0},
				{ModeProps.MaxScore,200},
				{ModeProps.TeamLayout,
				new Dictionary<string,string>()
				{
					{"0_0","~"},
					{"0_1","~"},
					{"0_2","~"},
					{"0_3","~"},
					{"0_4","~"},
					{"1_0","~"},
					{"1_1","~"},
					{"1_2","~"},
					{"1_3","~"},
					{"1_4","~"}
				}}}
		}
	};
	private static ModeManager _instance;
	private static Dictionary<ModeTypes, Mode> _modeRegistry;

	private ModeManager()
	{
		_modeRegistry = new Dictionary<ModeTypes, Mode>();
		var tdm = new TeamDeathMatch(configs[ModeTypes.TeamDeathMatch]);
		_modeRegistry.Add(ModeTypes.TeamDeathMatch, tdm);
	}

	public static ModeManager Instance
	{

		get
		{
			if (_instance == null)
				_instance = new ModeManager();
			return _instance;
		}
	}

	public Mode GetMode(ModeTypes type)
	{
		return _modeRegistry[type];
	}
}
