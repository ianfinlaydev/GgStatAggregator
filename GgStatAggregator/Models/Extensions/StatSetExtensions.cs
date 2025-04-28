namespace GgStatAggregator.Models.Extensions
{
    public static class StatSetExtensions
    {
        /// <summary>
        /// Determines whether a new StatSet could be a possible duplicate of an existing one,
        /// based on the number of hands played and time elapsed.
        /// </summary>
        /// <param name="existingStatSet">The existing stat set to compare against.</param>
        /// <param name="newHands">The number of hands in the new stat set.</param>
        /// <returns>
        /// True if the new stat set is considered a possible duplicate; otherwise, false.
        /// </returns>
        public static bool IsPossibleDuplicate(this StatSet existingStatSet, int newHands)
        {
            // Calculate the hours that have elapsed since the existing stat set was created
            var elapsedHours = (DateTime.UtcNow - existingStatSet.CreatedAt).TotalHours;

            // If the elapsed time is more than a day ago, it is unlikely to be a duplicate
            if (elapsedHours > 24)
                return false;

            // If the elapsed time is less than an hour, is is most likely a duplicate
            if (elapsedHours < 1)
                return true;

            // Calculate the number of new hands played since the last stat set
            var actualHands = newHands - existingStatSet.Hands;

            // If the actual hands played is less than 25, it is most likely the same data set
            if (actualHands < 25)
                return true;

            // Estimate the number of hands that should have been played based on elapsed time
            // Assuming ~100 hands per hour at an online poker table
            var expectedHands = elapsedHours * 100;

            // Calculate the confidence level based on how much time has passed
            var confidence = CalculateConfidence(elapsedHours);

            // Calculate an allowable variance range around the expected hands
            // Variance grows with confidence — early on (low hours), variance is wide; later it narrows
            var variance = expectedHands * confidence / 100; // Divide by 100 because confidence is a percentage (example: 63.4%)

            // Calculate lower and upper bounds for acceptable number of hands played
            var lowerBound = expectedHands - variance;
            var upperBound = expectedHands + variance;

            // If the actual number of new hands falls within the acceptable range,
            // then it is likely a duplicate
            return actualHands > lowerBound && actualHands < upperBound;
        }

        /// <summary>
        /// Calculates a confidence percentage based on elapsed hours.
        /// Confidence grows quickly at first and caps at 75% after 5 hours.
        /// Uses a logarithmic curve for natural feeling growth.
        /// </summary>
        /// <param name="elapsedHours">The number of hours elapsed (must be >= 0).</param>
        /// <returns>A confidence percentage between 0% and 75%.</returns>
        private static double CalculateConfidence(double elapsedHours)
        {
            const double maxConfidence = 75;       // Maximum confidence allowed (cap)
            const double fullConfidenceHours = 5;  // Number of hours to reach full confidence (cap)

            // If no time has passed, confidence is 0%
            if (elapsedHours <= 0)
                return 0;

            // If enough time has passed to reach full confidence, return the cap
            if (elapsedHours >= fullConfidenceHours)
                return maxConfidence;

            // Scaling factor adjusts the curve shape to hit near the maxConfidence at fullConfidenceHours
            // 0.7 is a tuning value chosen so that confidence grows fast initially but tapers off nicely
            double scalingFactor = fullConfidenceHours / 0.7;

            // Calculate confidence using a normalized logarithmic growth formula
            double confidence = Math.Log(elapsedHours + 1) / Math.Log(scalingFactor + 1) * maxConfidence;

            // Ensure we never exceed maxConfidence (even with rounding errors)
            return Math.Min(confidence, maxConfidence);
        }
    }
}
