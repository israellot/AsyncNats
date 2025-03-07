﻿namespace EightyDecibel.AsyncNats.Messages
{
    using System;
    using System.Buffers;
    using System.Buffers.Text;
    using System.Collections.Generic;
    using System.Text;


    public class NatsHPub
    {
        private static readonly ReadOnlyMemory<byte> _command = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("HPUB "));
        private static readonly ReadOnlyMemory<byte> _del = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(" "));
        private static readonly ReadOnlyMemory<byte> _end = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("\r\n"));

        public static IMemoryOwner<byte> RentedSerialize(NatsMemoryPool pool, string subject, string? replyTo, NatsMsgHeadersPublish header, ReadOnlyMemory<byte> payload)
        {
            var totalLength = payload.Length + header.SerializedLength;
            
            var hint = _command.Length; // HPUB
            hint += subject.Length + 2; // Subject + spaces
            hint += replyTo?.Length + 1 ?? 0; // ReplyTo
            
            if (header.SerializedLength < 10) hint += 1;
            else if (header.SerializedLength < 100) hint += 2;
            else if (header.SerializedLength < 1_000) hint += 3;
            else if (header.SerializedLength < 10_000) hint += 4;
            else if (header.SerializedLength < 100_000) hint += 5;
            else if (header.SerializedLength < 1_000_000) hint += 6;
            else if (header.SerializedLength < 10_000_000) hint += 7;
            else throw new ArgumentOutOfRangeException(nameof(header));
            
            if (totalLength < 10) hint += 1;
            else if (totalLength < 100) hint += 2;
            else if (totalLength < 1_000) hint += 3;
            else if (totalLength < 10_000) hint += 4;
            else if (totalLength < 100_000) hint += 5;
            else if (totalLength < 1_000_000) hint += 6;
            else if (totalLength < 10_000_000) hint += 7;
            else throw new ArgumentOutOfRangeException(nameof(payload));
            
            

            hint += _end.Length; // Ending
            hint += payload.Length;
            hint += header.SerializedLength;
            hint += _end.Length; // Ending Payload

            var rented = pool.Rent(hint);
            var buffer = rented.Memory;

            _command.CopyTo(buffer);
            var consumed = _command.Length;
            consumed += Encoding.ASCII.GetBytes(subject, buffer.Slice(consumed).Span);
            _del.CopyTo(buffer.Slice(consumed));
            consumed++;
            if (!string.IsNullOrEmpty(replyTo))
            {
                consumed += Encoding.UTF8.GetBytes(replyTo, buffer.Slice(consumed).Span);
                _del.CopyTo(buffer.Slice(consumed));
                consumed++;
            }
            
            Utf8Formatter.TryFormat(header.SerializedLength, buffer.Slice(consumed).Span, out var written);
            consumed += written;
            _del.CopyTo(buffer.Slice(consumed));
            consumed++;

            Utf8Formatter.TryFormat(payload.Length + header.SerializedLength, buffer.Slice(consumed).Span, out written);            
            consumed += written;
            _end.CopyTo(buffer.Slice(consumed));
            consumed += _end.Length;

            header.SerializeTo(buffer.Slice(consumed).Span);
            consumed += header.SerializedLength;

            if (!payload.IsEmpty)
            {
                payload.CopyTo(buffer.Slice(consumed));
                consumed += payload.Length;
            }

            _end.CopyTo(buffer.Slice(consumed));
            return rented;
        }
    }
}