using System;

namespace Xunit.Runner.v3;

/// <summary>
/// Byte sequences which represent commands issued between the runner and execution engines.
/// </summary>
public static class TcpEngineMessages
{
	/// <summary>
	/// Delineates the end of a message from one engine to the other. Guaranteed to always
	/// be a single byte.
	/// </summary>
	public static readonly byte[] EndOfMessage = "\n"u8.ToArray();

	/// <summary>
	/// Delineates the separator between command and data, or elements of data. Guaranteed to
	/// always be a single byte.
	/// </summary>
	public static readonly byte[] Separator = " "u8.ToArray();

	/// <summary>
	/// Splits an instance of <see cref="ReadOnlyMemory{T}"/> looking for a separator.
	/// </summary>
	/// <param name="memory">The memory to split</param>
	/// <returns>The value to the left of the separator, and the rest. If the separator
	/// was not found, then value will contain the entire memory block, and rest will
	/// be <c>null</c>.</returns>
	public static (ReadOnlyMemory<byte> value, ReadOnlyMemory<byte>? rest) SplitOnSeparator(ReadOnlyMemory<byte> memory)
	{
		var separatorIndex = memory.Span.IndexOf(Separator[0]);

		if (separatorIndex < 0)
			return (memory, null);
		else
			return (memory.Slice(0, separatorIndex), memory.Slice(separatorIndex + 1));
	}

	/// <summary>
	/// Byte sequences which represent commands issued from the execution engine.
	/// </summary>
	public static class Execution
	{
		/// <summary>
		/// Send to respond to an <see cref="Runner.Info"/> query from the runner. The data for the response is JSON-encoded
		/// message data, typically generated from <see cref="TcpExecutionEngineInfo"/>.
		/// First supported protocol: <see cref="TcpEngineProtocolVersion.v1_0"/>.
		/// </summary>
		public static readonly byte[] Info = "INFO"u8.ToArray();

		/// <summary>
		/// Send to indicate that this is a message intended as a result of an operation (typically a find or run operation).
		/// The data for the message is the operation ID, followed by <see cref="Separator"/>, followed by the
		/// JSON-encoded message data.
		/// First supported protocol: <see cref="TcpEngineProtocolVersion.v1_0"/>.
		/// </summary>
		public static readonly byte[] Message = "MSG"u8.ToArray();
	}

	/// <summary>
	/// Byte sequences which represent commands issued from the runner engine.
	/// </summary>
	public static class Runner
	{
		/// <summary>
		/// Send to indicate that a test run should be canceled. The data is the operation ID.
		/// First supported protocol: <see cref="TcpEngineProtocolVersion.v1_0"/>.
		/// </summary>
		public static readonly byte[] Cancel = "CANCEL"u8.ToArray();

		/// <summary>
		/// Send to indicate that the specified tests should be run. The data is the operation ID, followed by an optional
		/// specification of the tests to find (if there is no specification, finds all tests).
		/// First supported protocol: <see cref="TcpEngineProtocolVersion.v1_0"/>.
		/// </summary>
		public static readonly byte[] Find = "FIND"u8.ToArray();

		/// <summary>
		/// Send to query information about the test framework and test assembly.
		/// First supported protocol: <see cref="TcpEngineProtocolVersion.v1_0"/>.
		/// </summary>
		public static readonly byte[] Info = "INFO"u8.ToArray();

		/// <summary>
		/// Send to indicate that the engine should gracefully shut down, after ensuring that all pending response messages
		/// are sent. There is no data for this message.
		/// First supported protocol: <see cref="TcpEngineProtocolVersion.v1_0"/>.
		/// </summary>
		public static readonly byte[] Quit = "QUIT"u8.ToArray();

		/// <summary>
		/// Send to indicate that the specified tests should be run. The data is the operation ID, followed by an optional
		/// specification of the tests to run (if there is no specification, runs all tests).
		/// First supported protocol: <see cref="TcpEngineProtocolVersion.v1_0"/>.
		/// </summary>
		public static readonly byte[] Run = "RUN"u8.ToArray();
	}
}
