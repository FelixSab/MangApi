using DB.Models;
using Interface;
using Interface.Records;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DB.Commands;

internal record UpsertManganatoMangasCommand(IEnumerable<Interface.Records.ManganatoManga> Mangas) : IRequest { }
