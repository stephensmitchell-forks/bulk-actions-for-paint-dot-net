using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PDNBulkUpdater
{
	public static class CommandLine
	{
		static void PrintCmdUsage(string cmd, string desc)
		{
			const int DESC_PAD = 25;
			PrintCmdUsage(cmd, desc, DESC_PAD);
		}

		static void PrintCmdUsage(string cmd, string desc, int offset)
		{
			string final = string.Format("  {0}", cmd);

			System.Diagnostics.Debug.Write(final);
			System.Diagnostics.Debug.WriteLine(CreateIndent(offset - final.Length) + desc);

			Console.Write(final);
			PrintCmdDesc(desc, offset);
		}

		static void PrintCmdDesc(string desc, int offset)
		{
			if(offset >= Console.BufferWidth)
			{
				offset = Math.Max(0, Console.BufferWidth - 5);
			}

			string[] words = desc.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			int maxChars = Console.BufferWidth - offset;
			StringBuilder bldr = new StringBuilder();

			for(int i = 0; i < words.Length; ++i)
			{
				string curWord = words[i];

				if(bldr.Length + curWord.Length >= maxChars)
				{
					// remove trailing space
					bldr.Length = Math.Max(0, bldr.Length - 1);

					Console.SetCursorPosition(offset, Console.CursorTop);
					Console.WriteLine(bldr.ToString());
					bldr.Length = 0;
				}

				bldr.Append(curWord);
				bldr.Append(' ');
			}

			if(bldr.Length > 0)
			{
				// remove trailing space
				bldr.Length = bldr.Length - 1;

				Console.SetCursorPosition(offset, Console.CursorTop);
				Console.WriteLine(bldr.ToString());
			}

			Console.CursorVisible = true;
		}

		static void PrintUsage()
		{
			const string USAGE_STR = "Usage: PDNBulkUpdaterCmd.exe <source> ";
			// TODO: Create a more elegant and less error prone method of adding commands
			const string CMDS_STR = "[/o] [/dpu] [/u] [/res] [/print] [/pcnt] [/alg] [/ft] [/v0] [/v1] [/v2] [/v3] [/v4] [/oft] [/ren] [/ftprops]";

			Console.Write(USAGE_STR);
			PrintCmdDesc(CMDS_STR, USAGE_STR.Length);
			Console.WriteLine("");

			System.Diagnostics.Debug.Write(USAGE_STR);
			System.Diagnostics.Debug.WriteLine(CMDS_STR);
			System.Diagnostics.Debug.WriteLine("");

			PrintCmdUsage("<source>", "A list of source files, directories, and/or filters");

			PrintCmdUsage("/o:<directory>", "The destination directory i.e. \"/o:C:\\Users\\John Smith\\Pictures\"");

			PrintCmdUsage("/dpu:<value>", "The dots per unit i.e. /dpu:96");

			PrintCmdUsage("/u:<in|cm>", "Sets the dpu measurement to inches (in) or centimeters (cm) i.e. /u:in");

			PrintCmdUsage("/res:<resolution>", "The output resolution i.e. /res:800x600");

			PrintCmdUsage("/print:<resolution>", "The output resolution in dpu units i.e. /print:8.25x6.3");

			PrintCmdUsage("/pcnt:<percent>", "Scale images by a percentage of their input size i.e. /pcnt:50");

			PrintCmdUsage("/alg:<algorithm>", "The resampling algorithm. Valid algorithms are bicubic, super, nearest, or bilinear i.e. /alg:bicubic");

			PrintCmdUsage("/ft:<ext>:<prop>:<val>", "Sets the value of a file type property i.e. /ft:jpg:quality:100");

			PrintCmdUsage("/v0", "Sets the verbosity level to output nothing");

			PrintCmdUsage("/v1", "Sets the verbosity level to output exceptions");

			PrintCmdUsage("/v2", "Sets the verbosity level to output exceptions and errors");

			PrintCmdUsage("/v3", "Sets the verbosity level to output exceptions, errors, and warnings");

			PrintCmdUsage("/v4", "Sets the verbosity level to output everything. This is the default");

			PrintCmdUsage("/oft:<extension>", "Sets the output file type i.e. /oft:jpg. This can be any file type supported by Paint .NET");

			PrintCmdUsage("/ren:<file name>", "Renames output files i.e. \"/ren:Renamed File <i>\"");

			PrintCmdUsage("/ftprops", "Prints a list pf all available file type properties");
		}

		/// <summary>
		/// This code is required to be in a function other than Main(). Otherwise the local variable declarations force
		/// PDNBulkUpdater.dll to be loaded and we cannot do the delay load check.
		/// </summary>
		/// <param name="args">Command line arguments.</param>
		/// <returns>0 if application execution succeeded.</returns>
		public static int Run(string[] args)
		{
			UpdateContext ctx = new UpdateContext();
			ProcessingEventListener events = new ProcessingEventListener();
			bool hasCmdLineErrors = false;
			double? printWidth = null;
			double? printHeight = null;
			Dictionary<string, FileTypeSaveTokenPair> saveTokens = new Dictionary<string, FileTypeSaveTokenPair>();
			Dictionary<string, Dictionary<string, PaintDotNet.PropertySystem.Property>> fileTypeProperties = new Dictionary<string, Dictionary<string, PaintDotNet.PropertySystem.Property>>();

			try
			{
				// this is required to support file type plugins from a source other than paint.net
				foreach(PaintDotNet.FileType fileType in ctx.FileTypes)
				{
					FileTypeSaveTokenPair pair = new FileTypeSaveTokenPair(fileType);
					saveTokens[fileType.Name] = pair;

					PaintDotNet.PropertyBasedFileType propType = fileType as PaintDotNet.PropertyBasedFileType;

					if(propType != null)
					{
						PaintDotNet.PropertyBasedSaveConfigToken propTok = (PaintDotNet.PropertyBasedSaveConfigToken)pair.SaveToken;
						Dictionary<string, PaintDotNet.PropertySystem.Property> props = new Dictionary<string, PaintDotNet.PropertySystem.Property>();

						foreach(PaintDotNet.PropertySystem.Property curProp in propTok.Properties)
						{
							props[curProp.Name.ToLowerInvariant()] = curProp;
						}

						foreach(string ext in fileType.Extensions)
						{
							fileTypeProperties[ext] = props;
						}
					}
				}

				for(int i = 0; i < args.Length; ++i)
				{
					string arg = args[i];

					string[] parts = arg.Split(':');

					// yup this switch statement is pretty hideous
					// I should prob turn the case statements into functions - or something
					// the primary issue is type initialization causing the CLR to attempt to
					// load PDNBulkUpdater.dll before it gets to my delay loader which will cause a crash
					// it's on the todo list
					switch(parts[0].ToLowerInvariant())
					{
						case "/o":
							{
								if(parts.Length < 2)
								{
									hasCmdLineErrors = true;
								}
								else
								{
									// NOTE: + 1 skips the : after /o
									ctx.OutputDirectory = arg.Substring(parts[0].Length + 1);
								}

								break;
							}
						case "/dpu":
							{
								if(parts.Length != 2)
								{
									hasCmdLineErrors = true;
								}
								else
								{
									double dpu;

									if(!double.TryParse(parts[1], out dpu) || dpu < 0.0)
									{
										hasCmdLineErrors = true;
									}
									else
									{
										ctx.Dpu = dpu;
										UpdateResolutionFromDpu(ctx, printWidth, printHeight);
									}
								}

								break;
							}
						case "/u":
							{
								if(parts.Length != 2)
								{
									hasCmdLineErrors = true;
								}
								else
								{
									switch(parts[1].ToLowerInvariant())
									{
										case "in":
											{
												if(ctx.DpuUnit != PaintDotNet.MeasurementUnit.Inch)
												{
													ctx.Dpu = PaintDotNet.Document.InchesToCentimeters(ctx.Dpu);
													ctx.DpuUnit = PaintDotNet.MeasurementUnit.Inch;
													UpdateResolutionFromDpu(ctx, printWidth, printHeight);
												}

												break;
											}
										case "cm":
											{
												if(ctx.DpuUnit != PaintDotNet.MeasurementUnit.Centimeter)
												{
													ctx.Dpu = PaintDotNet.Document.CentimetersToInches(ctx.Dpu);
													ctx.DpuUnit = PaintDotNet.MeasurementUnit.Centimeter;
													UpdateResolutionFromDpu(ctx, printWidth, printHeight);
												}

												break;
											}
										default:
											{
												hasCmdLineErrors = true;
												break;
											}
									}
								}
								break;
							}
						case "/res":
							{
								if(parts.Length != 2)
								{
									hasCmdLineErrors = true;
								}
								else
								{
									string[] res = parts[1].Split('x');
									int pixelWidth = 0;
									int pixelHeight = 0;

									if(res.Length != 2)
									{
										hasCmdLineErrors = true;
									}
									else if(!int.TryParse(res[0], out pixelWidth) || !int.TryParse(res[1], out pixelHeight) || pixelWidth < 0 || pixelHeight < 0)
									{
										hasCmdLineErrors = true;
									}
									else
									{
										ctx.UsePercent = false;
										ctx.NewWidthInPixels = pixelWidth;
										ctx.NewHeightInPixels = pixelHeight;
									}
								}

								break;
							}
						case "/print":
							{
								if(parts.Length != 2)
								{
									hasCmdLineErrors = true;
								}
								else
								{
									string[] res = parts[1].Split('x');
									double tempPrintWidth = 0.0;
									double tempPrintHeight = 0.0;

									if(res.Length != 2)
									{
										hasCmdLineErrors = true;
									}
									else if(!double.TryParse(res[0], out tempPrintWidth) || !double.TryParse(res[1], out tempPrintHeight) || printWidth < 0.0 || printHeight < 0.0)
									{
										hasCmdLineErrors = true;
									}
									else
									{
										ctx.UsePercent = false;
										ctx.NewWidthInPixels = Util.PrintToPixel(ctx.Dpu, tempPrintWidth);
										ctx.NewHeightInPixels = Util.PrintToPixel(ctx.Dpu, tempPrintHeight);
										printWidth = tempPrintWidth;
										printHeight = tempPrintHeight;
									}
								}

								break;
							}
						case "/pcnt":
							{
								if(parts.Length != 2)
								{
									hasCmdLineErrors = true;
								}
								else
								{
									int percent;

									if(!int.TryParse(parts[1], out percent) || percent < 0)
									{
										hasCmdLineErrors = true;
									}
									else
									{
										ctx.Percent = percent;
										ctx.UsePercent = true;
									}
								}

								break;
							}
						case "/alg":
							{
								if(parts.Length != 2)
								{
									hasCmdLineErrors = true;
								}
								else
								{
									switch(parts[1].ToLowerInvariant())
									{
										case "bicubic":
											{
												ctx.ResamplingAlgorithm = PaintDotNet.ResamplingAlgorithm.Bicubic;
												break;
											}
										case "nearest":
											{
												ctx.ResamplingAlgorithm = PaintDotNet.ResamplingAlgorithm.NearestNeighbor;
												break;
											}
										case "bilinear":
											{
												ctx.ResamplingAlgorithm = PaintDotNet.ResamplingAlgorithm.Bilinear;
												break;
											}
										case "super":
											{
												ctx.ResamplingAlgorithm = PaintDotNet.ResamplingAlgorithm.SuperSampling;
												break;
											}
										default:
											{
												hasCmdLineErrors = true;
												break;
											}
									}
								}
								break;
							}
						case "/ft":
							{
								if(parts.Length != 4)
								{
									hasCmdLineErrors = true;
								}
								else
								{
									string ext = parts[1].ToLowerInvariant();
									string propName = parts[2].ToLowerInvariant();
									string propValue = parts[3];
									Dictionary<string, PaintDotNet.PropertySystem.Property> props = null;
									PaintDotNet.PropertySystem.Property property = null;
									int intVal = 0;
									double dblVal = 0.0;
									bool boolVal = false;
									bool succeeded = false;

									if(!ext.StartsWith("."))
									{
										ext = "." + ext;
									}

									if(fileTypeProperties.TryGetValue(ext, out props) && props.TryGetValue(propName, out property))
									{
										PaintDotNet.PropertySystem.Int32Property intProp = property as PaintDotNet.PropertySystem.Int32Property;
										PaintDotNet.PropertySystem.DoubleProperty dblProp = property as PaintDotNet.PropertySystem.DoubleProperty;
										PaintDotNet.PropertySystem.BooleanProperty boolProp = property as PaintDotNet.PropertySystem.BooleanProperty;
										PaintDotNet.PropertySystem.StaticListChoiceProperty listProp = property as PaintDotNet.PropertySystem.StaticListChoiceProperty;

										if(intProp != null)
										{
											succeeded = int.TryParse(propValue, out intVal);

											if(succeeded)
											{
												intProp.Value = intVal;
											}
										}
										else if(dblProp != null)
										{
											succeeded = double.TryParse(propValue, out dblVal);

											if(succeeded)
											{
												dblProp.Value = dblVal;
											}
										}
										else if(boolProp != null)
										{
											succeeded = bool.TryParse(propValue, out boolVal);

											if(succeeded)
											{
												boolProp.Value = boolVal;
											}
										}
										else if(listProp != null)
										{
											if(listProp.ValueChoices.Length == 0)
											{
												succeeded = true;
											}
											else
											{
												Type valType = listProp.ValueChoices[0].GetType();

												if(valType.IsEnum && valType.IsValueType)
												{
													try
													{
														listProp.Value = Enum.Parse(valType.DeclaringType, propValue, true);
														succeeded = true;
													}
													catch(Exception ex)
													{
														System.Diagnostics.Debug.WriteLine(ex.ToString());
														events.LogError("\'{0}\' is an invalid value for property \'{1}\' in file type \'{2}\'", propValue, propName, ext);
													}
												}
												else
												{
													foreach(object obj in listProp.ValueChoices)
													{
														if(obj.Equals(propValue))
														{
															listProp.Value = obj;
															succeeded = true;
															break;
														}
													}
												}
											}
										}
										else
										{
											events.LogWarning("Unknown property type \'{0}\' discarded!", property.GetType().Name);
											succeeded = true;
										}
									}

									if(!succeeded)
									{
										hasCmdLineErrors = true;
									}
								}

								break;
							}
						case "/v0":
							{
								events.Verbosity = Verbosity.None;
								break;
							}
						case "/v1":
							{
								events.Verbosity = Verbosity.Exceptions;
								break;
							}
						case "/v2":
							{
								events.Verbosity = Verbosity.Exceptions | Verbosity.Errors;
								break;
							}
						case "/v3":
							{
								events.Verbosity = Verbosity.Exceptions | Verbosity.Errors | Verbosity.Warnings;
								break;
							}
						case "/v4":
							{
								events.Verbosity = Verbosity.Everything;
								break;
							}
						case "/oft":
							{
								if(parts.Length != 2)
								{
									hasCmdLineErrors = true;
								}
								else
								{
									PaintDotNet.FileType ft = ctx.FileTypes[parts[1].ToLowerInvariant()];

									if(ft == null)
									{
										events.LogError("File type \'{0}\' is not supported!", parts[1]);
										hasCmdLineErrors = true;
									}
									else
									{
										ctx.OutputFileType = ft;
									}
								}

								break;
							}
						case "/ren":
							{
								if(parts.Length != 2)
								{
									hasCmdLineErrors = true;
								}
								else
								{
									ctx.RenameFiles = parts[1];
								}

								break;
							}
						case "/ftprops":
							{
								const int PROP_OFFSET = 35;

								Console.WriteLine("File Type Properties:");
								System.Diagnostics.Debug.WriteLine("File Type Properties:");

								foreach(PaintDotNet.FileType fileType in ctx.FileTypes)
								{
									PaintDotNet.PropertyBasedSaveConfigToken token = fileType.CreateDefaultSaveConfigToken() as PaintDotNet.PropertyBasedSaveConfigToken;

									if(token == null)
									{
										PrintCmdUsage(fileType.DefaultExtension, "0 properties", PROP_OFFSET);
									}
									else
									{
										PrintCmdUsage(fileType.DefaultExtension, token.Properties.Count.ToString() + " properties", PROP_OFFSET);

										foreach(PaintDotNet.PropertySystem.Property prop in token.Properties)
										{
											PrintCmdUsage("  " + prop.Name, prop.GetType().Name, PROP_OFFSET);
										}
									}

									System.Diagnostics.Debug.WriteLine("");
									Console.WriteLine("");
								}

								break;
							}
						default:
							{
								try
								{
									string fileName = System.IO.Path.GetFileName(arg);

									if(fileName.Length == 0 || !fileName.Contains('.'))
									{
										string dirName = System.IO.Path.GetFullPath(arg);

										if(!System.IO.Directory.Exists(dirName))
										{
											events.LogError("Directory \'{0}' does not exist!", dirName);
											hasCmdLineErrors = true;
										}
										else
										{
											ctx.Files.Add(new FileSystemDirectory(dirName, ctx.FileTypes));
										}
									}
									else if(fileName.Contains('*'))
									{
										string ext = System.IO.Path.GetExtension(fileName);

										if(ext == ".*")
										{
											string dirName = System.IO.Path.GetDirectoryName(arg);

											if(!System.IO.Directory.Exists(dirName))
											{
												events.LogError("Directory \'{0}' does not exist!", dirName);
												hasCmdLineErrors = true;
											}
											else
											{
												ctx.Files.Add(new FileSystemDirectory(dirName, ctx.FileTypes));
											}
										}
										else
										{
											PaintDotNet.FileType fileType = ctx.FileTypes[ext];

											if(fileType == null)
											{
												hasCmdLineErrors = true;
												events.LogError("Extension \'{0}\' is not supported!", ext);
											}
											else
											{
												string dirName = System.IO.Path.GetDirectoryName(arg);

												if(!System.IO.Directory.Exists(dirName))
												{
													events.LogError("Directory \'{0}' does not exist!", dirName);
													hasCmdLineErrors = true;
												}
												else
												{
													ctx.Files.Add(new FileSystemDirectory(dirName, new FileTypeCollection(new PaintDotNet.FileType[] { fileType })));
												}
											}
										}
									}
									else
									{
										if(!System.IO.File.Exists(arg))
										{
											events.LogError("File \'{0}' does not exist!", arg);
											hasCmdLineErrors = true;
										}
										else
										{
											ctx.Files.Add(new FileSystemFile(System.IO.Path.GetFullPath(arg), ctx.FileTypes));
										}
									}
								}
								catch(Exception ex)
								{
									events.LogError("Invalid input directory, filter, or file: " + arg);
									events.LogException(ex);
								}

								break;
							}
					}
				}

				if(ctx.Files.Count == 0 || hasCmdLineErrors)
				{
					PrintUsage();
				}
				else
				{
					if((events.Verbosity & Verbosity.Info) == Verbosity.Info)
					{
						events.LogInfo("Output Settings");

						if(ctx.UsePercent)
						{
							PrintCmdUsage("Percent:", ctx.Percent.ToString());
						}
						else
						{
							PrintCmdUsage("Pixel Resolution:", string.Format("{0}x{1}", ctx.NewWidthInPixels.ToString(), ctx.NewHeightInPixels.ToString()));
							PrintCmdUsage("Print Resolution:", string.Format("{0}x{1}", Util.PixelToPrint(ctx.Dpu, ctx.NewWidthInPixels).ToString("0.##"), Util.PixelToPrint(ctx.Dpu, ctx.NewHeightInPixels).ToString("0.##")));
						}

						PrintCmdUsage("Dpu:", ctx.Dpu.ToString("0.##"));
						PrintCmdUsage("Dpu Unit:", ctx.DpuUnit.ToString());
						PrintCmdUsage("Algorithm:", ctx.ResamplingAlgorithm.ToString());
						PrintCmdUsage("Rename To:", ctx.RenameFiles);
						PrintCmdUsage("Output File Type:", ctx.OutputFileType == null ? "" : ctx.OutputFileType.Name);
						PrintCmdUsage("Output Directory:", ctx.OutputDirectory);
						Console.WriteLine("");
					}

					Util.ProcessImages(ctx, saveTokens, events);
				}
			}
			catch(Exception ex)
			{
				events.LogException(ex);
				hasCmdLineErrors = true;
			}

			return hasCmdLineErrors ? 1 : 0;
		}

		private static void UpdateResolutionFromDpu(UpdateContext ctx, double? printWidth, double? printHeight)
		{
			if(printWidth.HasValue)
			{
				ctx.NewWidthInPixels = Util.PrintToPixel(ctx.Dpu, printWidth.Value);
			}

			if(printHeight.HasValue)
			{
				ctx.NewHeightInPixels = Util.PrintToPixel(ctx.Dpu, printHeight.Value);
			}
		}

		static string CreateIndent(int indent)
		{
			if(indent < 0)
			{
				throw new ArgumentOutOfRangeException("indent");
			}

			StringBuilder bldr = new StringBuilder(indent + 1);

			for(int i = 0; i < indent; ++i)
			{
				bldr.Append(' ');
			}

			return bldr.ToString();
		}
	}
}
