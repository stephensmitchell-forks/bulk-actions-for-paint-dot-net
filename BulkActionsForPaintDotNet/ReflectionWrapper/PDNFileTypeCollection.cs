using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PDNBulkUpdater.ReflectionWrapper
{
    public class PDNFileTypeCollection
    {
        // PaintDotNet.FileTypeCollection
        private object mFileTypeCollection;
        PropertyInfo mFileTypeCollectionIndexProperty;

        public PDNFileTypeCollection(object fileTypeCollection)
        {
            if (fileTypeCollection == null)
                throw new ArgumentNullException("fileTypeCollection");

            mFileTypeCollection = fileTypeCollection;

            PostCtorInit();
        }

        public PDNFileTypeCollection(IEnumerable<PaintDotNet.FileType> fileTypes)
        {
            if (fileTypes == null)
                throw new ArgumentNullException("fileTypes");

            Type FileTypeCollectionType = System.Type.GetType("PaintDotNet.FileTypeCollection");
            ConstructorInfo FileTypeCollectionTypeCtor = FileTypeCollectionType.GetConstructor(new Type[] { fileTypes.GetType() });

            mFileTypeCollection = FileTypeCollectionTypeCtor.Invoke(new object[] { fileTypes });

            PostCtorInit();
        }

        private void PostCtorInit()
        {
            PropertyInfo[] properties = mFileTypeCollection.GetType().GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].GetIndexParameters().Length == 1)
                {
                    ParameterInfo parameterInfo = properties[i].GetIndexParameters()[0];
                    if (parameterInfo.ParameterType.Equals(typeof(int)))
                    {
                        mFileTypeCollectionIndexProperty = properties[i];
                    }
                }
            }

            if (mFileTypeCollectionIndexProperty == null)
                throw new Exception("Could not find index property");
        }

        public static PaintDotNet.FileType[] FilterFileTypeList(PaintDotNet.FileType[] input, bool excludeCantSave, bool excludeCantLoad)
        {
            Type fileTypeCollectionType = Type.GetType("PaintDotNet.FileTypeCollection");

            return (PaintDotNet.FileType[])fileTypeCollectionType.GetMethod("FilterFileTypeList").Invoke(null, new object[] { input, excludeCantSave, excludeCantLoad });
        }

        public PaintDotNet.FileType this[int index]
        {
            get
            {
                return (PaintDotNet.FileType)mFileTypeCollectionIndexProperty.GetValue(mFileTypeCollection, new object[] { index });
            }
        }

        public string[] AllExtensions
        {
            get
            {
                return (string[])mFileTypeCollection.GetType().GetProperty("AllExtensions").GetValue(mFileTypeCollection);
            }
        }

        public PaintDotNet.FileType[] FileTypes
        {
            get
            {
                return (PaintDotNet.FileType[])mFileTypeCollection.GetType().GetProperty("FileTypes").GetValue(mFileTypeCollection);
            }
        }

        public int Length
        {
            get
            {
                return (int)mFileTypeCollection.GetType().GetProperty("Length").GetValue(mFileTypeCollection);
            }
        }

        public int IndexOfExtension(string findMeExt)
        {
            return (int)mFileTypeCollection.GetType().GetMethod("IndexOfExtension").Invoke(mFileTypeCollection, new object[] { findMeExt });
        }

        public int IndexOfFileType(PaintDotNet.FileType fileType)
        {
            return (int)mFileTypeCollection.GetType().GetMethod("IndexOfFileType").Invoke(mFileTypeCollection, new object[] { fileType });
        }

        public int IndexOfName(string name)
        {
            return (int)mFileTypeCollection.GetType().GetMethod("IndexOfName").Invoke(mFileTypeCollection, new object[] { name });
        }

        public string ToString(bool excludeCantSave, bool excludeCantLoad)
        {
            return (string)mFileTypeCollection.GetType().GetMethod("ToString").Invoke(mFileTypeCollection, new object[] { excludeCantSave, excludeCantLoad });
        }

        public string ToString(bool includeAll, string allName, bool excludeCantSave, bool excludeCantLoad)
        {
            return (string)mFileTypeCollection.GetType().GetMethod("ToString").Invoke(mFileTypeCollection, new object[] { includeAll, allName, excludeCantSave, excludeCantLoad });
        }
    }
}
