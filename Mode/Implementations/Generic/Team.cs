using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameModules.Teams
{
	public class TeamProps
	{
		public static readonly string TeamCode = "tc";
		public static readonly string TeamName = "tn";
		public static readonly string TeamSlot = "ts";
	}

	public class TeamColors
	{
		public static readonly Dictionary<string,Color> Colors = new Dictionary<string, Color>()
		{
			{"Blue",new Color32(30, 167, 225, 255)},
			{"Red",new Color32(214, 24, 120, 255)},
			{"Cyan",Color.cyan},
			{"Green",Color.green},
			{"Grey",Color.grey},
			{"Magenta",Color.magenta},
			{"Black",Color.black},
			{"Yellow",Color.yellow},
			{"NIP",Color.HSVToRGB(189,100,100)},
			{"Fanatic",Color.HSVToRGB(270,100,100)},
			{"Envyus",Color.HSVToRGB(160,100,100)},
			{"Cloud9",Color.HSVToRGB(90,100,100)},
			{"Neantic",Color.HSVToRGB(55,100,100)},
			{"Frostys",Color.HSVToRGB(33,100,100)},
			{"KillMongers",Color.HSVToRGB(136,100,100)},
			{"Ripers",Color.HSVToRGB(240,100,100)},
			{"Hooligans",Color.HSVToRGB(153,100,100)},
			{"Anonymous",Color.HSVToRGB(100,100,100)},
			{"Hacker80",Color.HSVToRGB(210,100,100)},
			{"SuperGiants",Color.HSVToRGB(300,100,100)}
		};
	}

	public class Team
	{
		public string name;
		public int code;
		public Color color;
		private int _maxPlayers;
		private List<string> _slotKeys;
		public int OpenSlots { get; private set; }
		public int MaxSlots { get { return _maxPlayers; } }
		private Team() { }
		public Team(string name, int code, int maxPlayers,Color color)
		{
			this.name = name;
			this.code = code;
			this._maxPlayers = maxPlayers;
			this.color = color;
			_slotKeys = new List<string>();

			for(int i = 0; i < maxPlayers; i++)
			{
				string slotKey = code.ToString() + "_" + i;
				_slotKeys.Add(slotKey);
			}
		}
		public void UpdateMaxPlayers(int maxPlayers)
		{
			_maxPlayers = maxPlayers;
		}
		public void ResetOpenSlots()
		{
			OpenSlots = _maxPlayers;
		}
		public bool DecreamentSlots()
		{
			//UnityEngine.Debug.LogError("DecreamentSlots");
			OpenSlots -= 1;
			if (OpenSlots < 0)
			{
				OpenSlots = 0;
				return false;
			}
			return true;
		}
		public bool IncreamentSlots()
		{
			OpenSlots += 1;
			if (OpenSlots > _maxPlayers)
			{
				OpenSlots = _maxPlayers;
				return false;
			}
			return true;
		}
		public override string ToString()
		{
			return string.Format("name {0} code [{1}] max {2} count {3}", name, code, _maxPlayers, OpenSlots);
		}

		public string GetSlotKey(int slotIndex)
		{
			return _slotKeys[slotIndex];
		}

		public List<string> GetAllSlotKeys()
		{
			return _slotKeys;
		}

	}
}
