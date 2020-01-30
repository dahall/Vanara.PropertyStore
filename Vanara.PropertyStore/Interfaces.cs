using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

// ********* NOTE ************ / Many of these concepts come directly from the similarly named interfaces in the Windows Property System.
// Unfortunately, those don't live on other platforms and this library is intended to be multi-platform. The foundation was good and
// familiar, so I decided to not completely reinvent the wheel.
// -------------------------- */

namespace Vanara
{
	/// <summary>Control hints for drawing the property.</summary>
	public enum DrawControl
	{
		/// <summary>
		/// Default. Uses the default control, based upon the attribute. The default type is "String" (multi-value) and the default control
		/// is "MultiValueText". Any other type results in using the "StaticText" control.
		/// </summary>
		Default,

		/// <summary>Uses a check mark control.</summary>
		BooleanCheckMark,

		/// <summary>Uses a toggle control.</summary>
		BooleanToggle,

		/// <summary>An enumeration of icons.</summary>
		IconList,

		/// <summary>Uses the multi-line text control.</summary>
		MultiLineText,

		/// <summary>Uses the multi-value text control.</summary>
		MultiValueText,

		/// <summary>Uses the percent bar control.</summary>
		PercentBar,

		/// <summary>Uses the progress bar control.</summary>
		ProgressBar,

		/// <summary>Uses the 5-star rating control.</summary>
		Rating,

		/// <summary>Uses a static text control to display the property value.</summary>
		StaticText
	}

	/// <summary>Control hints for editing the property.</summary>
	public enum EditControl
	{
		/// <summary>Uses the default control, based upon the attribute.</summary>
		Default,

		/// <summary>Uses the calendar control.</summary>
		Calendar,

		/// <summary>Uses the list control with checkboxes.</summary>
		CheckBoxDropList,

		/// <summary>Uses the dropdown list control.</summary>
		DropList,

		/// <summary>An enumeration of icons.</summary>
		IconList,

		/// <summary>Uses the multi-line text control.</summary>
		MultiLineText,

		/// <summary>Uses the multi-value text control.</summary>
		MultiValueText,

		/// <summary>Uses the 5-star rating control.</summary>
		Rating,

		/// <summary>Uses the text edit control.</summary>
		Text
	}

	/// <summary>Control hints for what control to use in the header filter menu.</summary>
	public enum FilterControl
	{
		/// <summary>Uses the default control, based upon the attribute.</summary>
		Default,

		/// <summary>Uses the calendar control.</summary>
		Calendar,

		/// <summary>Uses the 5-star rating control.</summary>
		Rating
	}

	/// <summary>
	/// Describes how property values are displayed when multiple items are selected. For a particular property, PropertyAggregationType
	/// describes how the property should be displayed when multiple items that have a value for the property are selected, such as whether
	/// the values should be summed, or averaged, or just displayed with the default "Multiple Values" string.
	/// </summary>
	public enum PropertyAggregationType
	{
		/// <summary>Display the string "Multiple Values".</summary>
		Default = 0,

		/// <summary>Display the first value in the selection.</summary>
		First = 1,

		/// <summary>
		/// Display the sum of the selected values. This flag is never returned for data types <see cref="string"/>, <see cref="bool"/>,
		/// <see cref="DateTime"/>.
		/// </summary>
		Sum = 2,

		/// <summary>
		/// Display the numerical average of the selected values. This flag is never returned for data types <see cref="string"/>, <see
		/// cref="bool"/>, <see cref="DateTime"/>.
		/// </summary>
		Average = 3,

		/// <summary>
		/// Display the date range of the selected values. This flag is returned only for values of the <see cref="DateTime"/> data type.
		/// </summary>
		DateRange = 4,

		/// <summary>
		/// Display a concatenated string of all the values. The order of individual values in the string is undefined. The concatenated
		/// string omits duplicate values; if a value occurs more than once, it appears only once in the concatenated string.
		/// </summary>
		Union = 5,

		/// <summary>Display the highest of the selected values.</summary>
		Max = 6,

		/// <summary>Display the lowest of the selected values.</summary>
		Min = 7,
	}

	/// <summary>Describes how a property should be treated.</summary>
	[Flags]
	public enum PropertyColumnState
	{
		/// <summary>The value is displayed according to default settings for the column.</summary>
		Default = 0,

		/// <summary>The value is displayed as a string.</summary>
		String = 0x1,

		/// <summary>The value is displayed as an integer.</summary>
		Number = 0x2,

		/// <summary>The value is displayed as a date/time.</summary>
		DateTime = 0x3,

		/// <summary>The column should be on by default in Details view.</summary>
		OnByDefault = 0x10,

		/// <summary>Will be slow to compute. Perform on a background thread.</summary>
		Slow = 0x20,

		/// <summary>Provided by a handler, not the folder.</summary>
		Extended = 0x40,

		/// <summary>Not displayed in the context menu, but is listed in the More... dialog.</summary>
		SecondaryUI = 0x80,

		/// <summary>Not displayed in the UI.</summary>
		Hidden = 0x100,

		/// <summary>Only displayed in the UI.</summary>
		ViewOnly = 0x10000,

		/// <summary>Marks columns with values that should be read in a batch.</summary>
		BatchRead = 0x20000,

		/// <summary>Grouping is disabled for this column.</summary>
		NoGroupBy = 0x40000,

		/// <summary>Can't resize the column.</summary>
		FixedWidth = 0x1000,

		/// <summary>The width is the same in all dpi.</summary>
		NoDpiScale = 0x2000,

		/// <summary>Fixed width and height ratio.</summary>
		FixedRatio = 0x4000,
	}

	/// <summary>Indicates the display type of a property.</summary>
	public enum PropertyDisplayType
	{
		/// <summary>The value is displayed as a string.</summary>
		String = 0,

		/// <summary>The value is displayed as a number.</summary>
		Number = 1,

		/// <summary>The value is displayed as a Boolean value.</summary>
		Boolean = 2,

		/// <summary>The value is displayed as date and time.</summary>
		DateTime = 3,

		/// <summary>
		/// The value is displayed as an enumerated type-list.
		/// </summary>
		Enumerated = 4,
	}

	/// <summary>Indicates the grouping type of a property.</summary>
	public enum PropertyGroupingRange
	{
		/// <summary>Displays individual values.</summary>
		Discrete = 0,

		/// <summary>Displays static alphanumeric ranges.</summary>
		Alphanumeric = 1,

		/// <summary>Displays static size ranges.</summary>
		Size = 2,

		/// <summary>Displays dynamically created ranges.</summary>
		Dynamic = 3,

		/// <summary>Displays month and year groups.</summary>
		Date = 4,

		/// <summary>Displays percent groups.</summary>
		Percent = 5,

		/// <summary>Displays enumeration values.</summary>
		Enumerated = 6,
	}

	/// <summary>Describes the relative description type for a property description.</summary>
	public enum PropertyRelativeDescriptionType
	{
		/// <summary>General type.</summary>
		General = 0,

		/// <summary>Date type.</summary>
		Date = 1,

		/// <summary>Size type.</summary>
		Size = 2,

		/// <summary>Count type.</summary>
		Count = 3,

		/// <summary>Revision type.</summary>
		Revision = 4,

		/// <summary>Length type.</summary>
		Length = 5,

		/// <summary>Duration type.</summary>
		Duration = 6,

		/// <summary>Speed type.</summary>
		Speed = 7,

		/// <summary>Rate type.</summary>
		Rate = 8,

		/// <summary>Rating type.</summary>
		Rating = 9,

		/// <summary>Priority type.</summary>
		Priority = 10,
	}

	/// <summary>Indicate the sort types available to the user for a property.</summary>
	public enum PropertySortDescription
	{
		/// <summary>Default. "Sort going up", "Sort going down"</summary>
		General = 0,

		/// <summary>"A on top", "Z on top"</summary>
		Lexical = 1,

		/// <summary>"Lowest on top", "Highest on top"</summary>
		LowestHighest = 2,

		/// <summary>"Smallest on top", "Largest on top"</summary>
		SmallestBiggest = 3,

		/// <summary>"Oldest on top", "Newest on top"</summary>
		OldestNewest = 4,
	}

	/// <summary>These flags describe properties in property description list strings.</summary>
	[Flags]
	public enum PropertyViewFlags
	{
		/// <summary>Show this property by default.</summary>
		Default = 0x00000000,

		/// <summary>This property should be centered.</summary>
		CenterAlign = 0x00000001,

		/// <summary>This property should be right aligned.</summary>
		RightAlign = 0x00000002,

		/// <summary>Show this property as the beginning of the next collection of properties in the view.</summary>
		BeginNewGroup = 0x00000004,

		/// <summary>Fill the remainder of the view area with the content of this property.</summary>
		FillArea = 0x00000008,

		/// <summary>Sort this property in reverse (descending) order. Applies to a property in a list of sorted properties.</summary>
		SortDescending = 0x00000010,

		/// <summary>Show this property only if it is present.</summary>
		ShowOnlyIfPresent = 0x00000020,

		/// <summary>This property should be shown by default in a view (where applicable).</summary>
		ShowByDefault = 0x00000040,

		/// <summary>This property should be shown by default in the primary column selection UI.</summary>
		ShowInPrimaryList = 0x00000080,

		/// <summary>This property should be shown by default in the secondary column selection UI.</summary>
		ShowInSecondaryList = 0x00000100,

		/// <summary>Hide the label of this property if the view normally shows the label.</summary>
		HideLabel = 0x00000200,

		/// <summary>This property should not be displayed as a column in the UI.</summary>
		Hidden = 0x00000800,

		/// <summary>This property can be wrapped to the next row.</summary>
		CanWrap = 0x00001000,
	}

	/// <summary>Exposes methods for reading and writing to a stream.</summary>
	public interface IPersistAsync
	{
		/// <summary>Loads the values of the current property store from a stream.</summary>
		/// <param name="stream">The stream containing the serialized properties for this store.</param>
		Task LoadAsync(Stream stream);

		/// <summary>Persists the values of the current property store to a stream.</summary>
		/// <param name="stream">The writable stream that will recieve the serialized properties of this store.</param>
		Task PersistAsync(Stream stream);
	}

	/// <summary>Exposes methods that enumerate and retrieve individual property description details.</summary>
	public interface IPropertyDescriptor
	{
		/// <summary>Gets the case-sensitive name by which a property is known to the system, regardless of its localized name.</summary>
		/// <value>The canonical name of the property.</value>
		string CanonicalName { get; }

		/// <summary>Specifies information about how to display the property.</summary>
		IPropertyDisplayInfo DisplayInfo { get; }

		/// <summary>Specifies how the property's labels are displayed.</summary>
		IPropertyLabelInfo LabelInfo { get; }

		/// <summary>Gets the type of the property.</summary>
		/// <value>The type of the property.</value>
		Type PropertyType { get; }

		/// <summary>Gets a list of related property names. These names should be the canonical name of another property in the store.</summary>
		/// <value>The related property names.</value>
		string[] RelatedPropertyNames { get; }

		/// <summary>Specifies information about the property type.</summary>
		IPropertyTypeInfo TypeInfo { get; }
	}

	/// <summary>
	/// Represents a set of unique <see cref="IPropertyDescriptor"/> instances that can be accessed by the descriptor's canonical name.
	/// </summary>
	/// <seealso cref="System.Collections.Generic.ICollection{T}"/>
	/// <seealso cref="IPersistAsync"/>
	public interface IPropertyDescriptorSet : ICollection<IPropertyDescriptor>, IPersistAsync
	{
		/// <summary>Gets the <see cref="IPropertyDescriptor"/> with the specified name.</summary>
		/// <value>The <see cref="IPropertyDescriptor"/>.</value>
		/// <param name="name">The name.</param>
		/// <returns>The <see cref="IPropertyDescriptor"/> with the specified name.</returns>
		IPropertyDescriptor this[string name] { get; }

		/// <summary>Creates property descriptors by pulling public properties from a type.</summary>
		/// <param name="type">The type to examine for properties.</param>
		void LoadFromType(Type type);
	}

	/// <summary>Specifies information about how to display the property.</summary>
	public interface IPropertyDisplayInfo
	{
		/// <summary>A .NET format string that can be passed to the ToString method for the property type for intended formatting.</summary>
		string DisplayFormatString { get; }

		/// <summary>Specifies which control to use when simply displaying the property.</summary>
		/// <value>The draw control.</value>
		DrawControl DrawControl { get; }

		/// <summary>Specifies which control to use when editing the property.</summary>
		/// <value>The edit control.</value>
		EditControl EditControl { get; }

		/// <summary>Specifies which control to use when filtering the property.</summary>
		/// <value>The filter control.</value>
		FilterControl FilterControl { get; }

		/// <summary>Gets the current set of flags governing the property's view.</summary>
		/// <value>The view flags.</value>
		PropertyViewFlags ViewFlags { get; }
	}

	/// <summary>Specifies how the property's labels are displayed.</summary>
	public interface IPropertyLabelInfo
	{
		/// <summary>Gets a value indicating whether the label is hidden.</summary>
		bool HideLabel { get; }

		/// <summary>Gets the text used in edit controls hosted in various dialog boxes.</summary>
		/// <value>The edit invitation.</value>
		string InvitationText { get; }

		/// <summary>Gets the label as it is displayed in the UI (for example, the details column label or preview pane)..</summary>
		/// <value>The label.</value>
		string Label { get; }
	}

	/// <summary>This interface can be added to an implementer to indicate that it exposes a property store.</summary>
	public interface IPropertyProvider
	{
		/// <summary>Gets the property store.</summary>
		/// <value>The property store.</value>
		IPropertyStore PropertyStore { get; }
	}

	/// <summary>This interface exposes methods used to enumerate and manipulate property values.</summary>
	public interface IPropertyStore : IDictionary<string, object>, INotifyCollectionChanged, INotifyPropertyChanged, INotifyPropertyChanging, IPersistAsync
	{
		/// <summary>Gets or sets a value indicating whether commits are made automatically after a property is set.</summary>
		/// <value>
		/// <see langword="true"/> if commits are immediate; otherwise, <see langword="false"/> and the user must use <see cref="Commit"/>
		/// to save the changes.
		/// </value>
		bool ImmediateCommitModel { get; set; }

		/// <summary>Gets a value indicating whether properties have been changed or added, but not committed.</summary>
		/// <value><see langword="true" /> if there are properties to commit; otherwise, <see langword="false" />.</value>
		bool IsDirty { get; }

		/// <summary>Gets the set of property descriptors.</summary>
		/// <value>The property descriptors.</value>
		IPropertyDescriptorSet PropertyDescriptors { get; }

		/// <summary>After a change has been made, this method saves the changes.</summary>
		void Commit();

		/// <summary>Gets the property value.</summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>The property value.</returns>
		object GetPropertyValue([CallerMemberName] string propertyName = "", object defaultValue = default);

		/// <summary>Sets the property value.</summary>
		/// <param name="value">The value.</param>
		/// <param name="propertyName">Name of the property.</param>
		void SetPropertyValue(object value, [CallerMemberName] string propertyName = "");

	}

	/// <summary>Specifies information about the property type.</summary>
	public interface IPropertyTypeInfo
	{
		/// <summary>Gets a value that describes how the property values are displayed when multiple items are selected in the UI.</summary>
		/// <value>The type of the aggregation.</value>
		PropertyAggregationType AggregationType { get; }

		/// <summary>Gets a value indicating whether this property can be read (has a get method).</summary>
		/// <value><see langword="true"/> if this property can be read; otherwise, <see langword="false"/>.</value>
		[DefaultValue(true)]
		bool CanRead { get; }

		/// <summary>Gets a value indicating whether this property can be written (has a set method).</summary>
		/// <value><see langword="true"/> if this property can be written; otherwise, <see langword="false"/>.</value>
		[DefaultValue(true)]
		bool CanWrite { get; }

		/// <summary>Gets the grouping method to be used when a view is grouped by a property, and retrieves the grouping type.</summary>
		/// <value>The grouping range.</value>
		PropertyGroupingRange GroupingRange { get; }
	}
}