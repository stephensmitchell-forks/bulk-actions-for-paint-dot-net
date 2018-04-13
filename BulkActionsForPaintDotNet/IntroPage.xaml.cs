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
            // Trim un-required sub-version data
            while (version.EndsWith(".0"))
                version = version.Substring(0, version.Length - 2);
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
