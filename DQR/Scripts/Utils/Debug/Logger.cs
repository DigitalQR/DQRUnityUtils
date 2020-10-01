using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LogCategory
{
	public virtual string GetCategoryName()
	{
		string typeName = GetType().Name;
		if (typeName.EndsWith("LogCategory", System.StringComparison.CurrentCultureIgnoreCase))
			return typeName.Substring(0, typeName.Length - "LogCategory".Length);
		return typeName;
	}
}

public static class Logger
{
	private static void LogMessage(LogCategory category, string fmt, params object[] args)
	{
		//ILogger
		//Debug.unityLogger.

		string message = string.Format("[" + category.GetCategoryName() + "]: " + fmt, args);
		Debug.Log(message);
	}

	public static void Message<Category>(string fmt, params object[] args) where Category : LogCategory
	{

	}
}
