namespace GgStatAggregator.Helpers
{
    public static class StringHelper
    {
        public static string Pluralize(string name)
        {
            if (name.EndsWith("y", StringComparison.OrdinalIgnoreCase))
                return string.Concat(name.AsSpan(0, name.Length - 1), "ies");

            if (name.EndsWith("s", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("x", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("z", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("ch", StringComparison.OrdinalIgnoreCase) ||
                name.EndsWith("sh", StringComparison.OrdinalIgnoreCase))
                return name + "es";

            return name + "s";
        }
    }
}
