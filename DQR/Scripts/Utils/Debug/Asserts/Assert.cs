using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Assertions;

namespace DQR
{
	public static class Assert
	{
		private static void LogAssertion(string msg)
		{
#if DQR_ASSERTS
			StackTrace trace = new StackTrace(2, true);

			AssertRecord record = AssertRecordController.SelectAssertRecord(trace, msg);
			record.HandleAssertFire(trace);
#endif
		}

		[Conditional("DQR_ASSERTS")]
		public static void Condition(bool cond)
		{
			if (!cond)
				LogAssertion(null);
		}

		[Conditional("DQR_ASSERTS")]
		public static void Message(bool cond, string msg)
		{
			if (!cond)
				LogAssertion(msg);
		}

		[Conditional("DQR_ASSERTS")]
		public static void Format(bool cond, string format, params object[] args)
		{
			if (!cond)
				LogAssertion(string.Format(format, args));
		}

		[Conditional("DQR_ASSERTS")]
		public static void Fail()
		{
			LogAssertion(null);
		}

		[Conditional("DQR_ASSERTS")]
		public static void FailMessage(string msg)
		{
			LogAssertion(msg);
		}

		[Conditional("DQR_ASSERTS")]
		public static void FailFormat(string format, params object[] args)
		{
			LogAssertion(string.Format(format, args));
		}
	}
}
