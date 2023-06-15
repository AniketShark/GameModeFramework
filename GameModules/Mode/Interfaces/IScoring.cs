using System;
using System.Collections;

namespace GameModules
{
	public class ScoringProps
	{
		public static readonly string MaxScore = "msc";
		public static readonly string TeamsCount = "tmc";
		public static readonly string Players = "pls";
		public static readonly string TeamScoreContribution = "tsc";
		public static readonly string PersonalScoreContribution = "psc";
		public static readonly string PersonalScore = "psc";
		public static readonly string TotalScoreContribution = "ttsc";
		public static readonly string ScoreMap = "scm";
	}

	public delegate void ScoreUpdateDelegate(string id, int score, bool result);

	public interface IScoring<T> : IDisposable
	{
		event Action<int,int,bool> onTeamScoreUpdate;
		event Action<int,int,bool> onPersonalScoreUpdate;
		void Init(Hashtable data);
		void AddScore(int id,T score);
		T GetScoreFor(int Id);
		T GetLeader();
		void Reset();
	}
}
