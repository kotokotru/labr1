namespace Car
{
    public static class ValidationService
    {
        public static bool IsValidName(string name) => !string.IsNullOrWhiteSpace(name);
    }
}
