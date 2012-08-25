using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace snowpack
{
	public class FDChecksum
	{
		public string checksum { get; set; }
		public bool running { get; set; }
		private string FilePath;
		private MD5 md5sum;
		
		public FDChecksum (string path)
		{
			running = false;
			checksum = null;
			
			if(File.Exists(path)) FilePath = path;
			else FilePath = null;
		}
		
		public void CalculateChecksum()
		{
			md5sum = MD5.Create();
			
			FileStream file = new FileStream(this.FilePath, FileMode.Open);
			
			running = true;
			byte[] data = md5sum.ComputeHash((Stream)file);
			
			StringBuilder sb = new StringBuilder();
			
			for (int i = 0; i < data.Length; i++)
			{
				sb.Append(data[i].ToString("x2"));
			}
			
			checksum = sb.ToString();
			running = false;
		}
	}
}

