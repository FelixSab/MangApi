using HtmlAgilityPack;
using Interface;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Crawler;

public static class Extensions
{
    internal static HtmlNode SelectSingleNodeByClass(this HtmlNode node, string className)
    {
        return node.SelectSingleNode($".//*[contains(@class, '{className}')]");
    }
    internal static HtmlNodeCollection SelectNodesByClass(this HtmlNode node, string className)
    {
        return node.SelectNodes($".//*[contains(@class, '{className}')]");
    }

    internal static int ParseShortifiedNumber(this string number)
    {
        if (number.EndsWith("K"))
        {
            float no = float.Parse(number.TrimEnd('K'), CultureInfo.InvariantCulture);
            return (int)(no * 1000);
        }

        if (number.EndsWith("M"))
        {
            float numberFloat = float.Parse(number.TrimEnd('M'), CultureInfo.InvariantCulture);
            return (int)(numberFloat * 1000000);
        }

        return int.Parse(number, CultureInfo.InvariantCulture);
    }

    public static IServiceCollection AddManganatoCrawler(this IServiceCollection services)
    {
        return services.AddTransient<IManganatoCrawler, ManganatoCrawler>();
    }
}
