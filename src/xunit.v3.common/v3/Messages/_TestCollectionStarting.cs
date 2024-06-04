using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message indicates that a test collection is about to start executing.
/// </summary>
[JsonTypeID("test-collection-starting")]
public class _TestCollectionStarting : _TestCollectionMessage, _ITestCollectionMetadata, _IWritableTestCollectionMetadata
{
	string? testCollectionDisplayName;

	/// <inheritdoc/>
	public string? TestCollectionClass { get; set; }

	/// <inheritdoc/>
	public string TestCollectionDisplayName
	{
		get => this.ValidateNullablePropertyValue(testCollectionDisplayName, nameof(TestCollectionDisplayName));
		set => testCollectionDisplayName = Guard.ArgumentNotNullOrEmpty(value, nameof(TestCollectionDisplayName));
	}

	internal override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		root.DeserializeTestCollectionMetadata(this);
	}

	internal override void Serialize(JsonObjectSerializer serializer)
	{
		base.Serialize(serializer);

		serializer.SerializeTestCollectionMetadata(this);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} name={1} class={2}", base.ToString(), testCollectionDisplayName.Quoted(), TestCollectionClass.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(testCollectionDisplayName, nameof(TestCollectionDisplayName), invalidProperties);
	}
}
