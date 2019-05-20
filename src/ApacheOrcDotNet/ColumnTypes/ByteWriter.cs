﻿using ApacheOrcDotNet.Compression;
using ApacheOrcDotNet.Encodings;
using ApacheOrcDotNet.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApacheOrcDotNet.ColumnTypes
{
	public class ByteWriter : IColumnWriter<byte?>
	{
		readonly bool _isNullable;
		readonly OrcCompressedBuffer _presentBuffer;
		readonly OrcCompressedBuffer _dataBuffer;

		public ByteWriter(bool isNullable, OrcCompressedBufferFactory bufferFactory, uint columnId)
		{
			_isNullable = isNullable;
			ColumnId = columnId;

			if (_isNullable)
			{
				_presentBuffer = bufferFactory.CreateBuffer(StreamKind.Present);
				_presentBuffer.MustBeIncluded = false;
			}
			_dataBuffer = bufferFactory.CreateBuffer(StreamKind.Data);
		}

		public List<IStatistics> Statistics { get; } = new List<IStatistics>();
		public long CompressedLength => Buffers.Sum(s => s.Length);
		public uint ColumnId { get; }
		public OrcCompressedBuffer[] Buffers => _isNullable ? new[] { _presentBuffer, _dataBuffer } : new[] { _dataBuffer };
		public ColumnEncodingKind ColumnEncoding => ColumnEncodingKind.Direct;

		public void FlushBuffers()
		{
			foreach (var buffer in Buffers)
				buffer.Flush();
		}

		public void Reset()
		{
			foreach (var buffer in Buffers)
				buffer.Reset();
			if (_isNullable)
				_presentBuffer.MustBeIncluded = false;
			Statistics.Clear();
		}

		public void AddBlock(IList<byte?> values)
		{
			var stats = new LongWriterStatistics();
			Statistics.Add(stats);
			foreach (var buffer in Buffers)
				buffer.AnnotatePosition(stats, 0);

			var valList = new List<byte>(values.Count);

			if (_isNullable)
			{
				var presentList = new List<bool>(values.Count);

				foreach (var value in values)
				{
					stats.AddValue(value);
					if (value.HasValue)
						valList.Add(value.Value);
					presentList.Add(value.HasValue);
				}

				var presentEncoder = new BitWriter(_presentBuffer);
				presentEncoder.Write(presentList);
				if (stats.HasNull)
					_presentBuffer.MustBeIncluded = true;
			}
			else
			{
				foreach (var value in values)
				{
					stats.AddValue(value);
					valList.Add(value.Value);
				}
			}

			var valEncoder = new ByteRunLengthEncodingWriter(_dataBuffer);
			valEncoder.Write(valList);
		}
	}
}