using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PDNBulkUpdater
{
	/// <summary>
	/// Interaction logic for UpdateItemsPage.xaml
	/// </summary>
	public partial class UpdateItemsPage : Page, IProcessingEventListener
	{
		delegate void LogStringDelegate(string msg, Brush foreground);

		enum ProcessingState
		{
			BeginProcessing = 0,
			CurrentlyProcessing,
			DoneProcessing
		}

		UpdateContext m_ctx;
		Dictionary<string, FileTypeSaveTokenPair> m_saveTokens = new Dictionary<string, FileTypeSaveTokenPair>();
		System.Threading.Thread m_thread;
		ProcessingState m_state;
		Verbosity m_verbosity = Verbosity.Everything;

		public UpdateItemsPage(LinkedList<FileTypeSaveTokenPair> saveTokens, UpdateContext ctx)
			: this()
		{
			m_ctx = ctx;

			foreach(FileTypeSaveTokenPair token in saveTokens)
			{
				m_saveTokens[token.FileType.Name] = token;
			}
		}

		public UpdateItemsPage()
		{
			InitializeComponent();
		}

		private void OnFinishButton_Click(object sender, RoutedEventArgs e)
		{
			switch(m_state)
			{
				case ProcessingState.BeginProcessing:
					{
						m_state = ProcessingState.CurrentlyProcessing;
						m_btnBack.IsEnabled = false;
						m_btnFinish.Content = "Cancel";
						m_tbDesc.Visibility = System.Windows.Visibility.Collapsed;
						m_gridProcessing.Visibility = System.Windows.Visibility.Visible;
						m_lblProgress.Content = "Enumerating images...";

						m_thread = new System.Threading.Thread(OnProcessImages);
						m_thread.Name = "UpdateItemsPage.OnProcessImages";
						m_thread.IsBackground = true;
						m_thread.Start();

						break;
					}
				case ProcessingState.CurrentlyProcessing:
					{
						if(m_thread.IsAlive)
						{
							m_thread.Abort();
						}

						m_state = ProcessingState.BeginProcessing;
						m_btnBack.IsEnabled = true;
						m_btnFinish.Content = "Next";
						m_tbDesc.Visibility = System.Windows.Visibility.Visible;
						m_gridProcessing.Visibility = System.Windows.Visibility.Collapsed;
						m_lblProgress.Content = "";

						break;
					}
				case ProcessingState.DoneProcessing:
					{
						NavigationWindow window = this.Parent as NavigationWindow;

						if(window != null)
						{
							window.Close();
						}
						break;
					}
			}
		}

		void OnProcessImages()
		{
			bool callOnFinished = false;

			try
			{
				Util.ProcessImages(m_ctx, m_saveTokens, this);
			}
			catch(System.Threading.ThreadAbortException)
			{
				callOnFinished = true;
			}
			catch(System.Threading.ThreadInterruptedException ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.ToString());
				callOnFinished = true;
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.ToString());
				callOnFinished = true;
			}
			finally
			{
				if(!this.Dispatcher.HasShutdownStarted && callOnFinished)
				{
					OnProcessImagesFinished();
				}
			}
		}

		void ResetProgress(int numImages)
		{
			m_progress.Minimum = 0.0;
			m_progress.Value = 0.0;
			m_progress.Maximum = (double)numImages;

			m_lblProgress.Content = "Processing images...";
		}

		void UpdateProgress(int progress)
		{
			m_progress.Value = (double)progress;
		}

		void OnImagesFinished()
		{
			m_state = ProcessingState.DoneProcessing;
			m_btnFinish.Content = "Finish";
			m_tbDesc.Visibility = System.Windows.Visibility.Collapsed;
			m_tbFinishedDesc.Visibility = System.Windows.Visibility.Visible;
			m_stackProgress.Visibility = System.Windows.Visibility.Collapsed;
			m_lblProgress.Content = "";
		}

		private void OnBackButton_Click(object sender, RoutedEventArgs e)
		{
			m_state = ProcessingState.BeginProcessing;
			NavigationService.GoBack();
		}

		public void AbortUpdate()
		{
			if(m_thread != null && m_thread.IsAlive)
			{
				m_thread.Abort();
			}
		}

		void LogString(string msg, Brush foreground)
		{
			Run run = new Run(msg);

			if(foreground != null)
			{
				run.Foreground = foreground;
			}

			m_parOutput.Inlines.Add(run);
			m_parOutput.Inlines.Add(new LineBreak());

			m_txtOutput.ScrollToEnd();
		}

		#region IProcessingEventListener Members

		public Verbosity Verbosity
		{
			get
			{
				return m_verbosity;
			}
			set
			{
				m_verbosity = value;
			}
		}

		public void LogInfo(string msg, params object[] parms)
		{
			this.Dispatcher.BeginInvoke(new LogStringDelegate(LogString), string.Format(msg, parms), null);
		}

		public void LogWarning(string msg, params object[] parms)
		{
			this.Dispatcher.BeginInvoke(new LogStringDelegate(LogString), string.Format(msg, parms), Brushes.Orange);
		}

		public void LogError(string msg, params object[] parms)
		{
			this.Dispatcher.BeginInvoke(new LogStringDelegate(LogString), string.Format(msg, parms), Brushes.Red);
		}

		public void LogException(Exception ex)
		{
			this.Dispatcher.BeginInvoke(new LogStringDelegate(LogString), ex.ToString(), Brushes.Violet);
		}

		public void OnResetProgress(int numImages)
		{
			this.Dispatcher.BeginInvoke(new ProgressDelegate(ResetProgress), numImages);
		}

		public void OnUpdateProgress(int newValue)
		{
			this.Dispatcher.BeginInvoke(new ProgressDelegate(UpdateProgress), newValue);
		}

		public void OnProcessImagesFinished()
		{
			this.Dispatcher.BeginInvoke(new ProcessImagesFinishedDelegate(OnImagesFinished));
		}

		#endregion
	}
}
