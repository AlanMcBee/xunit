using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v3;

/// <summary>
/// A base class used for TCP engines (specifically, <see cref="TcpRunnerEngine"/> and
/// <see cref="T:Xunit.Runner.v3.TcpExecutionEngine"/>).
/// </summary>
public abstract class TcpEngine : IAsyncDisposable, _IMessageSink
{
	/// <summary>
	/// Gets the operation ID that is used for broadcast messages (messages which are not associated with any specific
	/// operation ID, especially diagnostic/internal diagnostic messages).
	/// </summary>
	public const string BroadcastOperationID = "::BROADCAST::";

	readonly List<(byte[] command, Action<ReadOnlyMemory<byte>?> handler)> commandHandlers = new();
	TcpEngineState state = TcpEngineState.Unknown;

	/// <summary>
	/// Initializes a new instance of the <see cref="TcpEngine"/> class.
	/// </summary>
	/// <param name="engineID">The engine ID (used for diagnostic messages).</param>
	protected TcpEngine(string engineID)
	{
		EngineID = Guard.ArgumentNotNullOrEmpty(engineID);
		EngineDisplayName = string.Format(CultureInfo.CurrentCulture, "{0}({1})", GetType().Name, engineID);
	}

	/// <summary>
	/// Gets the disposal tracker that's automatically cleaned up during <see cref="DisposeAsync"/>.
	/// </summary>
	protected DisposalTracker DisposalTracker { get; } = new();

	/// <summary>
	/// Gets the display name for the current engine, for formatting diagnostic messages.
	/// </summary>
	protected string EngineDisplayName { get; }

	/// <summary>
	/// Gets the engine ID.
	/// </summary>
	protected string EngineID { get; }

	/// <summary>
	/// Gets the current state of the engine.
	/// </summary>
	public TcpEngineState State
	{
		get => state;
		protected set
		{
			// TODO: Should we offer an event for state changes?
			SendInternalDiagnosticMessage("{0}: [INF] Engine state transition from {1} to {2}", EngineDisplayName, state, value);
			state = value;
		}
	}

	/// <summary>
	/// An object which can be used for locks which test and change state.
	/// </summary>
	protected object StateLock { get; } = new();

	/// <summary>
	/// Adds a command handler to the engine.
	/// </summary>
	/// <param name="command">The command (in byte array form) to be handled</param>
	/// <param name="handler">The handler to be called when the command is issued</param>
	protected void AddCommandHandler(byte[] command, Action<ReadOnlyMemory<byte>?> handler) =>
		commandHandlers.Add((command, handler));

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		lock (StateLock)
		{
			if (State == TcpEngineState.Disconnecting || State == TcpEngineState.Disconnected)
				throw new ObjectDisposedException(EngineDisplayName);

			State = TcpEngineState.Disconnecting;
		}

		try
		{
			await DisposalTracker.DisposeAsync();
		}
		catch (Exception ex)
		{
			SendInternalDiagnosticMessage("{0}: [ERR] Error during disposal: {1}", EngineDisplayName, ex);
		}

		lock (StateLock)
			State = TcpEngineState.Disconnected;
	}

#pragma warning disable CA1033 // This is not intended to be part of the public contract

	// This allows this type to be used as a diagnostic message sink, which then converts the messages it receives
	// into calls to SendXxx.
	bool _IMessageSink.OnMessage(_MessageSinkMessage message)
	{
		if (message is _DiagnosticMessage diagnosticMessage)
			SendDiagnosticMessage("{0}", diagnosticMessage.Message);
		else if (message is _InternalDiagnosticMessage internalDiagnosticMessage)
			SendInternalDiagnosticMessage("{0}", internalDiagnosticMessage.Message);

		return true;
	}

#pragma warning restore CA1033

	/// <summary>
	/// Processes a request provided by the <see cref="BufferedTcpClient"/>. Dispatches to
	/// the appropriate command handler, as registered with <see cref="AddCommandHandler"/>.
	/// </summary>
	/// <param name="request">The received request.</param>
	protected void ProcessRequest(ReadOnlyMemory<byte> request)
	{
		var (command, data) = TcpEngineMessages.SplitOnSeparator(request);

		foreach (var commandHandler in commandHandlers)
			if (command.Span.SequenceEqual(commandHandler.command))
			{
				try
				{
					commandHandler.handler(data);
				}
				catch (Exception ex)
				{
					SendInternalDiagnosticMessage("{0}: [ERR] Error during message processing '{1}': {2}", EngineDisplayName, Encoding.UTF8.GetString(request.ToArray()), ex);
				}

				return;
			}

		SendInternalDiagnosticMessage("{0}: [ERR] Received unknown command '{1}'", EngineDisplayName, Encoding.UTF8.GetString(request.ToArray()));
	}

	/// <summary>
	/// Sends a diagnostic message (typically an instance of <see cref="_DiagnosticMessage"/>) to either a local listener and/or
	/// a remote-side engine.
	/// </summary>
	protected abstract void SendDiagnosticMessage(
		string format,
		params object[] args);

	/// <summary>
	/// Sends am internal diagnostic message (typically an instance of <see cref="_InternalDiagnosticMessage"/>) to either a local
	/// listener and/or a remote-side engine.
	/// </summary>
	protected abstract void SendInternalDiagnosticMessage(
		string format,
		params object[] args);
}
