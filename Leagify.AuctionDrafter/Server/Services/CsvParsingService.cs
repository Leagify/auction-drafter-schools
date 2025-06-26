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
        private readonly ILogger<CsvParsingService> _logger;

        public CsvParsingService(ILogger<CsvParsingService> logger)
        {
            _logger = logger;
        }

        public async Task<List<School>> ParseSchoolsFromCsvAsync(Stream csvStream)
        {
            var schools = new List<School>();
            int totalLinesRead = 0;
            int headerLinesSkipped = 0;
            int whitespaceLinesSkipped = 0;
            int malformedLinesSkipped = 0;
            int parsingErrorLinesSkipped = 0;
            int successfullyParsedSchools = 0;

            _logger.LogInformation("Starting CSV parsing for schools.");

            using (var reader = new StreamReader(csvStream))
            {
                string? line;
                bool isHeader = true;

                while ((line = await reader.ReadLineAsync()) != null)
                {
                    totalLinesRead++;
                    if (isHeader)
                    {
                        isHeader = false; // Skip header row
                        headerLinesSkipped++;
                        // TODO: Optionally validate header columns here
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        whitespaceLinesSkipped++;
                        continue; // Skip empty lines
                    }

                    var columns = line.Split(','); // Basic CSV split, assumes no commas within fields for now

                    if (columns.Length < 11) // Expecting at least 11 columns based on CSV structure
                    {
                        _logger.LogWarning("Skipping malformed CSV row (expected at least 11 columns, got {ActualColumns}): {RowData}", columns.Length, line);
                        malformedLinesSkipped++;
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
                        successfullyParsedSchools++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing CSV row: {RowData}", line);
                        parsingErrorLinesSkipped++;
                        // Decide whether to skip the row or throw, for now, skip.
                    }
                }
            }

            _logger.LogInformation("CSV Parsing Summary: Total Lines Read: {TotalLinesRead}, Headers Skipped: {HeaderLinesSkipped}, Whitespace Skipped: {WhitespaceLinesSkipped}, Malformed Skipped: {MalformedLinesSkipped}, Parsing Errors Skipped: {ParsingErrorLinesSkipped}, Successfully Parsed Schools: {SuccessfullyParsedSchools}",
                totalLinesRead, headerLinesSkipped, whitespaceLinesSkipped, malformedLinesSkipped, parsingErrorLinesSkipped, successfullyParsedSchools);

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
