﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CraftSynth.BuildingBlocks
{
	public static class Encryption
	{
		//Symetric algorithms:
		//  RijndaelManaged
		//  RC2
		//  DES
		//  TripleDES

		//Asymetric algorithms:
		//  RSA
		//  DSA

		//Hash algorithms
		//-nonkeyed:
		//  MD5
		//  RIPEMD160
		//  SHA1
		//  SHA256
		//  SHA384
		//  SHA512
		//-keyed
		//  HMACSHA1
		//  MACTripleDES

		//Signing algorithms:
		//  RSA
		//  DSA

		private static string saltString = "This is salt string";

		private static Rfc2898DeriveBytes GenerateRijndaelKey(string password, string saltString)
		{
			Rfc2898DeriveBytes key = null;


			byte[] salt = Encoding.ASCII.GetBytes(saltString);
			key = new Rfc2898DeriveBytes(password, salt);

			return key;
		}

		private static RijndaelManaged GetRijndaelAlgorithm(Rfc2898DeriveBytes key)
		{
			RijndaelManaged algorithm = new RijndaelManaged();
			algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
			algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

			return algorithm;
		}

		/// <summary>
		/// Encrypts provided data using symmetric Rijndael algorithm and provided password.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static byte[] EncryptWithRijndaelAlgorithm(this byte[] data, string password)
		{
			byte[] r;

			Rfc2898DeriveBytes key = GenerateRijndaelKey(password, saltString);
			using (RijndaelManaged algorithm = GetRijndaelAlgorithm(key))
			{
				ICryptoTransform encryptor = algorithm.CreateEncryptor();
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (CryptoStream encryptStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
					{
						encryptStream.Write(data, 0, data.Length);
					}
					r = memoryStream.ToArray();
				}
			}

			return r;
		}

		/// <summary>
		/// Decrypts provided data using symmetric Rijndael algorithm and provided password.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="password"></param>
		/// <returns></returns>
		public static byte[] DecryptWithRijndaelAlgorithm(this byte[] data, string password)
		{
			byte[] r;

			Rfc2898DeriveBytes key = GenerateRijndaelKey(password, saltString);
			using (RijndaelManaged algorithm = GetRijndaelAlgorithm(key))
			{
				ICryptoTransform decryptor = algorithm.CreateDecryptor();
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (CryptoStream decryptStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
					{
						decryptStream.Write(data, 0, data.Length);
						r = memoryStream.ToArray();
					}
				}
			}

			return r;
		}

		/// <summary>
		/// Encrypts provided data using symmetric Rijndael algorithm. Private secret key data is auto-generated and returned.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="autoGeneratedKey"></param>
		/// <param name="autoGeneratedKeyIV"></param>
		/// <returns></returns>
		public static byte[] EncryptWithRijndaelAlgorithm(this byte[] data, out byte[] autoGeneratedKey, out byte[] autoGeneratedKeyIV)
		{
			byte[] r;

			using (RijndaelManaged algorithm = new RijndaelManaged())
			{
				autoGeneratedKey = algorithm.Key;
				autoGeneratedKeyIV = algorithm.IV;
				ICryptoTransform encryptor = algorithm.CreateEncryptor();
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (CryptoStream encryptStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
					{
						encryptStream.Write(data, 0, data.Length);
						r = memoryStream.ToArray();
					}
				}
			}

			return r;
		}

		/// <summary>
		/// Decrypts provided data using symmetric Rijndael algorithm and provided private secret key data.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="key"></param>
		/// <param name="keyIV"></param>
		/// <returns></returns>
		public static byte[] DecryptWithRijndaelAlgorithm(this byte[] data, byte[] key, byte[] keyIV)
		{
			byte[] r;

			using (RijndaelManaged algorithm = new RijndaelManaged())
			{
				algorithm.Key = key;
				algorithm.IV = keyIV;
				ICryptoTransform decryptor = algorithm.CreateDecryptor();
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (CryptoStream decryptStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
					{
						decryptStream.Write(data, 0, data.Length);
						r = memoryStream.ToArray();
					}
				}
			}

			return r;
		}

		/// <summary>
		/// Returns xml string containing public key for RSA asymmetric algorithm. 
		/// Private key is securely stored under specified name.
		/// </summary>
		/// <param name="privateKeyContainerName"></param>
		/// <returns></returns>
		public static string GeneratePublicKeyXmlAndStorePrivateKey(string privateKeyContainerName)
		{
			string publicKeyXml = null;

			CspParameters cspParameters = new CspParameters();
			cspParameters.KeyContainerName = privateKeyContainerName;

			RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(cspParameters);
			rsaCryptoServiceProvider.PersistKeyInCsp = true;
			publicKeyXml = rsaCryptoServiceProvider.ToXmlString(false);

			return publicKeyXml;
		}

		/// <summary>
		/// Encrypts provided data using asymmetric RSA algotithm and provided public key xml string.
		/// </summary>
		/// <param name="publicKeyXml"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] EncryptWithRSAAlgorithm(this byte[] data, string publicKeyXml)
		{
			byte[] encryptedData = null;

			RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider();
			rsaCryptoServiceProvider.FromXmlString(publicKeyXml);

			encryptedData = rsaCryptoServiceProvider.Encrypt(data, false);

			return encryptedData;
		}

		/// <summary>
		/// Decrypts provided data using asymmetric RSA algotithm and private key from specified container.
		/// </summary>
		/// <param name="privateKeyContainerName"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] DecryptWithRSAAlgorithm(this byte[] data, string privateKeyContainerName)
		{
			byte[] decryptedData = null;

			CspParameters cspParameters = new CspParameters();
			cspParameters.KeyContainerName = privateKeyContainerName;

			RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(cspParameters);
			rsaCryptoServiceProvider.PersistKeyInCsp = true;

			decryptedData = rsaCryptoServiceProvider.Decrypt(data, false);

			return decryptedData;
		}

		/// <summary>
		/// This algorithm is added in Framework 2.0 as improvement to MD5 algorithm.
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static byte[] GetHashUsingRIPEMD160Algorithm(this byte[] data)
		{
			byte[] hash = null;
			//TODO: port to DotNet6
			throw new NotImplementedException();
			//RIPEMD160Managed algorithm = new RIPEMD160Managed();
			//algorithm.ComputeHash(data);
			//hash = algorithm.Hash;

			//return hash;
		}
		
		/// <summary>
		/// Returns bytes of specified string. No encoding is used.
		/// PROS: No data loss as with encoding when char is illegal.
		/// CONS: This and ToString method must be both used on same machine - Other case not tested.
		/// Source: http://stackoverflow.com/questions/472906/net-string-to-byte-array-c-sharp
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static byte[] StringToBytes(this string s)
		{
			byte[] bytes = new byte[s.Length * sizeof(char)];
			System.Buffer.BlockCopy(s.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}

		/// <summary>
		/// Returns string build up with bytes - no encoding is used.
		/// PROS: No data loss as with encoding when char is illegal.
		/// CONS: This and ToString method must be both used on same machine - Other case not tested.
		/// Source: http://stackoverflow.com/questions/472906/net-string-to-byte-array-c-sharp
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static string BytesToString(this byte[] bytes)
		{
			char[] chars = new char[bytes.Length / sizeof(char)];
			System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
			return new string(chars);
		}

		public static byte[] GetHashUsingMD5Algorithm(this byte[] data)
		{
			byte[] hash = null;

			MD5 algorithm = new MD5CryptoServiceProvider();
			algorithm.ComputeHash(data);
			hash = algorithm.Hash;

			return hash;
		}

		public static string GetHashUsingMD5Algorithm(this string text)
		{
			byte[] bytes = StringToBytes(text);
			bytes = GetHashUsingMD5Algorithm(bytes);
			text = BytesToString(bytes);
			return text;
		}

		public static string ToUnicodeStringFromBytes(this byte[] bytes)
		{
			return Encoding.Unicode.GetString(bytes);
		}

		public static byte[] ToBytesFromUnicodeString(this string unicodeString)
		{
			return Encoding.Unicode.GetBytes(unicodeString);
		}

		public static string GetHashAsUnicodeStringUsingMD5Algorithm(this string text)
		{
			byte[] bytes = ToBytesFromUnicodeString(text);
			bytes = GetHashUsingMD5Algorithm(bytes);
			text = ToUnicodeStringFromBytes(bytes);
			return text;
		}

		public static string ToHex(this byte[] bytes, bool upperCase)
		{
			StringBuilder r = new StringBuilder(bytes.Length * 2);

			for (int i = 0; i < bytes.Length; i++)
			{
				r.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
			}

			return r.ToString();
		}

		public static byte[] FromHex(this string hex)
		{
			return Enumerable.Range(0, hex.Length)
					 .Where(x => x % 2 == 0)
					 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
					 .ToArray();
		}

		public static string GetHashAsHexStringUsingMD5Algorithm(this string text)
		{
			byte[] bytes = ToBytesFromUnicodeString(text);
			bytes = GetHashUsingMD5Algorithm(bytes);
			text = ToHex(bytes, true);
			return text;
		}

		private static byte[] CombineBytes(this byte[] a, byte[] b)
		{
			byte[] c = new byte[a.Length + b.Length];
			System.Buffer.BlockCopy(a, 0, c, 0, a.Length);
			System.Buffer.BlockCopy(b, 0, c, a.Length, b.Length);
			return c;
		}

		public static byte[] GetHashUsingMD5AlgorithmWithSalt(this byte[] data, string salt)
		{
			byte[] saltBytes = Encoding.UTF8.GetBytes(salt);
			data = CombineBytes(data, saltBytes);
			data = CombineBytes(saltBytes,data);
			data = GetHashUsingMD5Algorithm(data);
			return data;
		}

		public static byte[] GetHashUsingSHA512Algorithm(this byte[] data)
		{
			byte[] hash = null;

			SHA512Managed algorithm = new SHA512Managed();
			algorithm.ComputeHash(data);
			hash = algorithm.Hash;

			return hash;
		}

		public static byte[] GetHashUsingHMACSHA512Algorithm(this byte[] data, string password)
		{
			byte[] hash = null;

			byte[] key = GenerateRijndaelKey(password, saltString).GetBytes(16);

			HMACSHA512 algorithm = new HMACSHA512(key);
			algorithm.ComputeHash(data);
			hash = algorithm.Hash;

			return hash;
		}

		//public static byte[] GetHashUsingMACTripleDESAlgorithm(this byte[] data, string password)
		//{
		//	byte[] hash = null;

		//	byte[] key = GenerateRijndaelKey(password, saltString).GetBytes(24);

		//	MACTripleDES algorithm = new MACTripleDES(key);
		//	algorithm.ComputeHash(data);
		//	hash = algorithm.Hash;

		//	return hash;
		//}

		public static byte[] SignDataUsingDSAAlgorithm(this byte[] data, out string generatedPublicKeyXml)
		{
			byte[] signature = null;

			DSACryptoServiceProvider algorithm = new DSACryptoServiceProvider();
			signature = algorithm.SignData(data);
			generatedPublicKeyXml = algorithm.ToXmlString(false);

			return signature;

		}

		public static bool VerifyDataUsingDSAAlgorithm(this byte[] data, byte[] signature, string publicKeyXml)
		{
			bool verifiedSuccessfully = false;

			DSACryptoServiceProvider algorithm = new DSACryptoServiceProvider();
			algorithm.FromXmlString(publicKeyXml);
			verifiedSuccessfully = algorithm.VerifyData(data, signature);

			return verifiedSuccessfully;

		}

		//Encrptinon method improvements that did not succeed:

		//            #region Encryption

		//        private static string saltString = "This is salt string";

		//        private static Rfc2898DeriveBytes GenerateRijndaelKey(string password, string saltString)
		//        {
		//            Rfc2898DeriveBytes key = null;


		//            byte[] salt = Encoding.ASCII.GetBytes(saltString);
		//            key = new Rfc2898DeriveBytes(password, salt);

		//            return key;
		//        }

		//        private static RijndaelManaged GetRijndaelAlgorithm(Rfc2898DeriveBytes key)
		//        {       
		//            RijndaelManaged algorithm = new RijndaelManaged();
		//            algorithm.Key = key.GetBytes(algorithm.KeySize/8);
		//            algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

		//            return algorithm;
		//        }

		//        public static byte[] EncryptWithRijndaelAlgorithm(byte[] data, string password)
		//        {
		//            byte[] encryptedData = null;

		//            MemoryStream memoryStream = new MemoryStream();
		//            CryptoStream encryptStream = null;
		//            try
		//            {
		//                Rfc2898DeriveBytes key = GenerateRijndaelKey(password, saltString);
		//                RijndaelManaged algorithm = GetRijndaelAlgorithm(key);
		//                ICryptoTransform encryptor = algorithm.CreateEncryptor();
		//                encryptStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
		//                encryptStream.Write(data, 0, data.Length);
		//                encryptedData =  memoryStream.ToArray();
		//            }
		//            finally{
		//                try
		//                {
		//                    encryptStream.Close();
		//                    memoryStream.Close();
		//                }
		//                catch (Exception) { }
		//            }
		//            return encryptedData;
		//        }

		//        public static byte[] DecryptWithRijndaelAlgorithm(byte[] data, string password)
		//        {
		//            byte[] decryptedData = null;

		//            MemoryStream memoryStream = new MemoryStream();
		//            CryptoStream decryptStream = null;
		//            try
		//            {
		//                Rfc2898DeriveBytes key = GenerateRijndaelKey(password, saltString);
		//                RijndaelManaged algorithm = GetRijndaelAlgorithm(key);
		//                ICryptoTransform decryptor = algorithm.CreateDecryptor();
		//                memoryStream = new MemoryStream();
		//                decryptStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write);
		//                decryptStream.Write(data, 0, data.Length);
		//                decryptedData = memoryStream.ToArray();
		//            }
		//            finally
		//            {
		//                try
		//                {
		//                    decryptStream.Close();
		//                    memoryStream.Close();
		//                }
		//                catch (Exception) { }
		//            }
		//            return decryptedData;
		//        }

		//        public static byte[] EncryptWithRijndaelAlgorithm(byte[] data, out byte[] autoGeneratedKey, 

		//out byte[] autoGeneratedKeyIV)
		//        {
		//            byte[] encryptedData = null;

		//            MemoryStream memoryStream = new MemoryStream();
		//            CryptoStream encryptStream = null;
		//            try
		//            {
		//                RijndaelManaged algorithm = new RijndaelManaged();
		//                autoGeneratedKey = algorithm.Key;
		//                autoGeneratedKeyIV = algorithm.IV;
		//                ICryptoTransform encryptor = algorithm.CreateEncryptor();                
		//                encryptStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
		//                encryptStream.Write(data, 0, data.Length);
		//                encryptedData = memoryStream.ToArray();
		//            }
		//            finally
		//            {
		//                try
		//                {
		//                    encryptStream.Close();
		//                    memoryStream.Close();
		//                }
		//                catch (Exception) { }
		//            }
		//            return encryptedData;
		//        }

		//        public static byte[] DecryptWithRijndaelAlgorithm(byte[] data, byte[] key, byte[] keyIV)
		//        {
		//            byte[] decryptedData = null;

		//            MemoryStream memoryStream = new MemoryStream();
		//            CryptoStream decryptStream = null;
		//            try
		//            {
		//                RijndaelManaged algorithm = new RijndaelManaged();
		//                algorithm.Key = key;
		//                algorithm.IV = keyIV;
		//                ICryptoTransform decryptor = algorithm.CreateDecryptor();
		//                decryptStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write);
		//                decryptStream.Write(data, 0, data.Length);
		//                decryptedData = memoryStream.ToArray();
		//            }
		//            finally
		//            {
		//                try
		//                {
		//                    decryptStream.Close();
		//                    memoryStream.Close();
		//                }
		//                catch (Exception) { }
		//            }
		//            return decryptedData;
		//        }

		//        #endregion Encryption
	}
}
