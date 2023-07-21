using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugInfo : MonoBehaviour
{
	public static DebugInfo Instance;
	[SerializeField]
	private Text m_LogText;

	void Awake()
	{
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public static void AppendLog(string log)
	{
		if (Instance == null)
			return;
		Instance.m_LogText.text += "\n" + log;
	}

	public static void Clean()
	{
		if (Instance == null)
			return;

		Instance.m_LogText.text = "";
	}
}
