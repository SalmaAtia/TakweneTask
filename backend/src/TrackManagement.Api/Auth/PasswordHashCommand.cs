using Microsoft.AspNetCore.Identity;

namespace TrackManagement.Api.Auth;

/// <summary>
/// Dev helper: <c>dotnet run --project src/TrackManagement.Api -- --hash-password</c>
/// reads a password from stdin and prints the hash to put in DemoUser:PasswordHash.
/// Reading from stdin keeps the password out of shell history.
/// </summary>
internal static class PasswordHashCommand
{
    public const string Argument = "--hash-password";

    public static void Run()
    {
        Console.Error.Write("Password: ");
        var password = Console.ReadLine();

        if (string.IsNullOrEmpty(password))
        {
            Console.Error.WriteLine("Password must not be empty.");
            Environment.ExitCode = 1;
            return;
        }

        Console.WriteLine(new PasswordHasher<DemoUserOptions>().HashPassword(new DemoUserOptions(), password));
    }
}
