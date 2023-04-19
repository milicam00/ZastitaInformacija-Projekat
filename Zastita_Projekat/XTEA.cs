using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Zastita_Projekat
{

	public class XTEA
	{

		private const uint Rounds = 32;
		private static XTEA instance;
		private uint[] key;
		
		#region Instance
		XTEA()
		{
			key = null;
		}

		public static XTEA GetInstance()
		{
			if (instance != null)
				return instance;
			instance = new XTEA();
			return instance;
		}
		#endregion

		#region Properties
		public uint[] Key
		{
			get { return key; }
			set
			{ 
				key = CreateKey(Encoding.Unicode.GetBytes(System.Text.Encoding.ASCII.GetString(value.SelectMany(BitConverter.GetBytes).ToArray()).ToCharArray()));
			}
		}
		#endregion

		#region Encrypt and Decrypt
		public string Encrypt(string data)
		{
			var dataBytes = Encoding.Unicode.GetBytes(data);
			var result = EncryptByteStream(dataBytes);
			return Convert.ToBase64String(result);
		}


		public string Decrypt(string data)
		{
			var dataBytes = Convert.FromBase64String(data);
			var result = DecryptByteStream(dataBytes);
			return Encoding.Unicode.GetString(result);
		}


		public byte[] EncryptByteStream(byte[] data)
		{

			var blockBuffer = new uint[2];
			var result = new byte[NextMultipleOf8(data.Length + 4)];
			var lengthBuffer = BitConverter.GetBytes(data.Length);
			Array.Copy(lengthBuffer, result, lengthBuffer.Length);
			Array.Copy(data, 0, result, lengthBuffer.Length, data.Length);
			using (var stream = new MemoryStream(result))
			{
				using (var writer = new BinaryWriter(stream))

					for (int i = 0; i < result.Length; i += 8)
					{
						blockBuffer[0] = BitConverter.ToUInt32(result, i);
						blockBuffer[1] = BitConverter.ToUInt32(result, i + 4);
						EncryptUintStream(Rounds, blockBuffer);
						writer.Write(blockBuffer[0]);
						writer.Write(blockBuffer[1]);
					}

			}
			return result;
		}


		public byte[] DecryptByteStream(byte[] data)
		{
			

			var blockBuffer = new uint[2];
			var buffer = new byte[data.Length];
			Array.Copy(data, buffer, data.Length);
			using (var stream = new MemoryStream(buffer))
			{
				using (var writer = new BinaryWriter(stream))
				{
					for (int i = 0; i < buffer.Length; i += 8)
					{
						blockBuffer[0] = BitConverter.ToUInt32(buffer, i);
						blockBuffer[1] = BitConverter.ToUInt32(buffer, i + 4);
						DecryptUIntStream(Rounds, blockBuffer);
						writer.Write(blockBuffer[0]);
						writer.Write(blockBuffer[1]);
					}
				}
			}
			
			var length = BitConverter.ToUInt32(buffer, 0);
			if (length > buffer.Length - 4) throw new ArgumentException("Invalid encrypted data");
			var result = new byte[length];
			Array.Copy(buffer, 4, result, 0, length);
			return result;
		}

		#endregion

		#region Methods
		private static int NextMultipleOf8(int length)
		{
			
			return (length + 7) / 8 * 8; 
		}


		private uint[] CreateKey(byte[] key)
		{
		
			var hash = new byte[16];
			for (int i = 0; i < key.Length; i++)
			{
				hash[i % 16] = (byte)((31 * hash[i % 16]) ^ key[i]);
			}
			for (int i = key.Length; i < hash.Length; i++)
			{ 
				hash[i] = (byte)(17 * i ^ key[i % key.Length]);
			}
			return new[] {
				BitConverter.ToUInt32(hash, 0), BitConverter.ToUInt32(hash, 4),
				BitConverter.ToUInt32(hash, 8), BitConverter.ToUInt32(hash, 12)
			};
		}
		#endregion
		#region Block Operations

		public void EncryptUintStream(uint rounds, uint[] v)
		{
			uint v0 = v[0], v1 = v[1], sum = 0, delta = 0x9E3779B9;
			for (uint i = 0; i < rounds; i++)
			{
				v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
				sum += delta;
				v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
			}
			v[0] = v0;
			v[1] = v1;
		}


		private void DecryptUIntStream(uint rounds, uint[] v)
		{
			uint v0 = v[0], v1 = v[1], delta = 0x9E3779B9, sum = delta * rounds;
			for (uint i = 0; i < rounds; i++)
			{
				v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
				sum -= delta;
				v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
			}
			v[0] = v0;
			v[1] = v1;
		}
		#endregion


	} }