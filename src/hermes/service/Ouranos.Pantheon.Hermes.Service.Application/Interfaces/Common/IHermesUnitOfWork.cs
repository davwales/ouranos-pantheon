using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Hermes.Service.Domain.Assistants;

namespace Ouranos.Pantheon.Hermes.Service.Application.Interfaces.Common;

public interface IHermesUnitOfWork : IUnitOfWork
{
    IRepository<Assistant> Assistants { get; }
}