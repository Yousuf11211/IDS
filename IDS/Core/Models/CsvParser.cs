using IDS.Core.Models;
using System.Collections.Generic;
using System.IO;
using System;

namespace IDS.Core.Engine
{
    public class CsvParser
    {
        // NOTE: Customize this method to match your CSV file's column headers and data types.
        public IEnumerable<PacketData> ParseCsv(Stream csvStream)
        {
            using (var reader = new StreamReader(csvStream))
            {
                // Skip header row
                if (!reader.EndOfStream)
                {
                    reader.ReadLine();
                }

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var values = line.Split(',');

                    if (values.Length < 2) continue; // Safety check

                    yield return new PacketData
                    {
                        // IMPORTANT: Replace the indices (0, 1) to match the columns 
                        // needed by your ML models (Feature1, Feature2, etc.)
                        Feature1 = float.Parse(values[0].Trim()),
                        Feature2 = float.Parse(values[1].Trim()),
                        // Add more features as needed for your model input
                    };
                }
            }
        }
    }
}