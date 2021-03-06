﻿// ReSharper disable CheckNamespace

namespace Nimbus
// ReSharper restore CheckNamespace
{
    /// <summary>
    ///     Wrapper for our dead letter queues. This interface is just a facade over a couple of queues so that they're not
    ///     all properties on the main IBus interface.
    /// </summary>
    public interface IDeadLetterQueues
    {
        IDeadLetterQueue CommandQueue { get; }
        IDeadLetterQueue RequestQueue { get; }
    }
}