namespace ProjectApi.Model.Domain
{
    public static class ProjectKeyGenerator
    {
        public static string FromName(string name)
        {
            var letters =
                new string(
                    name
                        .Where(char.IsLetterOrDigit)
                        .Take(10)
                        .ToArray())
                    .ToUpperInvariant();

            if (letters.Length >= 2)
            {
                return letters;
            }

            return "ZT";
        }

        public static void Validate(string key)
        {
            if (string.IsNullOrWhiteSpace(key) ||
                key.Length < 2 ||
                key.Length > 10)
            {
                throw new ArgumentException(
                    "Project key must be 2-10 characters");
            }

            if (!key.All(char.IsLetterOrDigit))
            {
                throw new ArgumentException(
                    "Project key must be alphanumeric");
            }
        }
    }
}
