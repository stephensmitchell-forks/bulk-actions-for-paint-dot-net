using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDNBulkUpdater
{
	[Flags]
	public enum Verbosity
	{
		None = 0,
		Info = 1,
		Warnings = 1 << 1,
		Errors = 1 << 2,
		Exceptions = 1 << 3,
		Everything = Info | Warnings | Errors | Exceptions
	}

	public delegate void ProgressDelegate(int value);
	public delegate void ProcessImagesFinishedDelegate();

	public interface IProcessingEventListener
	{
		Verbosity Verbosity
		{
			get;
			set;
		}

		void LogInfo(string msg, params object[] parms);
		void LogWarning(string msg, params object[] parms);
		void LogError(string msg, params object[] parms);
		void LogException(Exception ex);
		void OnResetProgress(int numImages);
		void OnUpdateProgress(int newValue);
		void OnProcessImagesFinished();
	}
}
