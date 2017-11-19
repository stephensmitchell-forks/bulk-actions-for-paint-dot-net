using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDNBulkUpdater
{
	public class UpdateContext
	{
		FileTypeCollection m_fileTypes = Util.GetFileTypes();
		string m_outDir = Util.DEFAULT_OUTPUT_DIR;
		string m_renameFiles = "";
		PaintDotNet.FileType m_outputFileType;
		HashSet<FileSystemItem> m_files = new HashSet<FileSystemItem>();

		public FileTypeCollection FileTypes
		{
			get { return m_fileTypes; }
		}

		public string OutputDirectory
		{
			get { return m_outDir; }
			set { m_outDir = value.Trim(); }
		}

		public string RenameFiles
		{
			get { return m_renameFiles; }
			set { m_renameFiles = value.Trim(); }
		}

		public PaintDotNet.FileType OutputFileType
		{
			get { return m_outputFileType; }
			set { m_outputFileType = value; }
		}

		public HashSet<FileSystemItem> Files
		{
			get { return m_files; }
		}

		public bool UsePercent
		{
			get;
			set;
		}

		public int Percent
		{
			get;
			set;
		}

		public int NewWidthInPixels
		{
			get;
			set;
		}

		public int NewHeightInPixels
		{
			get;
			set;
		}

		public double Dpu
		{
			get;
			set;
		}

		public PaintDotNet.MeasurementUnit DpuUnit
		{
			get;
			set;
		}

		public PaintDotNet.ResamplingAlgorithm ResamplingAlgorithm
		{
			get;
			set;
		}

		public UpdateContext()
		{
			this.Percent = 100;
			this.DpuUnit = PaintDotNet.MeasurementUnit.Inch;
			this.Dpu = PaintDotNet.Document.GetDefaultDpu(this.DpuUnit);
			this.UsePercent = true;
			this.ResamplingAlgorithm = PaintDotNet.ResamplingAlgorithm.Bicubic;
		}

		public LinkedList<FileTypeSaveTokenPair> GetFileTypesUsedByFiles()
		{
			Dictionary<string, PaintDotNet.FileType> fileTypes = new Dictionary<string, PaintDotNet.FileType>();

			foreach(FileSystemItem item in m_files)
			{
				foreach(PaintDotNet.FileType fileType in item.GetFileTypes())
				{
					if(!fileTypes.ContainsKey(fileType.Name))
					{
						fileTypes[fileType.Name] = fileType;
					}
				}
			}

			LinkedList<FileTypeSaveTokenPair> list = new LinkedList<FileTypeSaveTokenPair>();

			foreach(KeyValuePair<string, PaintDotNet.FileType> pair in fileTypes)
			{
				list.AddLast(new FileTypeSaveTokenPair(pair.Value));
			}

			return list;
		}
	}
}
