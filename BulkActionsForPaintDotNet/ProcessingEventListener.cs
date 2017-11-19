using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PDNBulkUpdater;

namespace PDNBulkUpdater
{
	public class ProcessingEventListener : IProcessingEventListener
	{
		bool m_isProcessing;
		int m_numImages;
		int m_curImage;

		public ProcessingEventListener()
		{
			this.Verbosity = PDNBulkUpdater.Verbosity.Everything;
		}

		private void WriteProgress()
		{
			if(m_numImages == 0)
			{
				Console.Write("Processing Images: 100%");
			}
			else
			{
				Console.Write("Processing Images: {0}%", ((int)((float)m_curImage / m_numImages * 100f)).ToString());
			}

			Console.Out.Flush();
		}

		#region IProcessingEventListener Members

		public Verbosity Verbosity
		{
			get;
			set;
		}

		public void LogInfo(string msg, params object[] parms)
		{
			if((this.Verbosity & PDNBulkUpdater.Verbosity.Info) != PDNBulkUpdater.Verbosity.None)
			{
				string final = string.Format(msg, parms);
				System.Diagnostics.Debug.WriteLine(final);

				if(m_isProcessing)
				{
					Console.CursorLeft = 0;
					Console.WriteLine(final);
					WriteProgress();
				}
				else
				{
					Console.WriteLine(final);
				}
			}
		}

		public void LogWarning(string msg, params object[] parms)
		{
			if((this.Verbosity & PDNBulkUpdater.Verbosity.Warnings) != PDNBulkUpdater.Verbosity.None)
			{
				string final = string.Format("Warning: " + msg, parms);
				System.Diagnostics.Debug.WriteLine(final);

				if(m_isProcessing)
				{
					Console.CursorLeft = 0;
					Console.WriteLine(final);
					WriteProgress();
				}
				else
				{
					Console.WriteLine(final);
				}
			}
		}

		public void LogError(string msg, params object[] parms)
		{
			if((this.Verbosity & PDNBulkUpdater.Verbosity.Errors) != PDNBulkUpdater.Verbosity.None)
			{
				string final = string.Format("Error: " + msg, parms);
				System.Diagnostics.Debug.WriteLine(final);

				if(m_isProcessing)
				{
					Console.CursorLeft = 0;
					Console.WriteLine(final);
					WriteProgress();
				}
				else
				{
					Console.WriteLine(final);
				}
			}
		}

		public void LogException(Exception ex)
		{
			if((this.Verbosity & PDNBulkUpdater.Verbosity.Exceptions) != PDNBulkUpdater.Verbosity.None)
			{
				string msg = ex.ToString();
				System.Diagnostics.Debug.WriteLine(msg);
				System.Diagnostics.Debug.WriteLine("");

				if(m_isProcessing)
				{
					Console.CursorLeft = 0;
					Console.WriteLine(msg);
					Console.WriteLine("");
					WriteProgress();
				}
				else
				{
					Console.WriteLine(msg);
					Console.WriteLine("");
				}
			}
		}

		public void OnResetProgress(int numImages)
		{
			m_isProcessing = true;
			m_curImage = 0;
			m_numImages = numImages;

			if((this.Verbosity & PDNBulkUpdater.Verbosity.Info) != PDNBulkUpdater.Verbosity.None)
			{
				WriteProgress();
			}
		}

		public void OnUpdateProgress(int newValue)
		{
			m_curImage = newValue;
			
			if((this.Verbosity & PDNBulkUpdater.Verbosity.Info) != PDNBulkUpdater.Verbosity.None)
			{
				Console.CursorLeft = 0;
				WriteProgress();
			}
		}

		public void OnProcessImagesFinished()
		{
			m_isProcessing = false;

			if((this.Verbosity & PDNBulkUpdater.Verbosity.Info) != PDNBulkUpdater.Verbosity.None)
			{
				Console.CursorLeft = 0;
				LogInfo("Processing {0} images complete!", m_numImages.ToString());
			}
		}

		#endregion
	}
}
