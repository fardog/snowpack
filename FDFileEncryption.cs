using System;
using System.Security.Cryptography;
using System.Text;

namespace snowpack
{
	public class FDFileEncryption
	{
		private RijndaelManaged SymmetricKey;

		public FDFileEncryption ()
		{
			RijndaelManaged SymmetricKey = new RijndaelManaged();
			SymmetricKey.Mode = CipherMode.CBC;
			SymmetricKey.Padding = PaddingMode.PKCS7;
		}

		public string GenerateKey ()
		{
			Rijndael RijndaelAlg = Rijndael.Create ();

			StringBuilder sb = new StringBuilder (RijndaelAlg.Key.Length * 2);
			foreach (byte b in RijndaelAlg.Key) {
				sb.AppendFormat ("{0:x2}", b);
			}

			System.Console.WriteLine (sb.ToString ());

			return sb.ToString ();
		}

		public string PrintKey ()
		{
			Rijndael RijndaelAlg = Rijndael.Create ();

			StringBuilder sb = new StringBuilder(RijndaelAlg.Key.Length * 2);
			foreach (byte b in RijndaelAlg.Key)
			{
				sb.AppendFormat("{0:x2}", b);
			}

			return sb.ToString ();
		}
	}
}

