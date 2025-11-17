namespace Infernity.Tools.SourceGenerators.Output;

public static class StringExtensions
{
    public static string Capitalize(this string input)
    {
        return input[0].ToString().ToUpper() + input.Substring(1);
    }   
}