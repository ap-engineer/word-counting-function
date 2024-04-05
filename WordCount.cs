using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace WordFrequencyCounter
{
    public partial class WordCount(ILogger<WordCount> logger)
    {
        // Getting rid of the delimiters
        [GeneratedRegex(@"\W+")]
        private static partial Regex MyRegex();

        /// <summary>
        /// Counts the occurrences of each word in the given text.
        /// </summary>
        /// <param name="text">The text to count word occurrences in.</param>
        /// <returns>A dictionary where the keys are words and the values are the counts of each word.</returns>
        private static Dictionary<string, int> CountWords(string text)
        {
            var regex = MyRegex();

            var words = regex.Split(text)
                .Where(word => !string.IsNullOrEmpty(word)) // Exclude empty strings resulted from split
                .Select(word => word.ToLowerInvariant());

            return words.GroupBy(word => word)
                .ToDictionary(group => group.Key, group => group.Count());
        }

        /// <summary>
        /// Processes a single file and updates the global word counts.
        /// </summary>
        /// <param name="file">The file to process.</param>
        /// <param name="wordCounts">The global word counts to update.</param>
        private static async Task ProcessFile(IFormFile file, ConcurrentDictionary<string, int> wordCounts)
        {
            string text;

            // Let's not extend the resource lifetime
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                text = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var fileWordCounts = CountWords(text);

            foreach (var kvp in fileWordCounts)
            {
                wordCounts.AddOrUpdate(kvp.Key, kvp.Value, (_, oldValue) => oldValue + kvp.Value);
            }
        }

        [Function("WordCount")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request)
        {
            logger.LogInformation($"Processing request at {DateTime.UtcNow}");

            var files = request.Form.Files;

            if (files.Count == 0)
            {
                return new BadRequestObjectResult("Please upload a text file");
            }

            var wordCounts = new ConcurrentDictionary<string, int>();

            // Process all files concurrently
            var tasks = files.Select(file => ProcessFile(file, wordCounts));
            await Task.WhenAll(tasks);

            // Sort the final word counts by value in descending order
            var sortedWordCounts = wordCounts.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return new OkObjectResult(sortedWordCounts);
        }
    }
}