using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DQR
{
	public enum LogMessageType
	{
		Info,
		Warning,
		Error,
	}

	public abstract class LogCategory
	{
		protected string PlayerPrefPrefix
		{
			get => "DQR.Log." + GetType().Name;
		}

		public bool ShouldLogToFile
		{
			get => PlayerPrefs.GetInt(PlayerPrefPrefix + ".LogToFile", 1) == 1;
			set => PlayerPrefs.SetInt(PlayerPrefPrefix + ".LogToFile", value ? 1 : 0);
		}

		public bool ShouldLogToConsole
		{
			get => PlayerPrefs.GetInt(PlayerPrefPrefix + ".LogToConsole", 1) == 1;
			set => PlayerPrefs.SetInt(PlayerPrefPrefix + ".LogToConsole", value ? 1 : 0);
		}

		public bool ShouldLogToUnityOutput
		{
			get => PlayerPrefs.GetInt(PlayerPrefPrefix + ".LogToUnityOutput", 0) == 1;
			set => PlayerPrefs.SetInt(PlayerPrefPrefix + ".LogToUnityOutput", value ? 1 : 0);
		}

		public virtual string GetCategoryName()
		{
			string typeName = GetType().Name;
			if (typeName.EndsWith("LogCategory", System.StringComparison.CurrentCultureIgnoreCase))
				return typeName.Substring(0, typeName.Length - "LogCategory".Length);
			return typeName;
		}

		public virtual Color GetCategoryColour()
		{
			return Color.green;
		}

		public void HandleMessage(LogMessageType logType, string message)
		{
			if (ShouldLogToFile)
			{
				// TODO - 
			}

			if (ShouldLogToConsole)
			{
				LogMessage msg = new LogMessage { Category = this, LogType = logType, Message = message, Timestamp = System.DateTime.Now };
				Log.LogInternal(msg);
			}

			if (ShouldLogToUnityOutput)
			{
				switch (logType)
				{
					case LogMessageType.Error:
						Debug.LogError(message);
						break;

					case LogMessageType.Warning:
						Debug.LogWarning(message);
						break;

					default:
						Debug.Log(message);
						break;
				}
			}
		}
	}
}
