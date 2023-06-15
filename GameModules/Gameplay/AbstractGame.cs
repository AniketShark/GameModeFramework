using GameModules;
using UnityEngine;

public abstract class AbstractGame : MonoBehaviour
{
	//protected Mode _mode;
	//public Mode Mode { get { return _mode; } }
	public abstract void RespawnPlayer(float delay);
}
