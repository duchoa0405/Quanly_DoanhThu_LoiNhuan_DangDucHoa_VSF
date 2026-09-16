namespace FashionWeb.Business.Domain.ValueObjects;

public readonly struct Money : IEquatable<Money>
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        Amount = decimal.Round(amount, 0, MidpointRounding.AwayFromZero);
    }

    public static Money Zero => new(0);

    public static Money operator +(Money a, Money b) => new(a.Amount + b.Amount);
    public static Money operator -(Money a, Money b) => new(a.Amount - b.Amount);

    public bool Equals(Money other) => Amount == other.Amount;
    public override bool Equals(object? obj) => obj is Money other && Equals(other);
    public override int GetHashCode() => Amount.GetHashCode();
    public override string ToString() => $"{Amount:N0} ₫";
}
