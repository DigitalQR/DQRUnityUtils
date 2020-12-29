using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DQR.Types;

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

		public PropertyPrefBool ShouldLogToFile { get; private set; }
		public PropertyPrefBool ShouldLogToConsole { get; private set; }
		public PropertyPrefBool ShouldLogToUnityOutput { get; private set; }

		public LogCategory()
		{
			ShouldLogToFile = new PropertyPrefBool(PlayerPrefPrefix + ".LogToFile", true);
			ShouldLogToConsole = new PropertyPrefBool(PlayerPrefPrefix + ".LogToConsole", true);
			ShouldLogToUnityOutput = new PropertyPrefBool(PlayerPrefPrefix + ".LogToUnityOutput", true);
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

		public virtual void HandleMessage(LogMessageType logType, string message)
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
