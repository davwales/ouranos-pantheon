using MediatR;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application.Commands.Messages.ProcessMessage;

public sealed record ProcessMessageInput(byte[] Message) : IRequest;