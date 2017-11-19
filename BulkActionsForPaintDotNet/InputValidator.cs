using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace PDNBulkUpdater
{
	public static class InputValidator
	{
		public static readonly DependencyProperty HasErrorsProperty = DependencyProperty.RegisterAttached("HasErrors", typeof(bool), typeof(InputValidator), new PropertyMetadata(false));

		public static void SetHasErrors(DependencyObject obj, bool hasErrors)
		{
			obj.SetValue(HasErrorsProperty, hasErrors);
		}

		public static bool GetHasErrors(DependencyObject obj)
		{
			return (bool)obj.GetValue(HasErrorsProperty);
		}
	}
}
