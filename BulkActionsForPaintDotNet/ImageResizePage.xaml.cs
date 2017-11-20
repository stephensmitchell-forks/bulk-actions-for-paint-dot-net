using BulkActionsForPaintDotNet;
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
	/// Interaction logic for ImageResizePage.xaml
	/// </summary>
	public partial class ImageResizePage : Page
	{
		const string DBL_FMT = "0.##";

		[Flags]
		enum UpdateState
		{
			None = 0,
			PixelDim = 1,
			PrintDim = 2,
			Both = PixelDim | PrintDim
		}

		UpdateContext m_ctx;
		PaintDotNet.MeasurementUnit m_dpuUnit = PaintDotNet.MeasurementUnit.Inch;
		double m_dpu = PaintDotNet.Document.GetDefaultDpu(PaintDotNet.MeasurementUnit.Inch);
		double m_printWidth;
		double m_printHeight;
		int m_pixelWidth = 800;
		int m_pixelHeight = 600;
		UpdateState m_updateState;

		public ImageResizePage(UpdateContext ctx)
			: this()
		{
			m_ctx = ctx;
		}

		public ImageResizePage()
		{
			InitializeComponent();

			m_comboResampling.Items.Add(PaintDotNet.ResamplingAlgorithm.Bicubic);
			m_comboResampling.Items.Add(PaintDotNet.ResamplingAlgorithm.Bilinear);
			m_comboResampling.Items.Add(PaintDotNet.ResamplingAlgorithm.SuperSampling);
			m_comboResampling.Items.Add(PaintDotNet.ResamplingAlgorithm.NearestNeighbor);
			m_comboResampling.SelectedIndex = 0;

			ComboBoxItem boxItem = (ComboBoxItem)m_comboPixelMeasurement.Items[0];
			boxItem.Tag = PaintDotNet.MeasurementUnit.Inch;

			boxItem = (ComboBoxItem)m_comboPixelMeasurement.Items[1];
			boxItem.Tag = PaintDotNet.MeasurementUnit.Centimeter;

			boxItem = (ComboBoxItem)m_comboPrintMeasurement.Items[0];
			boxItem.Tag = PaintDotNet.MeasurementUnit.Inch;

			boxItem = (ComboBoxItem)m_comboPrintMeasurement.Items[1];
			boxItem.Tag = PaintDotNet.MeasurementUnit.Centimeter;

			m_printWidth = m_pixelWidth / m_dpu;
			m_printHeight = m_pixelHeight / m_dpu;

			m_updateState = UpdateState.Both;
			m_txtPixelWidth.Text = m_pixelWidth.ToString();
			m_txtPixelHeight.Text = m_pixelHeight.ToString();
			m_txtPixelResolution.Text = m_dpu.ToString(DBL_FMT);
			m_txtPrintWidth.Text = m_printWidth.ToString(DBL_FMT);
			m_txtPrintHeight.Text = m_printHeight.ToString(DBL_FMT);
			m_updateState = UpdateState.None;

			UpdateUnits(PaintDotNet.Document.DefaultDpuUnit);
		}

		private void OnBackButton_Click(object sender, RoutedEventArgs e)
		{
			NavigationService.GoBack();
		}

		private void OnNextButton_Click(object sender, RoutedEventArgs e)
		{
			if(m_ctx != null)
			{
				m_ctx.UsePercent = m_radioByPercentage.IsChecked.Value;
				m_ctx.Percent = ParseInt(m_txtPercentage.Text);
				m_ctx.ResamplingAlgorithm = (PaintDotNet.ResamplingAlgorithm)m_comboResampling.SelectedItem;
				m_ctx.NewWidthInPixels = m_pixelWidth;
				m_ctx.NewHeightInPixels = m_pixelHeight;
				m_ctx.Dpu = m_dpu;
				m_ctx.DpuUnit = m_dpuUnit;
			}

            NavigationService.Navigate(new CanvasResizePage(m_ctx));

        }

		private void OnByAbsoluteSize_CheckedChanged(object sender, RoutedEventArgs e)
		{
			if(this.IsInitialized)
			{
				m_txtPixelHeight.IsEnabled = m_radioByAbsSize.IsChecked.Value;
				m_txtPixelResolution.IsEnabled = m_radioByAbsSize.IsChecked.Value;
				m_txtPixelWidth.IsEnabled = m_radioByAbsSize.IsChecked.Value;
				m_txtPrintHeight.IsEnabled = m_radioByAbsSize.IsChecked.Value;
				m_txtPrintWidth.IsEnabled = m_radioByAbsSize.IsChecked.Value;
				m_comboPixelMeasurement.IsEnabled = m_radioByAbsSize.IsChecked.Value;
				m_comboPrintMeasurement.IsEnabled = m_radioByAbsSize.IsChecked.Value;
			}
		}

		private void OnByPercentage_CheckedChanged(object sender, RoutedEventArgs e)
		{
			if(this.IsInitialized)
			{
				m_txtPercentage.IsEnabled = m_radioByPercentage.IsChecked.Value;
			}
		}

		private void OnPixelMeasurement_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBoxItem item = (ComboBoxItem)m_comboPixelMeasurement.SelectedItem;

			if(item.Tag != null)
			{
				UpdateUnits((PaintDotNet.MeasurementUnit)item.Tag);
			}
		}

		void UpdateUnits(PaintDotNet.MeasurementUnit newUnit)
		{
			if(m_dpuUnit != newUnit)
			{
				m_dpuUnit = newUnit;

				m_updateState = UpdateState.Both;

				switch(newUnit)
				{
					case PaintDotNet.MeasurementUnit.Inch:
						{
							m_dpu = PaintDotNet.Document.InchesToCentimeters(m_dpu);
							m_txtPixelResolution.Text = m_dpu.ToString(DBL_FMT);
							m_comboPixelMeasurement.SelectedIndex = 0;
							m_comboPrintMeasurement.SelectedIndex = 0;
							m_lblPrintHeight.Content = "inches";
							break;
						}
					case PaintDotNet.MeasurementUnit.Centimeter:
						{
							m_dpu = PaintDotNet.Document.CentimetersToInches(m_dpu);
							m_txtPixelResolution.Text = m_dpu.ToString(DBL_FMT);
							m_comboPixelMeasurement.SelectedIndex = 1;
							m_comboPrintMeasurement.SelectedIndex = 1;
							m_lblPrintHeight.Content = "centimeters";
							break;
						}
				}
				
				m_updateState = UpdateState.None;

				UpdateUnitMeasurements();
			}
		}

		private void UpdateUnitMeasurements()
		{
			if(this.IsInitialized && m_updateState == UpdateState.None)
			{
				m_printWidth = m_pixelWidth / m_dpu;
				m_printHeight = m_pixelHeight / m_dpu;

				m_updateState = UpdateState.PrintDim;
				m_txtPrintWidth.Text = m_printWidth.ToString(DBL_FMT);
				m_txtPrintHeight.Text = m_printHeight.ToString(DBL_FMT);
				m_updateState = UpdateState.None;
			}
		}

		private void OnPrintMeasurement_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBoxItem item = (ComboBoxItem)m_comboPrintMeasurement.SelectedItem;

			if(item.Tag != null)
			{
				UpdateUnits((PaintDotNet.MeasurementUnit)item.Tag);
			}
		}

		double ParseDouble(string txt)
		{
			if(txt == null || txt == "")
			{
				return 0.0;
			}

			return double.Parse(txt);
		}

		private void OnPixelDim_KeyDown(object sender, KeyEventArgs e)
		{
			if(!Util.IsNumericKey(e.Key) && e.Key != Key.Tab && e.Key != Key.OemBackTab)
			{
				e.Handled = true;
			}
		}

		private void OnPixelDim_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(this.IsInitialized && m_updateState == UpdateState.None)
			{
				if(sender == m_txtPixelWidth)
				{
					m_pixelWidth = ParseInt(m_txtPixelWidth.Text);
				}
				else if(sender == m_txtPixelHeight)
				{
					m_pixelHeight = ParseInt(m_txtPixelHeight.Text);
				}

				UpdateUnitMeasurements();
			}
		}

		private int ParseInt(string value)
		{
			if(value == null || value == "")
			{
				return 0;
			}

			return int.Parse(value);
		}

		private void OnPrintDim_KeyDown(object sender, KeyEventArgs e)
		{
			TextBox box = (TextBox)sender;

			if(((Util.IsDecimalKey(e.Key) && box.Text.Contains('.')) || !Util.IsNumericDecimalKey(e.Key)) && e.Key != Key.Tab && e.Key != Key.OemBackTab)
			{
				e.Handled = true;
			}
		}

		private void OnPrintDim_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(this.IsInitialized && m_updateState == UpdateState.None)
			{
				if(sender == m_txtPrintWidth)
				{
					m_printWidth = ParseDouble(m_txtPrintWidth.Text);
				}
				else if(sender == m_txtPrintHeight)
				{
					m_printHeight = ParseDouble(m_txtPrintHeight.Text);
				}

				UpdatePixelMeasurements();
			}
		}

		private void UpdatePixelMeasurements()
		{
			if(this.IsInitialized && m_updateState == UpdateState.None)
			{
				m_pixelWidth = Util.PrintToPixel(m_dpu, m_printWidth);
				m_pixelHeight = Util.PrintToPixel(m_dpu, m_printHeight);

				m_updateState = UpdateState.PixelDim;
				m_txtPixelWidth.Text = m_pixelWidth.ToString();
				m_txtPixelHeight.Text = m_pixelHeight.ToString();
				m_updateState = UpdateState.None;
			}
		}

		private void OnPixelResolution_TextChanged(object sender, TextChangedEventArgs e)
		{
			if(this.IsInitialized && m_updateState == UpdateState.None)
			{
				m_dpu = ParseDouble(m_txtPixelResolution.Text);
				UpdateUnitMeasurements();
			}
		}

		private void OnPercent_KeyDown(object sender, KeyEventArgs e)
		{
			if(!Util.IsNumericKey(e.Key) && e.Key != Key.Tab && e.Key != Key.OemBackTab)
			{
				e.Handled = true;
			}
		}

		private void PreventSpace_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Space)
			{
				e.Handled = true;
			}
		}
	}
}
