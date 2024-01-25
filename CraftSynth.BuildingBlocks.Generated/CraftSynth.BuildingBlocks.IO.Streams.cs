using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CraftSynth.BuildingBlocks.IO
{
    public class Streams
    {
		/// <summary>
		/// Writes stream content to console screen. If stream is null or empty writes 'NULL' or 'EMPTY'.
		/// </summary>
		/// <param name="theStream"></param>
		static void DumpStream(Stream theStream)
		{
			if (theStream == null)
			{
				Console.WriteLine("NULL");
			}
			else
			{
				if (theStream.Length == 0)
				{
					Console.WriteLine("EMPTY");
				}
				else
				{
					// Move the stream's position to the beginning
					theStream.Position = 0;
					// Go through entire stream and show the contents
					while (theStream.Position != theStream.Length)
					{
						Console.WriteLine("{0:x2}", theStream.ReadByte());
					}
				}
			}
		}

		/// <summary>
		/// Appends bytes to stream.
		/// </summary>
		/// <param name="theStream"></param>
		/// <param name="data"></param>
		/// <exception cref="ArgumentNullException">Thrown when one of parameters is null.</exception>
		static void AppendToStream(Stream theStream, byte[] data)
		{
			if (theStream == null) throw new ArgumentNullException("theStream");
			if (data == null) throw new ArgumentNullException("data");

			// Move the Position to the end
			theStream.Position = theStream.Length;
			// Append some bytes
			theStream.Write(data, 0, data.Length);
		}

		// This method accepts two streams to 
		// compare. A return value of 0 indicates that the contents of the streams
		// are the same. A return value of any other value indicates that the 
		// streams are not the same.
		public static bool CompareStreams(Stream stream1, Stream stream2)
		{
			int stream1byte;
			int stream2byte;

			// Check the stream sizes. If they are not the same, the streams 
			// are not the same.
			if (stream1.Length != stream2.Length)
			{
				// Return false to indicate streams are different
				return false;
			}

			// Read and compare a byte from each stream until either a
			// non-matching set of bytes is found or until the end of
			// stream is reached.
			do
			{
				// Read one byte from each stream.
				stream1byte = stream1.ReadByte();
				stream2byte = stream1.ReadByte();
			}
			while ((stream1byte == stream2byte) && (stream1byte != -1));

			// Close the streams.
			//stream1.Close();
			//stream1.Close();

			// Return the success of the comparison. "stream1byte" is 
			// equal to "stream2byte" at this point only if the streams are 
			// the same.
			return ((stream1byte - stream2byte) == 0);
		}

		// sourceStream is the stream you need to read
		// destStream is the stream you want to write to
		// Can ne used with WriteToFile method.
		public static void CopyStreamContent(Stream sourceStream, Stream destStream)
		{
			sourceStream.Seek(0, SeekOrigin.Begin);
			int Length = 256;
			Byte[] buffer = new Byte[Length];
			int bytesRead = sourceStream.Read(buffer, 0, Length);
			// write the required bytes
			while (bytesRead > 0)
			{
				destStream.Write(buffer, 0, bytesRead);
				bytesRead = sourceStream.Read(buffer, 0, Length);
			}
			sourceStream.Close();
			destStream.Close();
		}

		public static void WriteToFile(Stream sourceStream, string targetFilePath, FileMode fileMode = FileMode.Create)
		{
			FileStream writeStream = new FileStream(targetFilePath, fileMode, FileAccess.Write);
			CopyStreamContent(sourceStream, writeStream);
		}

		/// <summary>
		/// Source: http://stackoverflow.com/questions/221925/creating-a-byte-array-from-a-stream
		/// </summary>
		/// <param name="input"></param>
		/// <param name="streamDoesntChange"></param>
		/// <returns></returns>
	    public static byte[] ReadAll(Stream input, bool streamDoesntChange = true)
		{
			using (input)
			{
				input.Seek(0, SeekOrigin.Begin);
				byte[] buffer = streamDoesntChange ? new byte[input.Length] : new byte[16*1024];
				using (MemoryStream ms = new MemoryStream())
				{
					int read;
					while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
					{
						ms.Write(buffer, 0, read);
					}
					return ms.ToArray();
				}
			}
		}
    }
}
