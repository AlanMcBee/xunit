using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message indicates that a line of output was provided for a test.
/// </summary>
[JsonTypeID("test-output")]
public class _TestOutput : _TestMessage
{
	string? output;

	/// <summary>
	/// Gets or sets the line of output.
	/// </summary>
	public string Output
	{
		get => this.ValidateNullablePropertyValue(output, nameof(Output));
		set => output = Guard.ArgumentNotNull(value, nameof(Output));
	}

	internal override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		output = TryGetString(root, nameof(Output));
	}

	internal override void Serialize(JsonObjectSerializer serializer)
	{
		base.Serialize(serializer);

		serializer.Serialize(nameof(Output), Output);
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(output, nameof(Output), invalidProperties);
	}
}
