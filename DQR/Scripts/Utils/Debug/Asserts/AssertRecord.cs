using DQR;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

#if DQR_ASSERTS
public class AssertRecord
{
	private string m_Message;
	private string m_FilePath;
	private int m_LineNumber;

	private bool m_IsMuted;

	public AssertRecord(StackFrame frame, string providedMessage)
	{
		m_Message = providedMessage;

		m_FilePath = frame.GetFileName();
		m_LineNumber = frame.GetFileLineNumber();

		m_IsMuted = false;
		
		if(string.IsNullOrWhiteSpace(m_Message))
			m_Message = "Assert condition failed in " + frame.GetMethod().ToString();
	}

	public void HandleAssertFire(StackTrace trace)
	{
		string liteMessage = "Assert: '" + m_Message + "' @(" + m_FilePath + ":" + m_LineNumber + ")";
		string fullMessage = liteMessage + "\n\t" + GetFormattedStackTrace(trace, true);
		
		UnityEngine.Debug.LogFormat(LogType.Assert, LogOption.None, null, fullMessage);

		if (m_IsMuted)
			return;

		int result = -1;

		if (DQRUtils.InMainThread)
		{
			if (Debugger.IsAttached)
				result = UnityEditor.EditorUtility.DisplayDialogComplex("ASSERT", liteMessage, "Continue", "Mute", "Debug Break");
			else
				result = UnityEditor.EditorUtility.DisplayDialogComplex("ASSERT", liteMessage, "Continue", "Mute", null);
		}
		else
		{
			if (Debugger.IsAttached)
				result = 2;
			else
				result = 0;
		}

		switch (result)
		{
			// Continue
			case 0:
				break;

			// Mute
			case 1:
				m_IsMuted = true;
				break;

			// Debug Break
			case 2:
				Debugger.Break();
				break;

			default:
				throw new UnityEngine.Assertions.AssertionException(fullMessage, liteMessage);
		}
	}
	
	private static string GetFormattedStackTrace(StackTrace trace, bool cullInternalFrames)
	{
		bool IsInternalFrame(StackFrame frame)
		{
			var method = frame.GetMethod();
			string ns = method.ReflectedType.Namespace ?? "";

			return (ns.StartsWith("Unity"));
		}
		
		string stackTrace = "";
		int internalCallCounter = 0;

		for (int i = 0; i < trace.FrameCount; ++i)
		{
			StackFrame frame = trace.GetFrame(i);

			// Collapse internal frames into single line
			if (cullInternalFrames && IsInternalFrame(frame))
			{
				internalCallCounter++;
				continue;
			}
			else if (internalCallCounter != 0)
			{
				stackTrace += "..." + internalCallCounter + " internal frames...";
				internalCallCounter = 0;
			}

			string fileName = (frame.GetFileName() ?? "").Replace('\\', '/');

			var method = frame.GetMethod();
			string functionName = "[" + method.ReflectedType.Namespace + ":" + method.ReflectedType.Name + "] " + method.ToString();

			stackTrace += functionName + " (at " + fileName + ":" + frame.GetFileLineNumber() + ")\n";
		}

		// Log trailing frames
		if (internalCallCounter != 0)
		{
			stackTrace += "..." + internalCallCounter + " internal frames...";
			internalCallCounter = 0;
		}

		return stackTrace;
	}
}

public static class AssertRecordController
{
	private struct AssertRecordKey
	{
		public string File;
		public int LineNumber;
		public int ColumnNumber;
	}

	private static Dictionary<AssertRecordKey, AssertRecord> s_HitRecords = new Dictionary<AssertRecordKey, AssertRecord>();

	public static AssertRecord SelectAssertRecord(StackTrace trace, string providedMessage)
	{
		lock (s_HitRecords)
		{
			StackFrame assertFrame = trace.GetFrame(0);
			AssertRecordKey key = new AssertRecordKey
			{
				File = assertFrame.GetFileName(),
				LineNumber = assertFrame.GetFileLineNumber(),
				ColumnNumber = assertFrame.GetFileColumnNumber()
			};

			if (s_HitRecords.TryGetValue(key, out AssertRecord foundRecord))
				return foundRecord;

			AssertRecord newRecord = new AssertRecord(assertFrame, providedMessage);
			s_HitRecords.Add(key, newRecord);
			return newRecord;
		}
	}
}
#endif