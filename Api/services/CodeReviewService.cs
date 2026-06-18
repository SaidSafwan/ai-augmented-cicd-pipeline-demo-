namespace Api.services;

public class CodeReviewService
{
    public string ReviewCode(string code)
    {
        if (code.Contains("Console.WriteLine"))
            return "Consider Using a logging framework instead of console.WriteLine.";

        return "no major issue found";
    }
}
