using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Costura
{
	// Token: 0x02000003 RID: 3
	[CompilerGenerated]
	internal static class AssemblyLoader
	{
		// Token: 0x06000006 RID: 6 RVA: 0x0000205F File Offset: 0x0000025F
		private static string CultureToString(CultureInfo culture)
		{
			if (culture == null)
			{
				return "";
			}
			return culture.Name;
		}

		// Token: 0x06000007 RID: 7 RVA: 0x0000239C File Offset: 0x0000059C
		private static Assembly ReadExistingAssembly(AssemblyName name)
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			Assembly[] assemblies = currentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				AssemblyName name2 = assembly.GetName();
				if (string.Equals(name2.Name, name.Name, StringComparison.InvariantCultureIgnoreCase) && string.Equals(AssemblyLoader.CultureToString(name2.CultureInfo), AssemblyLoader.CultureToString(name.CultureInfo), StringComparison.InvariantCultureIgnoreCase))
				{
					return assembly;
				}
			}
			return null;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002414 File Offset: 0x00000614
		private static void CopyTo(Stream source, Stream destination)
		{
			byte[] array = new byte[81920];
			int count;
			while ((count = source.Read(array, 0, array.Length)) != 0)
			{
				destination.Write(array, 0, count);
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002448 File Offset: 0x00000648
		private static Stream LoadStream(string fullname)
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			if (fullname.EndsWith(".zip"))
			{
				using (Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(fullname))
				{
					using (DeflateStream deflateStream = new DeflateStream(manifestResourceStream, CompressionMode.Decompress))
					{
						MemoryStream memoryStream = new MemoryStream();
						AssemblyLoader.CopyTo(deflateStream, memoryStream);
						memoryStream.Position = 0L;
						return memoryStream;
					}
				}
			}
			return executingAssembly.GetManifestResourceStream(fullname);
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000024CC File Offset: 0x000006CC
		private static Stream LoadStream(Dictionary<string, string> resourceNames, string name)
		{
			string fullname;
			if (resourceNames.TryGetValue(name, out fullname))
			{
				return AssemblyLoader.LoadStream(fullname);
			}
			return null;
		}

		// Token: 0x0600000B RID: 11 RVA: 0x000024EC File Offset: 0x000006EC
		private static byte[] ReadStream(Stream stream)
		{
			byte[] array = new byte[stream.Length];
			stream.Read(array, 0, array.Length);
			return array;
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002514 File Offset: 0x00000714
		private static Assembly ReadFromEmbeddedResources(Dictionary<string, string> assemblyNames, Dictionary<string, string> symbolNames, AssemblyName requestedAssemblyName)
		{
			string text = requestedAssemblyName.Name.ToLowerInvariant();
			if (requestedAssemblyName.CultureInfo != null && !string.IsNullOrEmpty(requestedAssemblyName.CultureInfo.Name))
			{
				text = string.Format("{0}.{1}", requestedAssemblyName.CultureInfo.Name, text);
			}
			byte[] rawAssembly;
			using (Stream stream = AssemblyLoader.LoadStream(assemblyNames, text))
			{
				if (stream == null)
				{
					return null;
				}
				rawAssembly = AssemblyLoader.ReadStream(stream);
			}
			using (Stream stream2 = AssemblyLoader.LoadStream(symbolNames, text))
			{
				if (stream2 != null)
				{
					byte[] rawSymbolStore = AssemblyLoader.ReadStream(stream2);
					return Assembly.Load(rawAssembly, rawSymbolStore);
				}
			}
			return Assembly.Load(rawAssembly);
		}

		// Token: 0x0600000D RID: 13 RVA: 0x000025D4 File Offset: 0x000007D4
		public static Assembly ResolveAssembly(string assemblyName)
		{
			if (AssemblyLoader.nullCache.ContainsKey(assemblyName))
			{
				return null;
			}
			AssemblyName assemblyName2 = new AssemblyName(assemblyName);
			Assembly assembly = AssemblyLoader.ReadExistingAssembly(assemblyName2);
			if (assembly != null)
			{
				return assembly;
			}
			assembly = AssemblyLoader.ReadFromEmbeddedResources(AssemblyLoader.assemblyNames, AssemblyLoader.symbolNames, assemblyName2);
			if (assembly == null)
			{
				AssemblyLoader.nullCache.Add(assemblyName, true);
				if (assemblyName2.Flags == AssemblyNameFlags.Retargetable)
				{
					assembly = Assembly.Load(assemblyName2);
				}
			}
			return assembly;
		}

		// Token: 0x0600000E RID: 14 RVA: 0x00002638 File Offset: 0x00000838
		// Note: this type is marked as 'beforefieldinit'.
		static AssemblyLoader()
		{
			AssemblyLoader.assemblyNames.Add("bisdll", "costura.bisdll.dll.zip");
			AssemblyLoader.symbolNames.Add("bisdll", "costura.bisdll.pdb.zip");
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002070 File Offset: 0x00000270
		private static Assembly <Attach>b__0(object s, ResolveEventArgs e)
		{
			return AssemblyLoader.ResolveAssembly(e.Name);
		}

		// Token: 0x06000010 RID: 16 RVA: 0x0000268C File Offset: 0x0000088C
		public static void Attach()
		{
			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.AssemblyResolve += ((object s, ResolveEventArgs e) => AssemblyLoader.ResolveAssembly(e.Name));
		}

		// Token: 0x04000001 RID: 1
		private static readonly Dictionary<string, bool> nullCache = new Dictionary<string, bool>();

		// Token: 0x04000002 RID: 2
		private static readonly Dictionary<string, string> assemblyNames = new Dictionary<string, string>();

		// Token: 0x04000003 RID: 3
		private static readonly Dictionary<string, string> symbolNames = new Dictionary<string, string>();

		// Token: 0x04000004 RID: 4
		private static ResolveEventHandler CS$<>9__CachedAnonymousMethodDelegate1;
	}
}
