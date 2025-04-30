using FluentAssertions;
using GgStatAggregator.Models;
using GgStatAggregator.Models.Extensions;

namespace GgStatAggregator.Tests.Models.Extensions
{

    namespace YourProjectName.Tests.Models
    {
        public class StatSetExtensionsTests
        {
            // Helper method to create a StatSet with specified createdAt and hands
            private static StatSet CreateStatSet(DateTime createdAt, int hands) => new()
            {
                CreatedAt = createdAt,
                Hands = hands
            };

            [Fact]
            public void IsPossibleDuplicate_WhenElapsedHoursGreaterThan24_ShouldReturnFalse()
            {
                // Arrange
                var statSet = CreateStatSet(DateTime.UtcNow.AddHours(-25), hands: 100);
                int newHands = 200;

                // Act
                var result = statSet.IsPossibleDuplicate(newHands);

                // Assert
                result.Should().BeFalse();
            }

            [Fact]
            public void IsPossibleDuplicate_WhenElapsedHoursLessThan1_ShouldReturnTrue()
            {
                // Arrange
                var statSet = CreateStatSet(DateTime.UtcNow.AddMinutes(-30), hands: 100);
                int newHands = 120;

                // Act
                var result = statSet.IsPossibleDuplicate(newHands);

                // Assert
                result.Should().BeTrue();
            }

            [Fact]
            public void IsPossibleDuplicate_WhenActualHandsLessThan25_ShouldReturnTrue()
            {
                // Arrange
                var statSet = CreateStatSet(DateTime.UtcNow.AddHours(-2), hands: 100);
                int newHands = 120; // Only 20 new hands

                // Act
                var result = statSet.IsPossibleDuplicate(newHands);

                // Assert
                result.Should().BeTrue();
            }

            [Fact]
            public void IsPossibleDuplicate_WhenActualHandsWithinVarianceRange_ShouldReturnTrue()
            {
                // Arrange
                var statSet = CreateStatSet(DateTime.UtcNow.AddHours(-3), hands: 100);
                int newHands = 400; // Reasonably close based on elapsed time

                // Act
                var result = statSet.IsPossibleDuplicate(newHands);

                // Assert
                result.Should().BeTrue();
            }

            [Fact]
            public void IsPossibleDuplicate_WhenActualHandsOutsideVarianceRange_ShouldReturnFalse()
            {
                // Arrange
                var statSet = CreateStatSet(DateTime.UtcNow.AddHours(-3), hands: 100);
                int newHands = 1000; // Way too many hands for 3 hours

                // Act
                var result = statSet.IsPossibleDuplicate(newHands);

                // Assert
                result.Should().BeFalse();
            }

            [Theory]
            [InlineData(0, 0)]        // No elapsed time → 0% confidence
            [InlineData(1, 34)]       // After 1 hour, confidence ~34% (approximate)
            [InlineData(5, 75)]       // After 5 hours, capped at 75%
            [InlineData(10, 75)]      // After 10 hours, still capped at 75%
            public void CalculateConfidence_ShouldBehaveAsExpected(double elapsedHours, double expectedMaxConfidence)
            {
                // Act
                var confidence = typeof(StatSetExtensions)
                    .GetMethod("CalculateConfidence", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .Invoke(null, [elapsedHours]);

                // Assert
                // TODO: address null warning
                ((double)confidence).Should().BeLessThanOrEqualTo(expectedMaxConfidence);
            }
        }
    }

}
