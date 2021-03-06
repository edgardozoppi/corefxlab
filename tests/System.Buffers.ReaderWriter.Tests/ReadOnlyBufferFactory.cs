// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Buffers;
using System.Buffers.Testing;
using System.Collections.Generic;
using System.Text;

namespace System.IO.Pipelines.Tests
{
    public abstract class ReadOnlyBufferFactory
    {
        public static ReadOnlyBufferFactory Array { get; } = new ArrayTestBufferFactory();
        public static ReadOnlyBufferFactory OwnedMemory { get; } = new MemoryTestBufferFactory();
        public static ReadOnlyBufferFactory SingleSegment { get; } = new SingleSegmentTestBufferFactory();
        public static ReadOnlyBufferFactory SegmentPerByte { get; } = new BytePerSegmentTestBufferFactory();

        public abstract ReadOnlySequence<byte> CreateOfSize(int size);
        public abstract ReadOnlySequence<byte> CreateWithContent(byte[] data);

        public ReadOnlySequence<byte> CreateWithContent(string data)
        {
            return CreateWithContent(Encoding.ASCII.GetBytes(data));
        }

        internal class ArrayTestBufferFactory : ReadOnlyBufferFactory
        {
            public override ReadOnlySequence<byte> CreateOfSize(int size)
            {
                return new ReadOnlySequence<byte>(new byte[size + 20], 10, size);
            }

            public override ReadOnlySequence<byte> CreateWithContent(byte[] data)
            {
                var startSegment = new byte[data.Length + 20];
                System.Array.Copy(data, 0, startSegment, 10, data.Length);
                return new ReadOnlySequence<byte>(startSegment, 10, data.Length);
            }
        }

        internal class MemoryTestBufferFactory : ReadOnlyBufferFactory
        {
            public override ReadOnlySequence<byte> CreateOfSize(int size)
            {
                return CreateWithContent(new byte[size]);
            }

            public override ReadOnlySequence<byte> CreateWithContent(byte[] data)
            {
                var startSegment = new byte[data.Length + 20];
                System.Array.Copy(data, 0, startSegment, 10, data.Length);
                return new ReadOnlySequence<byte>(new Memory<byte>(startSegment, 10, data.Length));
            }
        }

        internal class SingleSegmentTestBufferFactory : ReadOnlyBufferFactory
        {
            public override ReadOnlySequence<byte> CreateOfSize(int size)
            {
                return BufferUtilities.CreateBuffer(size);
            }

            public override ReadOnlySequence<byte> CreateWithContent(byte[] data)
            {
                return BufferUtilities.CreateBuffer(data);
            }
        }

        internal class BytePerSegmentTestBufferFactory : ReadOnlyBufferFactory
        {
            public override ReadOnlySequence<byte> CreateOfSize(int size)
            {
                return CreateWithContent(new byte[size]);
            }

            public override ReadOnlySequence<byte> CreateWithContent(byte[] data)
            {
                var segments = new List<byte[]>();

                segments.Add(System.Array.Empty<byte>());
                foreach (var b in data)
                {
                    segments.Add(new[] { b });
                    segments.Add(System.Array.Empty<byte>());
                }

                return BufferUtilities.CreateBuffer(segments.ToArray());
            }
        }
    }
}
