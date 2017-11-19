﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace PaintDotNet.Effects
{
	public class PDNBulkUpdaterEffect : PaintDotNet.Effects.Effect
	{
		const string MENU_TXT = "Bulk Processing...";

		static System.Windows.Forms.Form m_mainWindow;
		static Exception m_loaderException = null;

		static PDNBulkUpdaterEffect()
		{
			try
			{
				if(System.Windows.Forms.Application.OpenForms.Count > 0)
				{
					m_mainWindow = System.Windows.Forms.Application.OpenForms[System.Windows.Forms.Application.OpenForms.Count - 1];

					while(m_mainWindow.Owner != null)
					{
						m_mainWindow = m_mainWindow.Owner;
					}
				}

				foreach(System.Windows.Forms.Control ctrl in m_mainWindow.Controls)
				{
					System.Windows.Forms.UserControl workSpace = ctrl as System.Windows.Forms.UserControl;

					if(workSpace != null)
					{
						PropertyInfo info = workSpace.GetType().GetProperty("ToolBar");

						if(info != null)
						{
							System.Windows.Forms.Control toolbar = info.GetGetMethod().Invoke(workSpace, new object[0]) as System.Windows.Forms.Control;

							if(toolbar != null)
							{
								info = toolbar.GetType().GetProperty("MainMenu");

								if(info != null)
								{
									System.Windows.Forms.MenuStrip menu = info.GetGetMethod().Invoke(toolbar, new object[0]) as System.Windows.Forms.MenuStrip;

									if(menu != null)
									{
										System.Windows.Forms.ToolStripMenuItem fileItem = menu.Items[0] as System.Windows.Forms.ToolStripMenuItem;
										fileItem.DropDownOpening += new EventHandler(fileItem_DropDownOpening);
									}
								}
							}
						}

						break;
					}
				}
			}
			catch(Exception ex)
			{
				m_loaderException = ex;
				System.Diagnostics.Debug.WriteLine(ex.ToString());
			}
		}

		static void fileItem_DropDownOpening(object sender, EventArgs e)
		{
			try
			{
				System.Windows.Forms.ToolStripMenuItem fileItem = sender as System.Windows.Forms.ToolStripMenuItem;
				Type pdnMenuItemType = fileItem.DropDownItems[0].GetType();
				System.Windows.Forms.ToolStripMenuItem menuItem = pdnMenuItemType.Assembly.CreateInstance(pdnMenuItemType.FullName) as System.Windows.Forms.ToolStripMenuItem;

				if(menuItem != null)
				{
					menuItem.Text = MENU_TXT;
					menuItem.Click += new EventHandler(OnBulkConvertClick);

					fileItem.DropDownItems.Insert(fileItem.DropDownItems.Count - 1, menuItem);
					fileItem.DropDownItems.Insert(fileItem.DropDownItems.Count - 1, new System.Windows.Forms.ToolStripSeparator());
				}
			}
			catch(Exception ex)
			{
				m_loaderException = ex;
				System.Diagnostics.Debug.WriteLine(ex.ToString());
			}
		}

		static void OnBulkConvertClick(object sender, EventArgs e)
		{
			try
			{
				PDNBulkUpdater.BulkUpdateWindow window = new PDNBulkUpdater.BulkUpdateWindow();
				System.Windows.Interop.WindowInteropHelper hlpr = new System.Windows.Interop.WindowInteropHelper(window);
				hlpr.Owner = m_mainWindow == null ? IntPtr.Zero : m_mainWindow.Handle;

				try
				{
					window.ShowDialog();
				}
				catch(Exception ex)
				{
					PaintDotNet.Utility.ErrorBox(m_mainWindow, ex.ToString());

					if(window.IsVisible)
					{
						window.Close();
					}
				}
			}
			catch(Exception ex)
			{
				PaintDotNet.Utility.ErrorBox(m_mainWindow, ex.ToString());
			}
		}

		public PDNBulkUpdaterEffect()
			: base("Bulk Updater", null, EffectFlags.None)
		{
			if(m_loaderException == null)
			{
				throw new NotImplementedException("This exception has been generated by the stub loader for PDNBulkUpdater. Please disregard.");
			}
			else
			{
				// NOTE: Marshals the exception to the plugin loader error dialog box
				throw m_loaderException;
			}
		}

		public override void Render(EffectConfigToken parameters, PaintDotNet.RenderArgs dstArgs, PaintDotNet.RenderArgs srcArgs, System.Drawing.Rectangle[] rois, int startIndex, int length)
		{
			if(m_loaderException == null)
			{
				throw new NotImplementedException("This exception has been generated by the stub loader for PDNBulkUpdater. Please disregard.");
			}
			else
			{
				// NOTE: Marshals the exception to the plugin loader error dialog box
				throw m_loaderException;
			}
		}
	}
}