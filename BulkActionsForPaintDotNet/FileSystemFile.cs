using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDNBulkUpdater
{
	public class FileSystemFile : FileSystemItem
	{
		FileTypeCollection m_fileTypes;

		public FileSystemFile(string path, FileTypeCollection fileTypes)
			: base(path)
		{
			if(fileTypes == null)
			{
				throw new ArgumentNullException("fileTypes");
			}

			m_fileTypes = fileTypes;
		}

		public FileSystemFile(string path)
			: this(path, Util.GetFileTypes())
		{
		}

		public override void GetOutputFiles(HashSet<OutputFile> files, string outputDir)
		{
			files.Add(new OutputFile(this.Path, System.IO.Path.Combine(outputDir, System.IO.Path.GetFileName(this.Path))));
		}

		public override IEnumerable<PaintDotNet.FileType> GetFileTypes()
		{
			yield return m_fileTypes[System.IO.Path.GetExtension(this.Path)];
		}
	}
}
