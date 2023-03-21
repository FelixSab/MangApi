using DB.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DB.Queries;

internal record GetMangaIDsQuery : IRequest<IEnumerable<string>> { }
