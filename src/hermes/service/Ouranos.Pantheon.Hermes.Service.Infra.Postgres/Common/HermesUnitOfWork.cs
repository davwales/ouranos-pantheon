using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Infra.Postgres.Common;
using Ouranos.Pantheon.Hermes.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Hermes.Service.Domain.Assistants;

namespace Ouranos.Pantheon.Hermes.Service.Infra.Postgres.Common;

public sealed class HermesUnitOfWork : UnitOfWork<HermesDbContext>, IHermesUnitOfWork
{
    public HermesUnitOfWork(
        HermesDbContext context,
        IServiceProvider serviceProvider
    ) : base(context, serviceProvider)
    {
        Assistants = GetRepository<Assistant>();
    }

    public IRepository<Assistant> Assistants { get; }
}