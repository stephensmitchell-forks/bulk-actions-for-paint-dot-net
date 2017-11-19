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
	/// Interaction logic for IntroPage.xaml
	/// </summary>
	public partial class IntroPage : Page
	{
		public IntroPage()
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

		private void OnDonateImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.NavigationService.Navigate(new DonatePage());
		}
	}
}
