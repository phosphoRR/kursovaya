using System.Numerics;
using System.Text;

namespace FractionConverter.Server;

/// <summary>
/// Перевод числа между позиционными системами счисления с основаниями 2–16.
/// Дробные разряды выдаются методом отсечения (без округления).
/// </summary>
public static class NumberConverter
{
    private const string Digits = "0123456789ABCDEF";

    public static string Convert(string? input, int sourceBase, int targetBase, int precision)
    {
        if (sourceBase is < 2 or > 16 || targetBase is < 2 or > 16)
            throw new ArgumentException("Основания должны быть от 2 до 16.");
        if (precision is < 0 or > 10)
            throw new ArgumentException("Точность должна быть от 0 до 10.");
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Число не должно быть пустым.");

        string value = input.Trim().ToUpperInvariant();
        bool negative = value.StartsWith('-');
        if (negative) value = value[1..];

        string[] parts = value.Split('.');
        if (parts.Length > 2 || parts[0].Length == 0 ||
            (parts.Length == 2 && parts[1].Length == 0))
            throw new ArgumentException("Неверный формат числа. Используйте точку.");

        BigInteger integer = ParseDigits(parts[0], sourceBase);
        BigInteger numerator = BigInteger.Zero;
        BigInteger denominator = BigInteger.One;

        if (parts.Length == 2)
        {
            foreach (char digit in parts[1])
            {
                int d = DigitValue(digit, sourceBase);
                numerator = numerator * sourceBase + d;
                denominator *= sourceBase;
            }
        }

        string whole = FormatInteger(integer, targetBase);
        var fractional = new StringBuilder();

        for (int i = 0; i < precision && numerator != 0; i++)
        {
            numerator *= targetBase;
            BigInteger digit = BigInteger.DivRem(numerator, denominator, out numerator);
            fractional.Append(Digits[(int)digit]);
        }

        string result = fractional.Length == 0 ? whole : whole + "." + fractional;
        bool isZero = integer == 0 && fractional.ToString().Trim('0').Length == 0;
        return negative && !isZero ? "-" + result : result;
    }

    private static BigInteger ParseDigits(string text, int numberBase)
    {
        BigInteger result = BigInteger.Zero;
        foreach (char digit in text)
            result = result * numberBase + DigitValue(digit, numberBase);
        return result;
    }

    private static int DigitValue(char digit, int numberBase)
    {
        int value = Digits.IndexOf(digit);
        if (value < 0 || value >= numberBase)
            throw new ArgumentException($"Символ '{digit}' недопустим для основания {numberBase}.");
        return value;
    }

    private static string FormatInteger(BigInteger number, int numberBase)
    {
        if (number == 0) return "0";
        var result = new StringBuilder();
        while (number > 0)
        {
            number = BigInteger.DivRem(number, numberBase, out BigInteger remainder);
            result.Insert(0, Digits[(int)remainder]);
        }
        return result.ToString();
    }
}
