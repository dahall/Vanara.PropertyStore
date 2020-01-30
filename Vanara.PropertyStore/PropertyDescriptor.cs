using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace Vanara
{
	/// <summary>Class that holds individual property description details.</summary>
	public class PropertyDescriptor : IPropertyDescriptor
	{
		/// <summary>Initializes a new instance of the <see cref="PropertyDescriptor"/> class.</summary>
		/// <param name="canonicalName">Canonical name of the property.</param>
		/// <param name="propertyType">Type of the property.</param>
		/// <param name="readOnly">if set to <see langword="true"/>, the property is read only.</param>
		public PropertyDescriptor(string canonicalName, Type propertyType, bool readOnly)
		{
			CanonicalName = canonicalName;
			PropertyType = propertyType;
			if (readOnly) TypeInfo = new PropertyTypeInfo { CanWrite = !readOnly };
		}

		/// <summary>Gets the case-sensitive name by which a property is known to the system, regardless of its localized name.</summary>
		/// <value>The canonical name of the property.</value>
		[JsonProperty("name", Required = Required.Always)]
		public string CanonicalName { get; internal set; }

		/// <summary>Specifies information about how to display the property.</summary>
		[JsonProperty("displayInfo", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(ConcreteTypeConverter<PropertyDisplayInfo>))]
		[DefaultValue(null)]
		public IPropertyDisplayInfo DisplayInfo { get; internal set; }

		/// <summary>Specifies how the property's labels are displayed.</summary>
		[JsonProperty("labelInfo", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(ConcreteTypeConverter<PropertyLabelInfo>))]
		[DefaultValue(null)]
		public IPropertyLabelInfo LabelInfo { get; internal set; }

		/// <summary>Gets the type of the property.</summary>
		/// <value>The type of the property.</value>
		[JsonProperty("type", Required = Required.Always), JsonConverter(typeof(JsonSystemTypeConverter))]
		public Type PropertyType { get; internal set; }

		/// <summary>Gets or sets the related property names.</summary>
		/// <value>The related property names.</value>
		[JsonProperty("relatedPropertyNames", NullValueHandling = NullValueHandling.Ignore)]
		[DefaultValue(null)]
		public string[] RelatedPropertyNames { get; internal set; }

		/// <summary>Specifies information about the property type.</summary>
		[JsonProperty("typeInfo", NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(ConcreteTypeConverter<PropertyTypeInfo>))]
		[DefaultValue(null)]
		public IPropertyTypeInfo TypeInfo { get; internal set; }

		internal class PropertyDisplayInfo : IPropertyDisplayInfo
		{
			/// <summary>A .NET format string that can be passed to the ToString method for the property type for intended formatting.</summary>
			[JsonProperty("displayFormatString", NullValueHandling = NullValueHandling.Ignore)]
			public string DisplayFormatString { get; internal set; }

			[JsonProperty("drawControl", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
			[JsonConverter(typeof(EnumTypeConverter<DrawControl>))]
			public DrawControl DrawControl { get; internal set; }

			[JsonProperty("editControl", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
			[JsonConverter(typeof(EnumTypeConverter<EditControl>))]
			public EditControl EditControl { get; internal set; }

			[JsonProperty("filterControl", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
			[JsonConverter(typeof(EnumTypeConverter<FilterControl>))]
			public FilterControl FilterControl { get; internal set; }

			[JsonProperty("viewFlags", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
			[JsonConverter(typeof(EnumTypeConverter<PropertyViewFlags>))]
			public PropertyViewFlags ViewFlags { get; internal set; }
		}

		internal class PropertyLabelInfo : IPropertyLabelInfo
		{
			[JsonProperty("hideLabel", NullValueHandling = NullValueHandling.Ignore)]
			public bool HideLabel { get; internal set; }

			[JsonProperty("invitationText", NullValueHandling = NullValueHandling.Ignore)]
			public string InvitationText { get; internal set; }

			[JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
			public string Label { get; internal set; }
		}

		internal class PropertyTypeInfo : IPropertyTypeInfo
		{
			[JsonProperty("aggregationType", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
			[JsonConverter(typeof(EnumTypeConverter<PropertyAggregationType>))]
			public PropertyAggregationType AggregationType { get; internal set; }

			[JsonProperty("canRead", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
			[DefaultValue(true)]
			public bool CanRead { get; internal set; } = true;

			[JsonProperty("canWrite", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
			[DefaultValue(true)]
			public bool CanWrite { get; internal set; } = true;

			[JsonProperty("groupingRange", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
			[JsonConverter(typeof(EnumTypeConverter<PropertyGroupingRange>))]
			public PropertyGroupingRange GroupingRange { get; internal set; }
		}
	}

	internal static class PDExt
	{
		public static bool IsRW(this IPropertyDescriptor pd) => (pd.TypeInfo?.CanRead).GetValueOrDefault(true) && (pd.TypeInfo?.CanWrite).GetValueOrDefault(true);
	}
}