using Rankora_API.Scripts.Rankora.Types;
using System;

namespace Rankora_API.Scripts.Utils
{
    /// <summary>
    /// Provides utility methods for formatting leaderboard scores
    /// based on the configured score format.
    /// </summary>
    public static class FormatScore
    {
        /// <summary>
        /// Formats a score using the default score format configured in RankoraSettings.
        /// </summary>
        /// <param name="score">The score value to format.</param>
        /// <returns>Formatted score string.</returns>
        public static string Format(double score)
        {
            return Format(score, RankoraSettings.Instance.ScoreFormat);
        }

        /// <summary>
        /// Formats a score using a specified score format.
        /// </summary>
        /// <param name="score">The score value to format.</param>
        /// <param name="format">The score format to apply.</param>
        /// <returns>Formatted score string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if an unknown format type is specified.</exception>
        public static string Format(double score, RankoraSettings.ScoreFormatType format)
        {
            switch (format)
            {
                case RankoraSettings.ScoreFormatType.Integer:
                    // Format score as integer (no decimals)
                    return ((int)score).ToString();

                case RankoraSettings.ScoreFormatType.Float:
                    // Format score as float with 2 decimal places
                    return score.ToString("F2");

                case RankoraSettings.ScoreFormatType.Time:
                    // Format score as time in MM:SS.ms format, assuming score is in seconds
                    TimeSpan timeSpan = TimeSpan.FromSeconds(score);
                    return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds / 10:D2}";

                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported score format.");
            }
        }
    }
}
