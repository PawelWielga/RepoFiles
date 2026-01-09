using System;

namespace DIHOR.RepoFiles.Utils;

public static class Guard
{
    public static void NotNull(object? value, string name)
    {
        if (value is null)
        {
            throw new ArgumentNullException(name);
        }
    }

    public static void NotNullOrWhiteSpace(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", name);
        }
    }
}
