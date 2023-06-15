using UnityEngine;

namespace GameModules
{
	public class PlayerInfo
	{
		public string name;
		public Color color;
		public string clan;
		public int id { get; set; }
		public int team { get; set; }
		public string teamName { get; set; }

		private PlayerInfo()
		{
		}

		public PlayerInfo(int playerId, int team, string teamName, Color color, string name = "player")
		{
			this.name = name;
			this.id = playerId;
			this.team = team;
			this.teamName = teamName;
			this.color = color;
		}
	}
}

