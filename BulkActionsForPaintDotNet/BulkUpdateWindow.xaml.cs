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
using System.Windows.Shapes;
using System.Windows.Navigation;

namespace PDNBulkUpdater
{
	/// <summary>
	/// Interaction logic for BulkUpdateWindow.xaml
	/// </summary>
	public partial class BulkUpdateWindow : NavigationWindow
	{
		public BulkUpdateWindow()
		{
			InitializeComponent();
		}

		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			UpdateItemsPage page = NavigationService.Content as UpdateItemsPage;

			if(page != null)
			{
				page.AbortUpdate();
			}

			base.OnClosing(e);
		}

		private void OnNavigating(object sender, NavigatingCancelEventArgs e)
		{
			if(e.Uri != null)
			{
				e.Cancel = true;

				try
				{
					System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
					info.UseShellExecute = true;
					info.Verb = "open";
					info.FileName = e.Uri.ToString();

					using(System.Diagnostics.Process proc = System.Diagnostics.Process.Start(info))
					{
					}
				}
				catch(Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.ToString());
				}
			}
		}
	}
}
