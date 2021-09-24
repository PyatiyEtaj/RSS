using System;
using System.Collections.Generic;
using System.IO;


/*
данный код был вытащен из AddinManager, расширения для Revit с помощью dnSpy
все что здесь есть находится почти в первозданном виде
*/
namespace RSSAddinManager
{
	public static class FileUtils
	{
		// Token: 0x06000075 RID: 117 RVA: 0x00004172 File Offset: 0x00002372
		public static DateTime GetModifyTime(string filePath)
		{
			return File.GetLastWriteTime(filePath);
		}

		// Token: 0x06000076 RID: 118 RVA: 0x0000417C File Offset: 0x0000237C
		public static string CreateTempFolder(string prefix)
		{
			string tempPath = Path.GetTempPath();
			DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(tempPath, "RevitAddins"));
			if (!directoryInfo.Exists)
			{
				directoryInfo.Create();
			}
			foreach (DirectoryInfo directoryInfo2 in directoryInfo.GetDirectories())
			{
				try
				{
					Directory.Delete(directoryInfo2.FullName, true);
				}
				catch (Exception)
				{
				}
			}
			string str = string.Format("{0:yyyyMMdd_HHmmss_ffff}", DateTime.Now);
			string path = Path.Combine(directoryInfo.FullName, prefix + str);
			DirectoryInfo directoryInfo3 = new DirectoryInfo(path);
			directoryInfo3.Create();
			return directoryInfo3.FullName;
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00004230 File Offset: 0x00002430
		public static void SetWriteable(string fileName)
		{
			if (File.Exists(fileName))
			{
				FileAttributes fileAttributes = File.GetAttributes(fileName) & ~FileAttributes.ReadOnly;
				File.SetAttributes(fileName, fileAttributes);
			}
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00004256 File Offset: 0x00002456
		public static bool SameFile(string file1, string file2)
		{
			return 0 == string.Compare(file1.Trim(), file2.Trim(), true);
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00004270 File Offset: 0x00002470
		public static bool CreateFile(string filePath)
		{
			if (File.Exists(filePath))
			{
				return true;
			}
			try
			{
				string directoryName = Path.GetDirectoryName(filePath);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				using (new FileInfo(filePath).Create())
				{
					FileUtils.SetWriteable(filePath);
				}
			}
			catch (Exception)
			{
				return false;
			}
			return File.Exists(filePath);
		}

		// Token: 0x0600007A RID: 122 RVA: 0x000042E8 File Offset: 0x000024E8
		public static void DeleteFile(string fileName)
		{
			if (File.Exists(fileName))
			{
				FileAttributes fileAttributes = File.GetAttributes(fileName) & ~FileAttributes.ReadOnly;
				File.SetAttributes(fileName, fileAttributes);
				try
				{
					File.Delete(fileName);
				}
				catch (Exception)
				{
				}
			}
		}

		// Token: 0x0600007B RID: 123 RVA: 0x0000432C File Offset: 0x0000252C
		public static bool FileExistsInFolder(string filePath, string destFolder)
		{
			string path = Path.Combine(destFolder, Path.GetFileName(filePath));
			return File.Exists(path);
		}

		// Token: 0x0600007C RID: 124 RVA: 0x0000434C File Offset: 0x0000254C
		public static string CopyFileToFolder(string sourceFilePath, string destFolder, bool onlyCopyRelated, List<FileInfo> allCopiedFiles)
		{
			if (!File.Exists(sourceFilePath))
			{
				return null;
			}
			string directoryName = Path.GetDirectoryName(sourceFilePath);
			if (onlyCopyRelated)
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath);
				string searchPattern = fileNameWithoutExtension + ".*";
				string[] files = Directory.GetFiles(directoryName, searchPattern, SearchOption.TopDirectoryOnly);
				foreach (string text in files)
				{
					string fileName = Path.GetFileName(text);
					string text2 = Path.Combine(destFolder, fileName);
					bool flag = FileUtils.CopyFile(text, text2);
					if (flag)
					{
						FileInfo item = new FileInfo(text2);
						allCopiedFiles.Add(item);
					}
				}
			}
			else
			{
				long folderSize = FileUtils.GetFolderSize(directoryName);
				if (folderSize > 50L)
				{
					FileUtils.CopyFileToFolder(sourceFilePath, destFolder, true, allCopiedFiles);
				}
				else
				{
					FileUtils.CopyDirectory(directoryName, destFolder, allCopiedFiles);
				}
			}
			string text3 = Path.Combine(destFolder, Path.GetFileName(sourceFilePath));
			if (File.Exists(text3))
			{
				return text3;
			}
			return null;
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00004448 File Offset: 0x00002648
		public static bool CopyFile(string sourceFilename, string destinationFilename)
		{
			if (!File.Exists(sourceFilename))
			{
				return false;
			}
			FileAttributes fileAttributes = File.GetAttributes(sourceFilename) & ~FileAttributes.ReadOnly;
			File.SetAttributes(sourceFilename, fileAttributes);
			if (File.Exists(destinationFilename))
			{
				FileAttributes fileAttributes2 = File.GetAttributes(destinationFilename) & ~FileAttributes.ReadOnly;
				File.SetAttributes(destinationFilename, fileAttributes2);
				File.Delete(destinationFilename);
			}
			try
			{
				if (!Directory.Exists(Path.GetDirectoryName(destinationFilename)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(destinationFilename));
				}
				File.Copy(sourceFilename, destinationFilename, true);
			}
			catch (Exception)
			{
				return false;
			}
			return File.Exists(destinationFilename);
		}

		// Token: 0x0600007E RID: 126 RVA: 0x000044D0 File Offset: 0x000026D0
		public static void CopyDirectory(string sourceDir, string desDir, List<FileInfo> allCopiedFiles)
		{
			try
			{
				string[] directories = Directory.GetDirectories(sourceDir, "*.*", SearchOption.AllDirectories);
				foreach (string text in directories)
				{
					string str = text.Replace(sourceDir, "");
					string path = desDir + str;
					if (!Directory.Exists(path))
					{
						Directory.CreateDirectory(path);
					}
				}
				string[] files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
				foreach (string text2 in files)
				{
					string str2 = text2.Replace(sourceDir, "");
					string text3 = desDir + str2;
					if (!Directory.Exists(Path.GetDirectoryName(text3)))
					{
						Directory.CreateDirectory(Path.GetDirectoryName(text3));
					}
					if (FileUtils.CopyFile(text2, text3))
					{
						allCopiedFiles.Add(new FileInfo(text3));
					}
				}
			}
			catch (Exception)
			{
			}
		}

		// Token: 0x0600007F RID: 127 RVA: 0x000045B8 File Offset: 0x000027B8
		public static long GetFolderSize(string folderPath)
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
			long num = 0L;
			foreach (FileSystemInfo fileSystemInfo in directoryInfo.GetFileSystemInfos())
			{
				if (fileSystemInfo is FileInfo)
				{
					num += ((FileInfo)fileSystemInfo).Length;
				}
				else
				{
					num += FileUtils.GetFolderSize(fileSystemInfo.FullName);
				}
			}
			return num / 1024L / 1024L;
		}

		// Token: 0x04000036 RID: 54
		private const string TempFolderName = "RevitAddins";
	}
}
