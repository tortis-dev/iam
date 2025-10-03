using System.Security.Cryptography;

namespace Tortis.Iam.Server.Components.Users;

public static class PasswordGenerator
{
    static readonly char[] _allowedUpperCharacters = 
        "ABCDEEFGHIJKLMNOPQRSTUVWXY"
            .ToCharArray();

    static readonly char[] _allowedLowerCharacters = 
        "abcdeefghijklmnopqrstuvwxyz"
            .ToCharArray();
    
    static readonly char[] _allowedNumericCharacters = 
        "1234567890"
            .ToCharArray();
    
    static readonly char[] _allowedSpecialCharacters = 
        "!@#$%^&*()<>,./?"
            .ToCharArray();
    
    static string Generate(int length, char[] allowedCharacters)
    {
        var data = new byte[4 * length];
        using var gen = RandomNumberGenerator.Create();
        gen.GetBytes(data);
        var customerNo = new char[length];
        for (int i = 0; i < length; i++)
        {
            var rnd = BitConverter.ToInt32(data, i * 4);
            var idx = Math.Abs(rnd % allowedCharacters.Length);

            customerNo[i] = allowedCharacters[idx];
        }

        return new string(customerNo);
    }

    public static string Generate(int length)
    {
        var parts = (int)Math.Ceiling(length / 4M);

        var unshuffled = new List<char>(length);
        unshuffled.AddRange(Generate(parts, _allowedUpperCharacters).ToCharArray());
        unshuffled.AddRange(Generate(parts, _allowedLowerCharacters).ToCharArray());
        unshuffled.AddRange(Generate(parts, _allowedNumericCharacters).ToCharArray());
        unshuffled.AddRange(Generate(parts, _allowedSpecialCharacters).ToCharArray());
        
        unshuffled.Shuffle();
        
        return new string(unshuffled.ToArray());
    }
    
    static void Shuffle<T>(this IList<T> list)
    {
        var provider = RandomNumberGenerator.Create();
        int n = list.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do provider.GetBytes(box);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}