using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFF.Shared
{
	// A set of extension methods for IDataReader
	public static class DataReaderExtensions
	{
		// Enumerates through the reads in an IDataReader.
		public static IEnumerable<IDataRecord> AsEnumerable(this IDataReader reader)
		{
			while (reader.Read())
                yield return reader;
        }
	}
}
