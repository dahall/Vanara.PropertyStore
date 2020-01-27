// This was created by converting the .json file using https://app.quicktype.io/#l=cs&r=json2csharp
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Vanara.PropertyStore
{
	internal static class JsonSettings
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			TypeNameHandling = TypeNameHandling.Auto,
			Formatting = Formatting.Indented,
			Converters =
			{
				new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
			},
		};
	}

	internal class ConcreteTypeConverter<TConcrete> : JsonConverter
	{
		public override bool CanConvert(Type objectType) =>
			//assume we can convert to anything for now
			true;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) =>
			//explicitly specify the concrete type we want to create
			serializer.Deserialize<TConcrete>(reader);

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) =>
			//use the default serialization - it works fine
			serializer.Serialize(writer, value);
	}

	internal class EnumTypeConverter<T> : JsonConverter where T : struct, Enum
	{
		public static readonly EnumTypeConverter<T> Singleton = new EnumTypeConverter<T>();

		public override bool CanConvert(Type t) => t == typeof(T);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null) return default(T);
			var value = serializer.Deserialize<string>(reader);
			if (Enum.TryParse<T>(value, true, out var enumValue))
				return enumValue;
			throw new Exception($"Cannot unmarshal type {typeof(T)}");
		}

		public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
		{
			var value = untypedValue as T?;
			if (!value.HasValue || Equals(value.Value, default(T)))
			{
				serializer.Serialize(writer, null);
				return;
			}
			if (!Enum.IsDefined(typeof(T), value.Value))
				throw new Exception($"Cannot marshal type {typeof(T)}");
			var enumString = new StringBuilder(value.Value.ToString());
			enumString[0] = char.ToLower(enumString[0]);
			serializer.Serialize(writer, enumString.ToString());
		}
	}

	internal class JsonGuidConverter : JsonConverter<Guid>
	{
		public override Guid ReadJson(JsonReader reader, Type objectType, [AllowNull] Guid existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.String)
				throw new ArgumentException();
			return new Guid(serializer.Deserialize<string>(reader));
		}

		public override void WriteJson(JsonWriter writer, [AllowNull] Guid value, JsonSerializer serializer) => serializer.Serialize(writer, value.ToString("D"));
	}

	internal class JsonSystemTypeConverter : JsonConverter<Type>
	{
		public override Type ReadJson(JsonReader reader, Type objectType, [AllowNull] Type existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			if (reader.TokenType != JsonToken.String)
				throw new ArgumentException();
			return Type.GetType(serializer.Deserialize<string>(reader));
		}

		public override void WriteJson(JsonWriter writer, [AllowNull] Type value, JsonSerializer serializer) => serializer.Serialize(writer, value.AssemblyQualifiedName);
	}
}