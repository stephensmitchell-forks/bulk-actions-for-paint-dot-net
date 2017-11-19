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
using System.Net;

namespace PDNBulkUpdater
{
	/// <summary>
	/// Interaction logic for DonatePage.xaml
	/// </summary>
	public partial class DonatePage : Page
	{
		public DonatePage()
		{
			InitializeComponent();
		}

		private void OnNextButton_Click(object sender, RoutedEventArgs e)
		{
			if(NavigationService.CanGoForward)
			{
				NavigationService.GoForward();
			}
			else
			{
				NavigationService.Navigate(new AddItemsPage());
			}
		}

		private void OnFrame_Navigated(object sender, NavigationEventArgs e)
		{
			m_btnNext.IsEnabled = true;
		}

		private void OnFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
		{
			m_btnNext.IsEnabled = true;
		}
	}
}
