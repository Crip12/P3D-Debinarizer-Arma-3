using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using BisDll.Model;
using BisDll.Model.MLOD;
using BisDll.Model.ODOL;

namespace P3DDebinarizer
{
	// Token: 0x02000002 RID: 2
	internal class Program
	{
		// Token: 0x06000002 RID: 2 RVA: 0x00002080 File Offset: 0x00000280
		[STAThread]
		private static int Main(string[] args)
		{
			Trace.Listeners.Add(new ConsoleTraceListener());
			string message = "======================\r\n P3DDebinarizer by T_D\r\n=======================";
			string message2 = "P3DDebinarizer converts binarized p3d models (ODOL format) to editable MLOD format.\r\n\r\nUsage: \r\nP3DDebinarizer.exe - opens a dialog to select a p3d file\r\nP3DDebinarizer.exe path/model.p3d - converts the given p3d and saves it as path/model_mlod.p3d\r\nP3DDebinarizer.exe inputFolder [outputFolder] - converts all p3d in inputFolder (sub folders are excluded) and saves them in outputFolder";
			Trace.WriteLine(message);
			Trace.WriteLine(message2);
			try
			{
				int num = args.Length;
				switch (num)
				{
				case 0:
				{
					OpenFileDialog openFileDialog = new OpenFileDialog();
					openFileDialog.Filter = "ArmA P3d (ODOL)|*.p3d";
					if (openFileDialog.ShowDialog() == DialogResult.OK)
					{
						Program.convertP3dFile(openFileDialog.FileName, null, false);
					}
					break;
				}
				case 1:
				{
					string text = args[0];
					if (File.Exists(text))
					{
						if (Path.GetExtension(text) == ".p3d")
						{
							Program.convertP3dFile(Path.GetFullPath(text), null, false);
						}
						else
						{
							Trace.WriteLine("The file '{0}' does not have the p3d file extension.", text);
						}
					}
					else if (Directory.Exists(text))
					{
						Program.convertP3dFiles(Directory.EnumerateFiles(text, "*.p3d", SearchOption.TopDirectoryOnly).ToArray<string>(), null, false);
					}
					else
					{
						Trace.WriteLine("The file or directory '{0}' was not found.", text);
					}
					break;
				}
				case 2:
				{
					string text2 = args[0];
					string text3 = args[1];
					if (Directory.Exists(text2))
					{
						if (Directory.Exists(text3))
						{
							Program.convertP3dFiles(Directory.EnumerateFiles(text2, "*.p3d", SearchOption.TopDirectoryOnly).ToArray<string>(), text3, false);
						}
						else
						{
							Trace.WriteLine(string.Format("The folder '{0}' does not exist.", text3));
						}
					}
					else
					{
						Trace.WriteLine(string.Format("The folder '{0}' does not exist.", text2));
					}
					break;
				}
				default:
					Trace.WriteLine(message2);
					break;
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLine(ex.Message);
				Trace.Write(ex.StackTrace);
				return 1;
			}
			return 0;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x0000220C File Offset: 0x0000040C
		private static void convertP3dFiles(IEnumerable<string> srcFiles, string dstFolder, bool allowOverwriting = false)
		{
			Trace.WriteLine(string.Format("Start conversion of {0} p3d files:", srcFiles.Count<string>()));
			Trace.Indent();
			int num = 0;
			foreach (string text in srcFiles)
			{
				string fileName = Path.GetFileName(text);
				string dstPath = (dstFolder == null) ? null : Path.Combine(dstFolder, fileName);
				if (!Program.convertP3dFile(text, dstPath, allowOverwriting))
				{
					num++;
				}
			}
			Trace.Unindent();
			if (num == 0)
			{
				Trace.WriteLine("Conversions finished successfully.");
				return;
			}
			Trace.WriteLine(string.Format("{0} was/were not successful.", num));
		}

		// Token: 0x06000004 RID: 4 RVA: 0x000022B8 File Offset: 0x000004B8
		private static bool convertP3dFile(string srcPath, string dstPath = null, bool allowOverwriting = false)
		{
			if (!allowOverwriting && srcPath == dstPath)
			{
				Trace.WriteLine("Overwriting the source file is disabled.");
				return false;
			}
			Trace.WriteLine(string.Format("Reading the p3d ('{0}')...", srcPath));
			P3D instance = P3D.GetInstance(srcPath);
			if (instance is MLOD)
			{
				Trace.WriteLine(string.Format("'{0}' is already in editable MLOD format", srcPath));
			}
			else
			{
				ODOL odol = instance as ODOL;
				if (odol != null)
				{
					Trace.WriteLine("ODOL was loaded successfully.");
					Trace.WriteLine("Start conversion...");
					MLOD mlod = Conversion.ODOL2MLOD(odol);
					Trace.WriteLine("Conversion successful.");
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(srcPath);
					string directoryName = Path.GetDirectoryName(srcPath);
					dstPath = (dstPath ?? Path.Combine(directoryName, fileNameWithoutExtension + "_mlod.p3d"));
					Trace.WriteLine("Saving...");
					mlod.writeToFile(dstPath, allowOverwriting);
					Trace.WriteLine(string.Format("MLOD successfully saved to '{0}'", dstPath));
					return true;
				}
				Trace.WriteLine(string.Format("'{0}' could not be loaded.", srcPath));
			}
			return false;
		}
	}
}
