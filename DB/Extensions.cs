using Microsoft.Extensions.DependencyInjection;

namespace DB;

public static class Extensions
{
    public static IServiceCollection AddMangapiContext(this IServiceCollection services)
    {
        using var context = new MangapiContext();
        context.Database.EnsureCreated();

        services.AddDbContext<MangapiContext>();
        return services;
    }

    public async static Task<IEnumerable<TOut>> SelectSequentially<TIn, TOut>(this IEnumerable<TIn> inEnum, Func<TIn, Task<TOut>> selector)
    {
        var outList = new List<TOut>();
        foreach (var item in inEnum)
        {
            outList.Add(await selector(item));
        }

        return outList;
    }

    public async static Task SelectSequentially<TIn>(this IEnumerable<TIn> inEnum, Func<TIn, Task> selector)
    {
        foreach (var item in inEnum)
        {
            await selector(item);
        }
    }
}
