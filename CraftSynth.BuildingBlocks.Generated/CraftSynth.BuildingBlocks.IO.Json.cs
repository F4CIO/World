using CraftSynth.BuildingBlocks.Common;
using CraftSynth.BuildingBlocks.Logging;
using Newtonsoft.Json;
using CBB_ExtenderClass = CraftSynth.BuildingBlocks.Common.ExtenderClass;

namespace CraftSynth.BuildingBlocks.IO;

public class Json
{
	public static T? Deserialize<T>(string json, T nullCaseResult = default(T), T whitespaceCaseResult = default(T), bool throwExceptions = true, CustomTraceLog log = null, EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs> customHandlerForErrors = null)
	{
		var r = default(T);
		try
		{
			if(json == null)
			{
				r = nullCaseResult;
			}
			else if(json.IsNullOrWhiteSpace())
			{
				r = whitespaceCaseResult;
			}
			else
			{
				customHandlerForErrors = customHandlerForErrors ?? new EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs>((sender, args) =>
					{//if user isn't handling error use our own handler
						if(!throwExceptions)
						{
							log.AddLine($"Error deserializing property: {args.ErrorContext.Member}, Error: {args.ErrorContext.Error.Message}");
							args.ErrorContext.Handled = true;
						}
					});

				var settings = new JsonSerializerSettings
				{
					Formatting = Formatting.Indented,
					Error = customHandlerForErrors
				};

				r = JsonConvert.DeserializeObject<T>(json, settings);
			}
		}
		catch(Newtonsoft.Json.JsonException ex)
		{
			string message = "Invalid JSON format or unable to deserialize. Content: " + json.ToNonNullString("null");
			log.AddLine(message);
			if(throwExceptions)
			{
				throw new Exception(message, ex);
			}
		}
		catch(Exception ex)
		{
			string message = $"An error occurred while deserializing json. Error: {ex.Message}. Content:" + json.ToNonNullString("null");
			log.AddLine(message);
			if(throwExceptions)
			{
				throw new Exception(message, ex);
			}
		}
		return r;
	}

	public static async Task<T?> ReadFileAndDeserializeAsync<T>(string jsonFilePath, T nullCaseResult = default(T), T whitespaceCaseResult = default(T), bool throwExceptions = true, CustomTraceLog log = null, EventHandler<Newtonsoft.Json.Serialization.ErrorEventArgs> customHandlerForErrors = null)
	{
		var r = default(T);
		string json = null;
		try
		{
			json = await File.ReadAllTextAsync(jsonFilePath);
		}
		catch(Exception ex)
		{
			string message = $"Error occurred while reading file from {jsonFilePath.ToNonNullString("null")}. Error: {ex.Message}";
			log.AddLine(message);
			if(throwExceptions)
			{
				throw new Exception(message, ex);
			}
		}
		r = Deserialize<T>(json, nullCaseResult, whitespaceCaseResult, throwExceptions, log, customHandlerForErrors);
		return r;
	}


	public static string Serialize<T>(T obj)
	{
		string json = null;
		try
		{
			json = JsonConvert.SerializeObject(obj, Formatting.Indented);
		}
		catch(Newtonsoft.Json.JsonException ex)
		{
			throw new Exception("Invalid object format or unable to serialize.", ex);
		}
		catch(Exception ex)
		{
			throw new Exception($"An error occurred while serializing object. Error: {ex.Message}", ex);
		}

		return json;
	}

	public static async Task SerializeAndWriteToFile<T>(T obj, string jsonFilePath, bool removeNonUnicodeCharacters = false)
	{
		try
		{
			string json = Serialize<T>(obj);
			if(removeNonUnicodeCharacters)
			{
				json = CBB_ExtenderClass.ReplaceNonUnicodeChars(json, "", true, new char[] { '\r', '\n' });       //!!!
			}
			await File.WriteAllTextAsync(jsonFilePath, json);
		}
		catch(Exception ex)
		{
			throw new Exception($"Error occurred while writting to file {jsonFilePath.ToNonNullString("null")}. Error: {ex.Message}", ex);
		}
	}
}
