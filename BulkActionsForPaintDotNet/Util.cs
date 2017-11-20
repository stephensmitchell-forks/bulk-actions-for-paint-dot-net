using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using PDNBulkUpdater.ReflectionWrapper;

namespace PDNBulkUpdater
{
	public static class Util
	{
		public const string DEFAULT_OUTPUT_DIR_DATE_FMT = "yyyy.M.d";
		public static readonly string DEFAULT_OUTPUT_DIR = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "BulkActionForPaintDotNet\\" + DateTime.Now.ToString(DEFAULT_OUTPUT_DIR_DATE_FMT));
		static readonly HashSet<char> m_invalidPathChars = new HashSet<char>(System.IO.Path.GetInvalidPathChars());

		public static bool IsNumericDecimalKey(System.Windows.Input.Key key)
		{
			return IsNumericKey(key) || IsDecimalKey(key);
		}

		public static bool IsDecimalKey(System.Windows.Input.Key key)
		{
			return key == System.Windows.Input.Key.Decimal || key == System.Windows.Input.Key.OemPeriod;
		}

		public static bool IsNumericKey(System.Windows.Input.Key key)
		{
			switch(key)
			{
				case System.Windows.Input.Key.NumPad0:
				case System.Windows.Input.Key.NumPad1:
				case System.Windows.Input.Key.NumPad2:
				case System.Windows.Input.Key.NumPad3:
				case System.Windows.Input.Key.NumPad4:
				case System.Windows.Input.Key.NumPad5:
				case System.Windows.Input.Key.NumPad6:
				case System.Windows.Input.Key.NumPad7:
				case System.Windows.Input.Key.NumPad8:
				case System.Windows.Input.Key.NumPad9:
				case System.Windows.Input.Key.D0:
				case System.Windows.Input.Key.D1:
				case System.Windows.Input.Key.D2:
				case System.Windows.Input.Key.D3:
				case System.Windows.Input.Key.D4:
				case System.Windows.Input.Key.D5:
				case System.Windows.Input.Key.D6:
				case System.Windows.Input.Key.D7:
				case System.Windows.Input.Key.D8:
				case System.Windows.Input.Key.D9:
					{
						return true;
					}
			}

			return false;
		}

		public static FileTypeCollection GetFileTypes()
		{
			string curDir = System.IO.Directory.GetCurrentDirectory();
			string pdnDir = curDir;

			try
			{
				using(Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Paint.NET", false))
				{
					pdnDir = key.GetValue("TARGETDIR", curDir, Microsoft.Win32.RegistryValueOptions.None) as string;
				}

				if(pdnDir == null)
				{
					pdnDir = curDir;
				}
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.ToString());
			}

			System.IO.Directory.SetCurrentDirectory(pdnDir);

			FileTypeCollection collection = null;

			try
			{                                
                Type ftypes = GetAssemblyPaintDotNet().GetType("PaintDotNet.Data.FileTypes", true, true);
				MethodInfo info = ftypes.GetMethod("GetFileTypes");

				collection = new FileTypeCollection(new PDNFileTypeCollection(info.Invoke(null, new object[0])));
			}
			finally
			{
				System.IO.Directory.SetCurrentDirectory(curDir);
			}

			return collection;
		}

        public static Assembly GetAssemblyPaintDotNet()
        {
            Type mainAssemblyType = typeof(PaintDotNet.Rendering.Matrix3x2DoubleExtensions);

            return mainAssemblyType.Assembly;
        }

        public static Assembly GetAssemblyPaintDotNetCore()
        {
            Type coreAssemblyType = typeof(PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra>);

            return coreAssemblyType.Assembly;
        }

		public static PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> CreateResizeBicubic(PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> source, int width, int height)
		{
			return (PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra>)GetAssemblyPaintDotNetCore().CreateInstance(
				"PaintDotNet.Rendering.ResizeBicubicRendererBgra",
				false,
				BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
				null, new object[] { source, width, height },
				null,
				new object[0]);
		}

		public static PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> CreateResizeBilinear(PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> source, int width, int height)
		{
			return (PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra>)GetAssemblyPaintDotNetCore().CreateInstance(
				"PaintDotNet.Rendering.ResizeBilinearRendererBgra",
				false,
				BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
				null, new object[] { source, width, height },
				null,
				new object[0]);
		}

		public static PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> CreateResizeNearestNeighbor(PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> source, int width, int height)
		{
			return (PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra>)GetAssemblyPaintDotNetCore().CreateInstance(
				"PaintDotNet.Rendering.ResizeNearestNeighborRendererBgra",
				false,
				BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
				null, new object[] { source, width, height },
				null,
				new object[0]);
		}

		public static PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> CreateResizeSuperSampling(PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> source, int width, int height)
		{
			return (PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra>)GetAssemblyPaintDotNetCore().CreateInstance(
				"PaintDotNet.Rendering.ResizeSuperSamplingRendererBgra",
				false,
				BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
				null, new object[] { source, width, height },
				null,
				new object[0]);
		}

		public static PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> CreateParallelRenderer(PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> renderer, PaintDotNet.Rendering.TilingStrategy tilingStrategy, int tileEdge, PaintDotNet.Threading.WorkItemQueuePriority workItemQueuePriority)
		{
            Type genericType = GetAssemblyPaintDotNetCore().GetType("PaintDotNet.Rendering.ParallelizeRenderer`1");
            Type concreteType = genericType.MakeGenericType(typeof(PaintDotNet.ColorBgra));

            PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> outRenderer = (PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra>)
                GetAssemblyPaintDotNetCore().CreateInstance(
                    concreteType.FullName,
                    false,
				    BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
				    null, new object[] { renderer, tilingStrategy, tileEdge, workItemQueuePriority },
				    null,
				    new object[0]);

            if (outRenderer == null)
                throw new NullReferenceException("outRenderer");

            return outRenderer;
        }

		public static PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> CreateTileizeRenderer(PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> renderer, PaintDotNet.Rendering.SizeInt32 size)
		{
            Type genericType = GetAssemblyPaintDotNetCore().GetType("PaintDotNet.Rendering.TileizeRenderer`1");
            Type concreteType = genericType.MakeGenericType(typeof(PaintDotNet.ColorBgra));

            PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> outRenderer = (PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra>)
                GetAssemblyPaintDotNetCore().CreateInstance(
                    concreteType.FullName,
				    false,
				    BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
				    null, new object[] { renderer, size },
				    null,
				    new object[0]);

            if (outRenderer == null)
                throw new NullReferenceException("outRenderer");

            return outRenderer;
        }

        public static int PrintToPixel(double dpu, double print)
		{
			return (int)Math.Round(print * dpu, MidpointRounding.AwayFromZero);
		}

		public static double PixelToPrint(double dpu, int pixel)
		{
			return pixel / dpu;
		}

		public static void ProcessImages(UpdateContext ctx, Dictionary<string, FileTypeSaveTokenPair> saveTokens, IProcessingEventListener events)
		{
			StringBuilder newPath = new StringBuilder();
			StringBuilder args = new StringBuilder();
			string outDir = "";

			try
			{
				outDir = SanitizePath(ctx.OutputDirectory, 0, newPath, args); ;

				if(!System.IO.Directory.Exists(outDir))
				{
					System.IO.Directory.CreateDirectory(ctx.OutputDirectory);
					events.LogInfo("Created root output directory \'{0}\'", ctx.OutputDirectory);
				}
			}
			catch(Exception ex)
			{
				events.LogError("Could not create root output directory \'{0}\'!", ctx.OutputDirectory);
				events.LogException(ex);

				events.OnProcessImagesFinished();
				return;
			}

			int imageIndex = 1;

			try
			{
				HashSet<OutputFile> outFiles = new HashSet<OutputFile>();

				foreach(FileSystemItem item in ctx.Files)
				{
					item.GetOutputFiles(outFiles, outDir);
				}

				events.OnResetProgress(outFiles.Count);

				foreach(OutputFile file in outFiles)
				{
					try
					{
						file.Process(ctx, saveTokens, events, imageIndex - 1);
						++imageIndex;
						events.LogInfo("Processed \'{0}\' to \'{1}\'", file.SourcePath, file.DestinationPath);
					}
					catch(Exception ex)
					{
						events.LogError("Processing image \'{0}\' to \'{1}\' failed!", file.SourcePath, file.DestinationPath);
						events.LogException(ex);
					}

					events.OnUpdateProgress(imageIndex);
				}
			}
			catch(Exception ex)
			{
				events.LogError("A fatal error occurred that has stopped image processing!");
				events.LogException(ex);
			}

			events.OnProcessImagesFinished();
		}

		public static string SanitizePath(string path, int index)
		{
			StringBuilder newPath = new StringBuilder();
			StringBuilder args = new StringBuilder();

			return SanitizePath(path, index, newPath, args);
		}

		public static string SanitizePath(string path, int index, StringBuilder newPath, StringBuilder args)
		{
			newPath.Length = 0;
			args.Length = 0;
			bool isParsing = false;
			int colonCount = 0;
			DateTime now = DateTime.Now;

			foreach(char c in path)
			{
				if(c == '<')
				{
					if(isParsing)
					{
						throw new FormatException("Missing closing '>'");
					}

					isParsing = true;
					args.Length = 0;
				}
				else if(c == '>')
				{
					if(!isParsing)
					{
						throw new FormatException("Missing closing '>'");
					}

					isParsing = false;
					string arg = args.ToString();

					if(arg.Equals("i", StringComparison.OrdinalIgnoreCase))
					{
						newPath.Append(index);
					}
					else
					{
						newPath.Append(now.ToString(arg));
					}
				}
				else if(isParsing)
				{
					args.Append(c);
				}
				else if(c == ':')
				{
					++colonCount;
					newPath.Append(c);
				}
				else
				{
					if(m_invalidPathChars.Contains(c))
					{
						throw new FormatException(string.Format("\'{0}\' is an invalid path character", c));
					}

					newPath.Append(c);
				}
			}

			string finalPath = newPath.ToString();

			if(isParsing)
			{
				throw new FormatException("Missing closing '>'");
			}
			// NOTE: finalPath should only contain valid path characters by this point so if IsPathRooted() fails
			// then that's acceptable
			else if(colonCount > 1 || (colonCount == 1 && !System.IO.Path.IsPathRooted(finalPath)))
			{
				throw new FormatException("A path may not contain instances of a non-root ':'");
			}

			return finalPath;
		}
	}
}
