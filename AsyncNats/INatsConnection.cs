﻿namespace EightyDecibel.AsyncNats
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EightyDecibel.AsyncNats.Channels;
    using EightyDecibel.AsyncNats.Messages;

    public interface INatsConnection : IAsyncDisposable
    {
        INatsOptions Options { get; }

        ValueTask ConnectAsync();
        ValueTask DisconnectAsync();

        ValueTask PublishObjectAsync<T>(string subject, T payload, string? replyTo = null);
        ValueTask PublishAsync(string subject, byte[]? payload, string? replyTo = null);
        ValueTask PublishTextAsync(string subject, string text, string? replyTo = null);
        ValueTask PublishMemoryAsync(string subject, ReadOnlyMemory<byte> payload, string? replyTo = null);

        IAsyncEnumerable<INatsServerMessage> SubscribeAll();
        ValueTask<INatsChannel> Subscribe(string subject, string? queueGroup = null);
        ValueTask<INatsChannel<T>> Subscribe<T>(string subject, string? queueGroup = null, INatsSerializer? serializer = null);
        ValueTask<INatsObjectChannel<T>> SubscribeObject<T>(string subject, string? queueGroup = null, INatsSerializer? serializer = null);
        ValueTask<INatsChannel<string>> SubscribeText(string subject, string? queueGroup = null);
        ValueTask Unsubscribe<T>(INatsObjectChannel<T> channel);
        ValueTask Unsubscribe<T>(INatsChannel<T> channel);
        ValueTask Unsubscribe(INatsChannel channel);

        Task<string> RequestText(string subject, string request, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
        Task<TResponse> RequestObject<TRequest, TResponse>(string subject, TRequest request, INatsSerializer? serializer = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    }
}