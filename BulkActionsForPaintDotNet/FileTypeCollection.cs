using PDNBulkUpdater.ReflectionWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDNBulkUpdater
{
	public class FileTypeCollection : IEnumerable<PaintDotNet.FileType>
	{
        PDNFileTypeCollection m_collection;

		public FileTypeCollection(PDNFileTypeCollection collection)
		{
			if(collection == null)
			{
				throw new ArgumentNullException("collection");
			}

			m_collection = collection;
		}

		public FileTypeCollection(IEnumerable<PaintDotNet.FileType> fileTypes)
			: this(new PDNFileTypeCollection(fileTypes))
		{
		}

		public string[] AllExtensions { get { return m_collection.AllExtensions; } }
		public PaintDotNet.FileType[] FileTypes { get { return m_collection.FileTypes; } }
		public int Length { get { return m_collection.Length; } }

		public PaintDotNet.FileType this[int index] { get { return m_collection[index]; } }
		public PaintDotNet.FileType this[string ext]
		{
			get
			{
				if(!ext.StartsWith("."))
				{
					ext = "." + ext;
				}

				int index = m_collection.IndexOfExtension(ext);

				return index > -1 ? m_collection[index] : null;
			}
		}

		public static PaintDotNet.FileType[] FilterFileTypeList(PaintDotNet.FileType[] input, bool excludeCantSave, bool excludeCantLoad)
		{
			return PDNFileTypeCollection.FilterFileTypeList(input, excludeCantSave, excludeCantLoad);
		}

		public int IndexOfExtension(string findMeExt)
		{
			return m_collection.IndexOfExtension(findMeExt);
		}

		public int IndexOfFileType(PaintDotNet.FileType fileType)
		{
			return m_collection.IndexOfFileType(fileType);
		}

		public int IndexOfName(string name)
		{
			return m_collection.IndexOfName(name);
		}

		public string ToString(bool excludeCantSave, bool excludeCantLoad)
		{
			return m_collection.ToString(excludeCantSave, excludeCantLoad);
		}

		public string ToString(bool includeAll, string allName, bool excludeCantSave, bool excludeCantLoad)
		{
			return m_collection.ToString(includeAll, allName, excludeCantSave, excludeCantLoad);
		}

		#region IEnumerable<FileType> Members

		public IEnumerator<PaintDotNet.FileType> GetEnumerator()
		{
			for(int i = 0; i < m_collection.Length; ++i)
			{
				yield return m_collection[i];
			}
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}
