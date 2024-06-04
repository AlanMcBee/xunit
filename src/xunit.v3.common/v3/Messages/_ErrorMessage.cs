using System;
using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// This message indicates that an error has occurred in the execution process.
/// </summary>
[JsonTypeID("error")]
public sealed class _ErrorMessage : _MessageSinkMessage, _IErrorMetadata, _IWritableErrorMetadata
{
	int[]? exceptionParentIndices;
	string?[]? exceptionTypes;
	string[]? messages;
	string?[]? stackTraces;

	/// <inheritdoc/>
	public int[] ExceptionParentIndices
	{
		get => this.ValidateNullablePropertyValue(exceptionParentIndices, nameof(ExceptionParentIndices));
		set => exceptionParentIndices = Guard.ArgumentNotNullOrEmpty(value, nameof(ExceptionParentIndices));
	}

	/// <inheritdoc/>
	public string?[] ExceptionTypes
	{
		get => this.ValidateNullablePropertyValue(exceptionTypes, nameof(ExceptionTypes));
		set => exceptionTypes = Guard.ArgumentNotNullOrEmpty(value, nameof(ExceptionTypes));
	}

	/// <inheritdoc/>
	public string[] Messages
	{
		get => this.ValidateNullablePropertyValue(messages, nameof(Messages));
		set => messages = Guard.ArgumentNotNullOrEmpty(value, nameof(Messages));
	}

	/// <inheritdoc/>
	public string?[] StackTraces
	{
		get => this.ValidateNullablePropertyValue(stackTraces, nameof(StackTraces));
		set => stackTraces = Guard.ArgumentNotNullOrEmpty(value, nameof(StackTraces));
	}

	/// <summary>
	/// Creates a new <see cref="_ErrorMessage"/> constructed from an <see cref="Exception"/> object.
	/// </summary>
	/// <param name="ex">The exception to use</param>
	public static _ErrorMessage FromException(Exception ex)
	{
		Guard.ArgumentNotNull(ex);

		var errorMetadata = ExceptionUtility.ExtractMetadata(ex);

		return new _ErrorMessage
		{
			ExceptionTypes = errorMetadata.ExceptionTypes,
			Messages = errorMetadata.Messages,
			StackTraces = errorMetadata.StackTraces,
			ExceptionParentIndices = errorMetadata.ExceptionParentIndices,
		};
	}

	internal override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		root.DeserializeErrorMetadata(this);
	}

	internal override void Serialize(JsonObjectSerializer serializer)
	{
		base.Serialize(serializer);

		serializer.SerializeErrorMetadata(this);
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(exceptionParentIndices, nameof(ExceptionParentIndices), invalidProperties);
		ValidatePropertyIsNotNull(exceptionTypes, nameof(ExceptionTypes), invalidProperties);
		ValidatePropertyIsNotNull(messages, nameof(Messages), invalidProperties);
		ValidatePropertyIsNotNull(stackTraces, nameof(StackTraces), invalidProperties);
	}
}
