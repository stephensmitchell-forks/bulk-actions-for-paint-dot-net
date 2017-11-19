using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDNBulkUpdater
{
	public class FileTypeSaveTokenPair
	{
		public PaintDotNet.FileType FileType
		{
			get;
			private set;
		}

		public PaintDotNet.SaveConfigToken SaveToken
		{
			get;
			private set;
		}

		public FileTypeSaveTokenPair(PaintDotNet.FileType fileType)
		{
			if(fileType == null)
			{
				throw new ArgumentNullException("fileType");
			}

			this.FileType = fileType;
			this.SaveToken = fileType.GetLastSaveConfigToken();
		}

		public System.Windows.Forms.Control CreateWidget()
		{
			PaintDotNet.SaveConfigWidget widget = this.FileType.CreateSaveConfigWidget();
			widget.Token = this.SaveToken;

			return widget;
		}
	}
}
