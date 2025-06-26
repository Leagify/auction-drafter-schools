using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Leagify.AuctionDrafter.Shared.Models;

namespace Leagify.AuctionDrafter.Server.Services
{
    public interface ICsvParsingService
    {
        Task<List<School>> ParseSchoolsFromCsvAsync(Stream csvStream);
    }

    public class CsvParsingService : ICsvParsingService
    {
        public async Task<List<School>> ParseSchoolsFromCsvAsync(Stream csvStream)
        {
            var schools = new List<School>();
            // Ensure stream is at the beginning if it's seekable, though typically for uploads it won't be.
            // if (csvStream.CanSeek)
            // {
            //    csvStream.Position = 0;
            // }

            using (var reader = new StreamReader(csvStream))
            {
                string? line;
                bool isHeader = true;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (isHeader)
                    {
                        isHeader = false; // Skip header row
                        // TODO: Optionally validate header columns here
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue; // Skip empty lines
                    }

                    var columns = line.Split(','); // Basic CSV split, assumes no commas within fields for now

                    if (columns.Length < 11) // Expecting at least 11 columns based on CSV structure
                    {
                        // Log or handle malformed row
                        Console.WriteLine($"Skipping malformed CSV row: {line}");
                        continue;
                    }

                    try
                    {
                        var school = new School
                        {
                            Name = columns[0].Trim(),
                            Conference = columns[1].Trim(),
                            ProjectedPoints = TryParseNullableDouble(columns[2]),
                            NumberOfProspects = TryParseNullableInt(columns[3]),
                            SchoolURL = columns[4].Trim(),
                            SuggestedAuctionValue = TryParseNullableDouble(columns[5]),
                            LeagifyPosition = columns[6].Trim(),
                            ProjectedPointsAboveAverage = TryParseNullableDouble(columns[7]),
                            ProjectedPointsAboveReplacement = TryParseNullableDouble(columns[8]),
                            AveragePointsForPosition = TryParseNullableDouble(columns[9]),
                            ReplacementValueAverageForPosition = TryParseNullableDouble(columns[10])
                        };
                        schools.Add(school);
                    }
                    catch (Exception ex)
                    {
                        // Log or handle parsing error for a specific row
                        Console.WriteLine($"Error parsing row: {line}. Error: {ex.Message}");
                        // Decide whether to skip the row or throw, for now, skip.
                    }
                }
            }
            return schools;
        }

        private double? TryParseNullableDouble(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            return null; // Or throw, or log error
        }

        private int? TryParseNullableInt(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
            {
                return result;
            }
            return null; // Or throw, or log error
        }
    }
}
