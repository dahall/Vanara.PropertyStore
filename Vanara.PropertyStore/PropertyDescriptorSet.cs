using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vanara.PropertyStore
{
	/// <summary>A property descriptor set implemetnation that supports loading and persisting its values to/from a json file.</summary>
	/// <seealso cref="System.Collections.ObjectModel.KeyedCollection{TKey, TItem}"/>
	/// <seealso cref="Vanara.PropertyStore.IPropertyDescriptorSet"/>
	public class PropertyDescriptorSet : KeyedCollection<string, IPropertyDescriptor>, IPropertyDescriptorSet
	{
		/// <summary>Loads the values of the current property store from a stream.</summary>
		/// <param name="stream">The stream containing the serialized properties for this store.</param>
		public async Task LoadAsync(Stream stream) => await Task.Factory.StartNew(() =>
		{
			using var rdr = new StreamReader(stream, true);
			using var reader = new JsonTextReader(rdr);
			var serializer = JsonSerializer.Create(JsonSettings.Settings);
			var set = serializer.Deserialize<JsonPropertyDescriptorSet>(reader);
			foreach (var pd in set.PropertyDescriptors)
				Add(pd);
		});

		/// <summary>Creates property descriptors by pulling public properties from a type.</summary>
		/// <param name="type">The type to examine for properties.</param>
		public void LoadFromType(Type type)
		{
			foreach (var pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				Add(new PropertyDescriptor(pi.Name, pi.PropertyType, !pi.CanWrite));
		}

		/// <summary>Persists the values of the current property store to a stream.</summary>
		/// <param name="stream">The writable stream that will recieve the serialized properties of this store.</param>
		public async Task PersistAsync(Stream stream) => await Task.Factory.StartNew(() =>
		{
			using var wr = new StreamWriter(stream, Encoding.UTF8);
			var serializer = JsonSerializer.Create(JsonSettings.Settings);
			serializer.Serialize(wr, new JsonPropertyDescriptorSet(this));
		});

		internal bool IsValidGet(string propertyName, Type propertyType = null)
		{
			if (propertyType is null)
				return !(this[propertyName] is null);
			return this[propertyName] is IPropertyDescriptor d && d.PropertyType.Equals(propertyType);
		}

		internal bool IsValidSet(string propertyName, object value) => IsValidSet(propertyName, value?.GetType());

		internal bool IsValidSet(string propertyName, Type valueType)
		{
			var d = this[propertyName] as IPropertyDescriptor;
			var valid = !(d is null) && (d.TypeInfo?.CanWrite ?? true);
			return valid && !(valueType is null) ? d.PropertyType.Equals(valueType) : valid;
		}

		/// <summary>When implemented in a derived class, extracts the key from the specified element.</summary>
		/// <param name="item">The element from which to extract the key.</param>
		/// <returns>The key for the specified element.</returns>
		protected override string GetKeyForItem(IPropertyDescriptor item) => item.CanonicalName;

		private class JsonPropertyDescriptorSet
		{
			public JsonPropertyDescriptorSet(PropertyDescriptorSet parent = null) => PropertyDescriptors = parent?.Cast<PropertyDescriptor>().ToArray();

			[JsonProperty("propertyDescriptors", NullValueHandling = NullValueHandling.Ignore)]
			public PropertyDescriptor[] PropertyDescriptors { get; set; }
		}
	}
}