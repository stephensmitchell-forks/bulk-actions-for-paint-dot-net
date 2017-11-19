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
	/// Interaction logic for SaveTokenPage.xaml
	/// </summary>
	public partial class SaveTokenPage : Page
	{
		LinkedListNode<FileTypeSaveTokenPair> m_token;
		UpdateContext m_ctx;

		public SaveTokenPage(LinkedListNode<FileTypeSaveTokenPair> token, UpdateContext ctx)
			: this()
		{
			m_token = token;
			m_ctx = ctx;

			System.Windows.Forms.Control widget = m_host.Child = m_token.Value.CreateWidget();
			
			System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel();
			panel.AutoScroll = true;
			panel.Controls.Add(widget);
			
			m_tbTitle.Text = this.Title = m_token.Value.FileType.Name + " Settings";
			m_host.Child = panel;
		}

		public SaveTokenPage()
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
				if(m_token.Next == null)
				{
					NavigationService.Navigate(new UpdateItemsPage(m_token.List, m_ctx));
				}
				else
				{
					NavigationService.Navigate(new SaveTokenPage(m_token.Next, m_ctx));
				}
			}
		}

		private void OnBackButton_Click(object sender, RoutedEventArgs e)
		{
			NavigationService.GoBack();
		}
	}
}
