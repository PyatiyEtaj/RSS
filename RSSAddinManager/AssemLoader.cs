using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

/*
данный код был вытащен из AddinManager, расширения для Revit с помощью dnSpy
все что здесь есть находится почти в первозданном виде
*/

namespace RSSAddinManager
{
    class AssemLoader
	{// Token: 0x17000051 RID: 81
	 // (get) Token: 0x06000138 RID: 312 RVA: 0x00008B7D File Offset: 0x00006D7D
	 // (set) Token: 0x06000139 RID: 313 RVA: 0x00008B85 File Offset: 0x00006D85
		public string OriginalFolder
		{
			get
			{
				return this.m_originalFolder;
			}
			set
			{
				this.m_originalFolder = value;
			}
		}

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x0600013A RID: 314 RVA: 0x00008B8E File Offset: 0x00006D8E
		// (set) Token: 0x0600013B RID: 315 RVA: 0x00008B96 File Offset: 0x00006D96
		public string TempFolder
		{
			get
			{
				return this.m_tempFolder;
			}
			set
			{
				this.m_tempFolder = value;
			}
		}

		// Token: 0x0600013C RID: 316 RVA: 0x00008B9F File Offset: 0x00006D9F
		public AssemLoader()
		{
			this.m_tempFolder = string.Empty;
			this.m_refedFolders = new List<string>();
			this.m_copiedFiles = new Dictionary<string, DateTime>();
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00008BC8 File Offset: 0x00006DC8
		public void CopyGeneratedFilesBack()
		{
			string[] files = Directory.GetFiles(this.m_tempFolder, "*.*", SearchOption.AllDirectories);
			foreach (string text in files)
			{
				if (this.m_copiedFiles.ContainsKey(text))
				{
					DateTime t = this.m_copiedFiles[text];
					FileInfo fileInfo = new FileInfo(text);
					if (fileInfo.LastWriteTime > t)
					{
						string str = text.Remove(0, this.m_tempFolder.Length);
						string destinationFilename = this.m_originalFolder + str;
						FileUtils.CopyFile(text, destinationFilename);
					}
				}
				else
				{
					string str2 = text.Remove(0, this.m_tempFolder.Length);
					string destinationFilename2 = this.m_originalFolder + str2;
					FileUtils.CopyFile(text, destinationFilename2);
				}
			}
		}

		// Token: 0x0600013E RID: 318 RVA: 0x00008C93 File Offset: 0x00006E93
		public void HookAssemblyResolve()
		{
			AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;
		}

		// Token: 0x0600013F RID: 319 RVA: 0x00008CAB File Offset: 0x00006EAB
		public void UnhookAssemblyResolve()
		{
			AppDomain.CurrentDomain.AssemblyResolve -= this.CurrentDomain_AssemblyResolve;
		}

		// Token: 0x06000140 RID: 320 RVA: 0x00008CC4 File Offset: 0x00006EC4
		public Assembly LoadAddinsToTempFolder(string originalFilePath, bool parsingOnly)
		{
			if (string.IsNullOrEmpty(originalFilePath) || originalFilePath.StartsWith("\\") || !File.Exists(originalFilePath))
			{
				return null;
			}
			this.m_parsingOnly = parsingOnly;
			this.m_originalFolder = Path.GetDirectoryName(originalFilePath);
			StringBuilder stringBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(originalFilePath));
			if (parsingOnly)
			{
				stringBuilder.Append("-Parsing-");
			}
			else
			{
				stringBuilder.Append("-Executing-");
			}
			this.m_tempFolder = FileUtils.CreateTempFolder(stringBuilder.ToString());
			Assembly assembly = this.CopyAndLoadAddin(originalFilePath, parsingOnly);
			if (null == assembly/* || !this.IsAPIReferenced(assembly)*/)
			{
				return null;
			}
			return assembly;
		}

		// Token: 0x06000141 RID: 321 RVA: 0x00008D60 File Offset: 0x00006F60
		private Assembly CopyAndLoadAddin(string srcFilePath, bool onlyCopyRelated)
		{
			string text = string.Empty;
			if (!FileUtils.FileExistsInFolder(srcFilePath, this.m_tempFolder))
			{
				string directoryName = Path.GetDirectoryName(srcFilePath);
				if (!this.m_refedFolders.Contains(directoryName))
				{
					this.m_refedFolders.Add(directoryName);
				}
				List<FileInfo> list = new List<FileInfo>();
				text = FileUtils.CopyFileToFolder(srcFilePath, this.m_tempFolder, onlyCopyRelated, list);
				if (string.IsNullOrEmpty(text))
				{
					return null;
				}
				foreach (FileInfo fileInfo in list)
				{
					this.m_copiedFiles.Add(fileInfo.FullName, fileInfo.LastWriteTime);
				}
			}
			return this.LoadAddin(text);
		}

		// Token: 0x06000142 RID: 322 RVA: 0x00008E20 File Offset: 0x00007020
		private Assembly LoadAddin(string filePath)
		{
			Assembly result = null;
			try
			{
				Monitor.Enter(this);
				result = Assembly.LoadFile(filePath);
			}
			finally
			{
				Monitor.Exit(this);
			}
			return result;
		}

		// Token: 0x06000143 RID: 323 RVA: 0x00008E58 File Offset: 0x00007058
		private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			Assembly result;
			lock (this)
			{
				new AssemblyName(args.Name);
				string text = this.SearchAssemblyFileInTempFolder(args.Name);
				if (File.Exists(text))
				{
					result = this.LoadAddin(text);
				}
				else
				{
					text = this.SearchAssemblyFileInOriginalFolders(args.Name);
					if (string.IsNullOrEmpty(text))
					{
						string[] array = args.Name.Split(new char[]
						{
							','
						});
						string text2 = array[0];
						if (array.Length > 1)
						{
							string text3 = array[2];
							if (text2.EndsWith(".resources", StringComparison.CurrentCultureIgnoreCase) && !text3.EndsWith("neutral", StringComparison.CurrentCultureIgnoreCase))
							{
								text2 = text2.Substring(0, text2.Length - ".resources".Length);
							}
							text = this.SearchAssemblyFileInTempFolder(text2);
							if (File.Exists(text))
							{
								return this.LoadAddin(text);
							}
							text = this.SearchAssemblyFileInOriginalFolders(text2);
						}
					}
					if (string.IsNullOrEmpty(text))
					{
						return null;
					}
					result = this.CopyAndLoadAddin(text, true);
				}
			}
			return result;
		}

		// Token: 0x06000144 RID: 324 RVA: 0x00008FC8 File Offset: 0x000071C8
		private string SearchAssemblyFileInTempFolder(string assemName)
		{
			string[] array = new string[]
			{
				".dll",
				".exe"
			};
			string text = string.Empty;
			string str = assemName.Substring(0, assemName.IndexOf(','));
			foreach (string str2 in array)
			{
				text = this.m_tempFolder + "\\" + str + str2;
				if (File.Exists(text))
				{
					return text;
				}
			}
			return string.Empty;
		}

		// Token: 0x06000145 RID: 325 RVA: 0x0000904C File Offset: 0x0000724C
		private string SearchAssemblyFileInOriginalFolders(string assemName)
		{
			string[] array = new string[]
			{
				".dll",
				".exe"
			};
			string text = string.Empty;
			string text2 = assemName.Substring(0, assemName.IndexOf(','));
			foreach (string str in array)
			{
				text = AssemLoader.m_dotnetDir + "\\" + text2 + str;
				if (File.Exists(text))
				{
					return text;
				}
			}
			foreach (string str2 in array)
			{
				foreach (string str3 in this.m_refedFolders)
				{
					text = str3 + "\\" + text2 + str2;
					if (File.Exists(text))
					{
						return text;
					}
				}
			}
			try
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
				string path = directoryInfo.Parent.FullName + "\\Regression\\_RegressionTools\\";
				if (Directory.Exists(path))
				{
					foreach (string text3 in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
					{
						if (Path.GetFileNameWithoutExtension(text3).Equals(text2, StringComparison.OrdinalIgnoreCase))
						{
							return text3;
						}
					}
				}
			}
			catch (Exception)
			{
			}
			int num = assemName.IndexOf("XMLSerializers", StringComparison.OrdinalIgnoreCase);
			if (num != -1)
			{
				assemName = "System.XML" + assemName.Substring(num + "XMLSerializers".Length);
				return this.SearchAssemblyFileInOriginalFolders(assemName);
			}
			return null;
		}

		// Token: 0x06000146 RID: 326 RVA: 0x00009214 File Offset: 0x00007414
		private bool IsAPIReferenced(Assembly assembly)
		{
			if (string.IsNullOrEmpty(this.m_revitAPIAssemblyFullName))
			{
				foreach (Assembly assembly2 in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (string.Compare(assembly2.GetName().Name, "RevitAPI", true) == 0)
					{
						this.m_revitAPIAssemblyFullName = assembly2.GetName().Name;
						break;
					}
				}
			}
			foreach (AssemblyName assemblyName in assembly.GetReferencedAssemblies())
			{
				if (this.m_revitAPIAssemblyFullName == assemblyName.Name)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x04000087 RID: 135
		private List<string> m_refedFolders;

		// Token: 0x04000088 RID: 136
		private Dictionary<string, DateTime> m_copiedFiles;

		// Token: 0x04000089 RID: 137
		private bool m_parsingOnly;

		// Token: 0x0400008A RID: 138
		private string m_originalFolder;

		// Token: 0x0400008B RID: 139
		private string m_tempFolder;

		// Token: 0x0400008C RID: 140
		private static string m_dotnetDir = Environment.GetEnvironmentVariable("windir") + "\\Microsoft.NET\\Framework\\v2.0.50727";

		// Token: 0x0400008D RID: 141
		public static string m_resolvedAssemPath = string.Empty;

		// Token: 0x0400008E RID: 142
		private string m_revitAPIAssemblyFullName;
	}
}

