using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// Base message for all messages related to test methods.
/// </summary>
public abstract class _TestMethodMessage : _TestClassMessage
{
	/// <summary>
	/// Gets the test method's unique ID. Can be used to correlate test messages with the appropriate
	/// test method that they're related to. Test method metadata (as <see cref="_ITestMethodMetadata"/>)
	/// is provided via <see cref="_TestMethodStarting"/> (during execution) and should be cached as needed.
	/// Might be <c>null</c> if the test does not belong to a method.
	/// </summary>
	public string? TestMethodUniqueID { get; set; }

	internal override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		TestMethodUniqueID = TryGetString(root, nameof(TestMethodUniqueID));
	}

	internal override void Serialize(JsonObjectSerializer serializer)
	{
		base.Serialize(serializer);

		serializer.Serialize(nameof(TestMethodUniqueID), TestMethodUniqueID);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0}({1})", GetType().Name, TestMethodUniqueID.Quoted());
}

