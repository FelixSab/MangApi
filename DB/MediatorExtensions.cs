using DB.Commands;
using DB.Queries;
using Interface.Records;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB;

public static class MediatorExtensions
{
    public static Task<IEnumerable<string>> GetMangaIDs(this IMediator mediator)
        => mediator.Send(new GetMangaIDsQuery());

    public static Task UpsertManganatoMangas(this IMediator mediator, IEnumerable<ManganatoManga> mangas)
        => mediator.Send(new UpsertManganatoMangasCommand(mangas));

    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        return services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MangapiContext>());
    }
}
