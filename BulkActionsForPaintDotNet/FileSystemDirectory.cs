using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDNBulkUpdater
{
	public class FileSystemDirectory : FileSystemItem
	{
		FileTypeCollection m_fileTypes;
		List<string> m_outFiles = new List<string>();
		System.Threading.Thread m_thread;
		System.Threading.ManualResetEvent m_waitHandle = new System.Threading.ManualResetEvent(false);
		Dictionary<string, PaintDotNet.FileType> m_usedTypes = new Dictionary<string, PaintDotNet.FileType>();

		public FileSystemDirectory(string path, FileTypeCollection fileTypes)
			: base(path)
		{
			if(fileTypes == null)
			{
				throw new ArgumentNullException("fileTypes");
			}

			m_fileTypes = fileTypes;

			m_thread = new System.Threading.Thread(FindOutputFiles);
			m_thread.Name = "FileSystemDirectory.FindOutputFiles";
			m_thread.IsBackground = true;
			m_thread.Start();
		}

		public FileSystemDirectory(string path)
			: this(path, Util.GetFileTypes())
		{
		}

		void FindOutputFiles()
		{
			try
			{
				// TODO: Upgrade to .net 4 and use the parallel lib for this
				foreach(string ext in m_fileTypes.AllExtensions)
				{
					string[] pathList = System.IO.Directory.GetFiles(this.Path, "*" + ext, System.IO.SearchOption.AllDirectories);

					if(pathList.Length > 0)
					{
						m_usedTypes[ext] = m_fileTypes[ext];
					}

					foreach(string path in pathList)
					{
						m_outFiles.Add(path);
					}
				}
			}
			catch(System.Threading.ThreadAbortException)
			{
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.ToString());
			}
			finally
			{
				m_waitHandle.Set();
			}
		}

		public override void GetOutputFiles(HashSet<OutputFile> files, string outputDir)
		{
			m_waitHandle.WaitOne();

			string dirPath = this.Path.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar);

			foreach(string path in m_outFiles)
			{
				files.Add(new OutputFile(path, System.IO.Path.Combine(outputDir, path.Substring(dirPath.Length + 1))));
			}
		}

		public override IEnumerable<PaintDotNet.FileType> GetFileTypes()
		{
			m_waitHandle.WaitOne();

			return m_usedTypes.Values;
		}
	}
}
