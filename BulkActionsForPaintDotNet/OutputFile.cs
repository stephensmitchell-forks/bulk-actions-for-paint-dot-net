using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDNBulkUpdater
{
	public class OutputFile
	{
		public string SourcePath
		{
			get;
			private set;
		}

		public string DestinationPath
		{
			get;
			private set;
		}

		public OutputFile(string srcPath, string destPath)
		{
			SourcePath = srcPath;
			DestinationPath = destPath;
		}

		public void Process(UpdateContext ctx, Dictionary<string, FileTypeSaveTokenPair> saveTokens, IProcessingEventListener events, int index)
		{
			PaintDotNet.FileType srcFileType = ctx.FileTypes[System.IO.Path.GetExtension(this.SourcePath)];
			PaintDotNet.FileType destFileType = ctx.OutputFileType ?? ctx.FileTypes[System.IO.Path.GetExtension(this.DestinationPath)];
			PaintDotNet.Document doc = null;
			PaintDotNet.Document newDoc = null;
			PaintDotNet.SaveConfigToken token = null;

			if(ctx.RenameFiles.Length > 0)
			{
				this.DestinationPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.DestinationPath), Util.SanitizePath(ctx.RenameFiles, index));
			}

			if(System.IO.Path.GetExtension(this.DestinationPath).Length == 0)
			{
				this.DestinationPath += destFileType.DefaultExtension;
			}
			else if(ctx.OutputFileType != null)
			{
				this.DestinationPath = System.IO.Path.ChangeExtension(this.DestinationPath, ctx.OutputFileType.DefaultExtension);
			}

			string dir = System.IO.Path.GetDirectoryName(this.DestinationPath);

			if(!System.IO.Directory.Exists(dir))
			{
				System.IO.Directory.CreateDirectory(dir);
				events.LogInfo("Created directory \'{0}\'", dir);
			}
			
			FileTypeSaveTokenPair tokenPair;
			if(!saveTokens.TryGetValue(destFileType.Name, out tokenPair))
			{
				token = destFileType.GetLastSaveConfigToken();
			}
			else
			{
				token = tokenPair.SaveToken;
			}

			using(System.IO.FileStream file = System.IO.File.OpenRead(this.SourcePath))
			{
				newDoc = doc = srcFileType.Load(file);
			}

            // Image resize
			int newWidth = ctx.NewWidthInPixels;
			int newHeight = ctx.NewHeightInPixels;

			if(ctx.UsePercent)
			{
				if(ctx.Percent != 100)
				{
					double percent = ctx.Percent / 100.0;
					newWidth = (int)(doc.Width * percent);
					newHeight = (int)(doc.Height * percent);
				}
				else
				{
					newWidth = doc.Width;
					newHeight = doc.Height;
				}
			}

			if(doc.Width != newWidth || doc.Height != newHeight)
			{
				newDoc = new PaintDotNet.Document(newWidth, newHeight);
				newDoc.ReplaceMetadataFrom(doc);
				newDoc.DpuUnit = ctx.DpuUnit;
				newDoc.DpuX = ctx.Dpu;
				newDoc.DpuY = ctx.Dpu;

				for(int i = 0; i < doc.Layers.Count; ++i)
				{
					PaintDotNet.BitmapLayer oldLayer = (PaintDotNet.BitmapLayer)doc.Layers[i];
					PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> renderer = null;

					switch(ctx.ResamplingAlgorithm)
					{
						case PaintDotNet.ResamplingAlgorithm.Bicubic:
							{
								renderer = Util.CreateResizeBicubic(oldLayer.Surface, newWidth, newHeight);
								break;
							}
						case PaintDotNet.ResamplingAlgorithm.Bilinear:
							{
								renderer = Util.CreateResizeBilinear(oldLayer.Surface, newWidth, newHeight);
								break;
							}
						case PaintDotNet.ResamplingAlgorithm.NearestNeighbor:
							{
								renderer = Util.CreateResizeNearestNeighbor(oldLayer.Surface, newWidth, newHeight);
								break;
							}
						case PaintDotNet.ResamplingAlgorithm.SuperSampling:
							{
								if(newWidth >= oldLayer.Width || newHeight >= oldLayer.Height)
								{
									renderer = Util.CreateResizeBicubic(oldLayer.Surface, newWidth, newHeight);
								}
								else
								{
									renderer = Util.CreateResizeSuperSampling(oldLayer.Surface, newWidth, newHeight);
								}

								break;
							}
					}

					PaintDotNet.Rendering.ObservableRenderer<PaintDotNet.ColorBgra> observableRenderer = new PaintDotNet.Rendering.ObservableRenderer<PaintDotNet.ColorBgra>(renderer);
					PaintDotNet.Rendering.IRenderer<PaintDotNet.ColorBgra> parallelRenderer = Environment.ProcessorCount > 1 ? 
                        Util.CreateParallelRenderer(observableRenderer, PaintDotNet.Rendering.TilingStrategy.Tiles, 7, PaintDotNet.Threading.WorkItemQueuePriority.High) : 
                        Util.CreateTileizeRenderer(renderer, new PaintDotNet.Rendering.SizeInt32(1 << 7, 1 << 7));

					PaintDotNet.Surface newSurface = new PaintDotNet.Surface(newWidth, newHeight);
					parallelRenderer.Render(newSurface, new PaintDotNet.Rendering.PointInt32(0, 0));

					PaintDotNet.BitmapLayer newLayer = new PaintDotNet.BitmapLayer(newSurface, true);
					newLayer.LoadProperties(oldLayer.SaveProperties());

					newDoc.Layers.Add(newLayer);
				}
			}

			using(System.IO.FileStream file = System.IO.File.Create(this.DestinationPath))
			{
				using(PaintDotNet.Surface scratchSurface = new PaintDotNet.Surface(newWidth, newHeight))
				{
					destFileType.Save(newDoc, file, token, scratchSurface, null, false);
				}
			}

			doc.Dispose();

			if(!newDoc.IsDisposed)
			{
				newDoc.Dispose();
			}
		}
	}
}
