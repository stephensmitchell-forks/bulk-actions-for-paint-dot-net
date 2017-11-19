using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDNBulkUpdater
{
	public abstract class FileSystemItem
	{
		public string Path
		{
			get;
			private set;
		}

		public abstract void GetOutputFiles(HashSet<OutputFile> files, string outputDir);
		public abstract IEnumerable<PaintDotNet.FileType> GetFileTypes();

		public FileSystemItem(string path)
		{
			if(path == null)
			{
				throw new ArgumentNullException("path");
			}

			this.Path = System.IO.Path.GetFullPath(path);
		}

		public override int GetHashCode()
		{
			return Path.GetHashCode();
		}

		public override string ToString()
		{
			return Path;
		}

		public override bool Equals(object obj)
		{
			FileSystemItem item = obj as FileSystemItem;

			if(item == null)
			{
				return false;
			}

			return item == this || string.Equals(Path, item.Path, StringComparison.OrdinalIgnoreCase);
		}
	}
}
