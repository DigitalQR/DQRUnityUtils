using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DQR
{
	public class DefaultLogCategory : LogCategory { }

	public struct LogMessage
	{
		public LogCategory Category;
		public LogMessageType LogType;
		public string Message;
		public System.DateTime Timestamp;
		
		public override string ToString()
		{
			string time = Timestamp.ToShortTimeString();

			switch (LogType)
			{
				case LogMessageType.Error:
					return "[" + time + "][ERROR] " + Category.GetCategoryName() + ": " + Message;

				case LogMessageType.Warning:
					return "[" + time + "][WARNING] " + Category.GetCategoryName() + ": " + Message;

				default:
					return "[" + time + "]" + Category.GetCategoryName() + ": " + Message;
			}
		}
	}

	public static class Log
	{
		#region Categories
		private static Dictionary<System.Type, LogCategory> s_CategoryInstances = new Dictionary<System.Type, LogCategory>();

		public static LogCategory GetCategoryInstance<Category>() where Category : LogCategory, new()
		{
			var key = typeof(Category);
			LogCategory category = null;

			if (s_CategoryInstances.TryGetValue(key, out category))
				return category;

			category = new Category();
			s_CategoryInstances.Add(key, category);
			return category;
		}

		public static IEnumerable<LogCategory> GetDiscoveredCategories()
		{
			return s_CategoryInstances.Values;
		}
		#endregion

		#region Log Message
		private const int c_LogCapacity = 1024 * 8;
		private static LogMessage[] s_LogBuffer = new LogMessage[c_LogCapacity];
		private static int s_LogTailIndex = 0;

		public static IEnumerable<LogMessage> GetLogMessages()
		{
			return (s_LogBuffer.Skip(s_LogTailIndex).Union(s_LogBuffer.Take(s_LogTailIndex))).Where((msg) => msg.Category != null);
		}

		internal static void LogInternal(LogMessage message)
		{
			s_LogBuffer[s_LogTailIndex] = message;
			s_LogTailIndex = (s_LogTailIndex + 1) % c_LogCapacity;
		}

		public static void Clear()
		{
			s_LogBuffer = new LogMessage[c_LogCapacity];
			s_LogTailIndex = 0;
		}
		#endregion

		public static void Info<Category>(string fmt, params object[] args) where Category : LogCategory, new()
		{
			GetCategoryInstance<Category>().HandleMessage(LogMessageType.Info, string.Format(fmt, args));
		}

		public static void Warning<Category>(string fmt, params object[] args) where Category : LogCategory, new()
		{
			GetCategoryInstance<Category>().HandleMessage(LogMessageType.Warning, string.Format(fmt, args));
		}

		public static void Error<Category>(string fmt, params object[] args) where Category : LogCategory, new()
		{
			GetCategoryInstance<Category>().HandleMessage(LogMessageType.Error, string.Format(fmt, args));
		}
	}
}
