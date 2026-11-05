namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.ShoppingLists;

/// <summary>A convertible unit; <see cref="BaseFactor"/> is millilitres (volume) or grams (mass) per unit.</summary>
public sealed record MeasurementUnit(string Symbol, UnitDimension Dimension, decimal BaseFactor);
