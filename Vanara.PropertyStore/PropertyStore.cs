﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Vanara
{
	/// <summary>A property store (set) implemetnation that supports getting and setting passthrough and staged commits.</summary>
	/// <seealso cref="IPropertyStore"/>
	public class PropertyStore : IPropertyStore
	{
		/// <summary>The set of descriptors.</summary>
		protected PropertyDescriptorSet descriptors = new PropertyDescriptorSet();

		/// <summary>Stores the names of any items that are dirty and must be set on <see cref="Commit"/>.</summary>
		protected HashSet<string> dirty = new HashSet<string>();

		/// <summary>Stores the current values of the properties.</summary>
		protected ConcurrentDictionary<string, object> properties = new ConcurrentDictionary<string, object>();

		private readonly FuncOut<string, object, bool> getFunc;
		private readonly Action<string> resetFunc;
		private readonly Action<string, object> setFunc;

		/// <summary>Initializes a new instance of the <see cref="PropertyStore"/> class.</summary>
		/// <param name="physicalGetMethod">The physical get method.</param>
		/// <param name="physicalSetMethod">The physical set method.</param>
		/// <param name="physicalResetMethod">The physical reset method.</param>
		/// <param name="passthrough">
		/// if set to <see langword="true"/>, does not perform a staged commit. In other words, all sets are immediately committed.
		/// </param>
		public PropertyStore(FuncOut<string, object, bool> physicalGetMethod, Action<string, object> physicalSetMethod, Action<string> physicalResetMethod, bool passthrough = false)
		{
			ImmediateCommitModel = passthrough;
			getFunc = physicalGetMethod;
			resetFunc = physicalResetMethod;
			setFunc = physicalSetMethod;
		}

		/// <summary>Occurs when the collection changes.</summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>Occurs when a property value changes.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>Occurs when a property value is changing.</summary>
		public event PropertyChangingEventHandler PropertyChanging;

		/// <summary>Gets or sets a value indicating whether commits are made automatically after a property is set.</summary>
		/// <value>
		/// <see langword="true"/> if commits are immediate; otherwise, <see langword="false"/> and the user must use <see cref="Commit"/>
		/// to save the changes.
		/// </value>
		public bool ImmediateCommitModel { get; set; }

		/// <summary>Gets a value indicating whether properties have been changed or added, but not committed.</summary>
		/// <value><see langword="true"/> if there are properties to commit; otherwise, <see langword="false"/>.</value>
		public virtual bool IsDirty => dirty.Count > 0;

		/// <summary>Gets or sets a value indicating whether to check for a valid descriptor before getting or setting a property value.</summary>
		/// <value><see langword="true"/> if no descriptor validation; otherwise, <see langword="false"/>.</value>
		public bool NoDescriptorValidation { get; set; } = false;

		/// <summary>Gets the set of property descriptors.</summary>
		/// <value>The property descriptors.</value>
		public virtual IPropertyDescriptorSet PropertyDescriptors => descriptors;

		/// <summary>Gets the number of elements contained in the <see cref="ICollection{T}"/>.</summary>
		public virtual int Count => properties.Count;

		/// <summary>Gets a value indicating whether the <see cref="ICollection{T}"/> is read-only.</summary>
		public virtual bool IsReadOnly => ((IDictionary<string, object>)properties).IsReadOnly;

		/// <summary>Gets an <see cref="ICollection{T}"/> containing the keys of the <see cref="IDictionary{TKey, TValue}"/>.</summary>
		public ICollection<string> Keys => properties.Keys;

		/// <summary>Gets an <see cref="ICollection{T}"/> containing the values in the <see cref="IDictionary{TKey, TValue}"/>.</summary>
		public ICollection<object> Values => properties.Values;

		/// <summary>Gets or sets the <see cref="System.Object"/> with the specified property name.</summary>
		/// <value>The <see cref="System.Object"/>.</value>
		/// <param name="propertyName">The property name.</param>
		/// <returns></returns>
		public virtual object this[string propertyName]
		{
			get => GetPropertyValue(propertyName);
			set => SetPropertyValue(value, propertyName);
		}

		/// <summary>Adds an element with the provided property name and value to the <see cref="IDictionary{TKey, TValue}"/>.</summary>
		/// <param name="propertyName">The object to use as the property name of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		public virtual void Add<T>(string propertyName, T value)
		{
			if (!NoDescriptorValidation && !descriptors.IsValidSet(propertyName, typeof(T)))
				throw new InvalidOperationException("Property is not defined.");
			AddNoChecks(propertyName, value);
		}

		/// <summary>Removes all items from the <see cref="ICollection{T}"/>.</summary>
		public virtual void Clear()
		{
			lock (properties)
			{
				var items = properties.ToList();
				foreach (var item in items)
					OnPropertyChanging(item.Key);
				properties.Clear();
				if (ImmediateCommitModel)
					foreach (var item in items)
						resetFunc?.Invoke(item.Key);
				else
					dirty.Clear();
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, items));
				foreach (var item in items)
					OnPropertyChanged(item.Key);
			}
		}

		/// <summary>After a change has been made, this method saves the changes.</summary>
		public virtual void Commit()
		{
			lock (dirty)
			{
				foreach (var propertyName in dirty)
					setFunc?.Invoke(propertyName, properties[propertyName]);
				dirty.Clear();
			}
		}

		/// <summary>Determines whether this instance contains the object.</summary>
		/// <param name="item">The object to locate in the <see cref="ICollection{T}"/>.</param>
		/// <returns>
		/// <see langword="true"/> if <paramref name="item"/> is found in the <see cref="ICollection{T}"/>; otherwise, <see langword="false"/>.
		/// </returns>
		public bool Contains(KeyValuePair<string, object> item) => properties.Contains(item);

		/// <summary>Determines whether the <see cref="IDictionary{TKey, TValue}"/> contains an element with the specified property name.</summary>
		/// <param name="propertyName">The property name to locate in the <see cref="IDictionary{TKey, TValue}"/>.</param>
		/// <returns>
		/// <see langword="true"/> if the <see cref="IDictionary{TKey, TValue}"/> contains an element with the property name; otherwise,
		/// <see langword="false"/>.
		/// </returns>
		public bool ContainsKey(string propertyName) => properties.ContainsKey(propertyName);

		/// <summary>Returns an enumerator that iterates through the collection.</summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => properties.GetEnumerator();

		/// <summary>Gets the property value.</summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>The property value.</returns>
		public object GetPropertyValue([CallerMemberName] string propertyName = "", object defaultValue = default) => TryGetValue(propertyName, out var value) ? value : defaultValue;

		/// <summary>Gets the property value.</summary>
		/// <typeparam name="T">The type of the property.</typeparam>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>The property value, cast to <typeparamref name="T"/>.</returns>
		public T GetPropertyValue<T>([CallerMemberName] string propertyName = "", T defaultValue = default) => (T)GetPropertyValue(propertyName, (object)defaultValue);

		/// <summary>Loads the values of the current property store from a stream.</summary>
		/// <param name="stream">The stream containing the serialized properties for this store.</param>
		public virtual async Task LoadAsync(Stream stream)
		{
			await Task.Factory.StartNew(() =>
			{
				var ser = new BinaryFormatter { Binder = new PSSerializationBinder() };
				var parray = (KeyValuePair<string, object>[])ser.Deserialize(stream);
				foreach (var kv in parray)
					SetPropertyValue(kv.Value, kv.Key);
			});
		}

		/// <summary>Persists the values of the current property store to a stream.</summary>
		/// <param name="stream">The writable stream that will recieve the serialized properties of this store.</param>
		public virtual async Task PersistAsync(Stream stream)
		{
			// Before we start, ensure all values can be serialized
			/*foreach (var kv in properties)
			{
				if (!(kv.Value is null) && !kv.Value.GetType().IsSerializable)
				{
					var conv = TypeDescriptor.GetConverter(kv.Value);
					if (!conv.CanConvertTo(typeof(string)) && !conv.CanConvertTo(typeof(byte[])))
						throw new InvalidOperationException($"Unable to serialize property type for {kv.Key}.");
				}
			}*/

			// Get current list of properties. For missing ones that are R/W, grab them.
			var props = properties.ToList();
			foreach (var pd in descriptors)
				if (pd.IsRW() && !props.Any(kv => kv.Key.Equals(pd.CanonicalName)) && TryGetValue(pd.CanonicalName, out var value))
					props.Add(new KeyValuePair<string, object>(pd.CanonicalName, value));

			// Write out values for binary serializer.
			await Task.Factory.StartNew(() => new BinaryFormatter { Binder = new PSSerializationBinder() }.Serialize(stream, props.ToArray()));
		}

		//{
		//	if (stream is null) throw new ArgumentNullException(nameof(stream));
		//	var json = JsonSerializer.DeserializeAsync<JsonPropertyDescriptorSet>(stream));
		//}
		/// <summary>Removes the element with the specified property name from the <see cref="IDictionary{TKey, TValue}"/>.</summary>
		/// <param name="propertyName">The property name of the element to remove.</param>
		/// <returns>
		/// <see langword="true"/> if the element is successfully removed; otherwise, <see langword="false"/>. This method also returns <see
		/// langword="false"/> if <paramref name="propertyName"/> was not found in the original <see cref="IDictionary{TKey, TValue}"/>.
		/// </returns>
		public virtual bool Remove(string propertyName)
		{
			OnPropertyChanging(propertyName);
			if (properties.TryRemove(propertyName, out var value))
			{
				if (ImmediateCommitModel)
					resetFunc?.Invoke(propertyName);
				else
					dirty.Remove(propertyName);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new KeyValuePair<string, object>(propertyName, value)));
				OnPropertyChanged(propertyName);
				return true;
			}
			return false;
		}

		/// <summary>Removes the first occurrence of a specific object from the <see cref="ICollection{T}"/>.</summary>
		/// <param name="item">The object to remove from the <see cref="ICollection{T}"/>.</param>
		/// <returns>
		/// <see langword="true"/> if <paramref name="item"/> was successfully removed from the <see cref="ICollection{T}"/>; otherwise,
		/// <see langword="false"/>. This method also returns <see langword="false"/> if <paramref name="item"/> is not found in the
		/// original <see cref="ICollection{T}"/>.
		/// </returns>
		public bool Remove(KeyValuePair<string, object> item)
		{
			if (!TryGetValue(item.Key, out var value) || !Equals(value, item.Value))
				return false;
			Remove(item.Key);
			return true;
		}

		/// <summary>Sets the property value.</summary>
		/// <param name="value">The value.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <exception cref="InvalidOperationException">Property is not defined.</exception>
		public virtual void SetPropertyValue(object value, [CallerMemberName] string propertyName = "")
		{
			// Get the current value.
			TryGetValue(propertyName, out var oldItem);

			// If they're the same, don't bother setting.
			if (Equals(oldItem, value))
				return;

			if (oldItem is null)
			{
				Add(propertyName, value);
			}
			else
			{
				if (!NoDescriptorValidation && !descriptors.IsValidSet(propertyName, value))
					throw new InvalidOperationException("Property is not defined.");
				OnPropertyChanging(propertyName);
				properties[propertyName] = value;
				if (ImmediateCommitModel)
					setFunc?.Invoke(propertyName, value);
				else
					dirty.Add(propertyName);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new KeyValuePair<string, object>(propertyName, value), oldItem));
				OnPropertyChanged(propertyName);
			}
		}

		/// <summary>Gets the value associated with the specified property name.</summary>
		/// <param name="propertyName">The property name whose value to get.</param>
		/// <param name="value">
		/// When this method returns, the value associated with the specified propertyName, if the property name is found; otherwise, the
		/// default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.
		/// </param>
		/// <returns>
		/// <see langword="true"/> if the object that implements <see cref="IDictionary{TKey, TValue}"/> contains an element with the
		/// specified property name; otherwise, <see langword="false"/>.
		/// </returns>
		public virtual bool TryGetValue(string propertyName, [MaybeNullWhen(false)] out object value)
		{
			if (!NoDescriptorValidation && !descriptors.IsValidGet(propertyName))
				throw new InvalidOperationException("Property is not defined.");

			if (!ImmediateCommitModel && properties.TryGetValue(propertyName, out value))
				return true;
			return getFunc != null ? getFunc(propertyName, out value) : throw new ArgumentNullException();
		}

		/// <summary>Adds an element with the provided property name and value to the <see cref="IDictionary{TKey, TValue}"/>.</summary>
		/// <param name="propertyName">The object to use as the property name of the element to add.</param>
		/// <param name="value">The object to use as the value of the element to add.</param>
		void IDictionary<string, object>.Add(string propertyName, object value)
		{
			if (!NoDescriptorValidation && !descriptors.IsValidSet(propertyName, value))
				throw new InvalidOperationException("Property is not defined.");
			AddNoChecks(propertyName, value);
		}

		/// <summary>Adds an item to the <see cref="ICollection{T}"/>.</summary>
		/// <param name="item">The object to add to the <see cref="ICollection{T}"/>.</param>
		void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item) => Add(item.Key, item.Value);

		/// <summary>
		/// Copies the elements of the <see cref="ICollection{T}"/> to an <see cref="T:System.Array"/>, starting at a particular <see
		/// cref="T:System.Array"/> index.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see
		/// cref="ICollection{T}"/>. The <see cref="T:System.Array"/> must have zero-based indexing.
		/// </param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => ((IDictionary<string, object>)properties).CopyTo(array, arrayIndex);

		/// <summary>Returns an enumerator that iterates through a collection.</summary>
		/// <returns>An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.</returns>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>Raises the <see cref="CollectionChanged"/> event.</summary>
		/// <param name="args">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs args) => CollectionChanged?.Invoke(this, args);

		/// <summary>Raises the <see cref="PropertyChanged"/> event.</summary>
		/// <param name="propertyName">Name of the property.</param>
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		/// <summary>Raises the <see cref="PropertyChanging"/> event.</summary>
		/// <param name="propertyName">Name of the property.</param>
		protected virtual void OnPropertyChanging([CallerMemberName] string propertyName = "") => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

		private void AddNoChecks(string propertyName, object value)
		{
			OnPropertyChanging(propertyName);
			if (properties.TryAdd(propertyName, value))
			{
				if (ImmediateCommitModel)
					setFunc?.Invoke(propertyName, value);
				else
					dirty.Add(propertyName);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new KeyValuePair<string, object>(propertyName, value)));
				OnPropertyChanged(propertyName);
			}
		}

		private class PSSerializationBinder : SerializationBinder
		{
			/// <summary>When overridden in a derived class, controls the binding of a serialized object to a type.</summary>
			/// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
			/// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
			/// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object.</param>
			public override void BindToName(Type serializedType, out string assemblyName, out string typeName) =>
				base.BindToName(serializedType, out assemblyName, out typeName);

			/// <summary>When overridden in a derived class, controls the binding of a serialized object to a type.</summary>
			/// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
			/// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object.</param>
			/// <returns>The type of the object the formatter creates a new instance of.</returns>
			public override Type BindToType(string assemblyName, string typeName) =>
				Type.GetType($"{typeName}, {assemblyName}", false, true);
		}
	}
}