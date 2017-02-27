﻿using ApacheOrcDotNet.Compression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApacheOrcDotNet.Protocol;

namespace ApacheOrcDotNet.ColumnTypes
{
    public class StructWriter : ColumnWriter<object>
    {
		//Assume all root values are present
		public StructWriter(OrcCompressedBufferFactory bufferFactory)
			:base(bufferFactory)
		{
		}

		protected override ColumnEncodingKind DetectEncodingKind(IList<object> values)
		{
			return ColumnEncodingKind.DirectV2;
		}

		protected override void AddDataStreamBuffers(IList<OrcCompressedBuffer> buffers, ColumnEncodingKind encodingKind)
		{
		}

		protected override IStatistics CreateStatistics() => new StructWriterStatistics();

		protected override void EncodeValues(IList<object> values, IList<OrcCompressedBuffer> buffers, IStatistics statistics)
		{
			var stats = (StructWriterStatistics)statistics;
			stats.NumValues += (uint)values.Count;
		}
	}
}