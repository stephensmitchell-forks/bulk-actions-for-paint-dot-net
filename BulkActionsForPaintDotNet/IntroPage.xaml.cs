using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

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

            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            TextBlockDescription.Text = TextBlockDescription.Text.Replace("[VERSION]", version);
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
	}
}
