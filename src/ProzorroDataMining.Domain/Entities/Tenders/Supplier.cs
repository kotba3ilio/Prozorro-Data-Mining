namespace ProzorroDataMining.Domain.Entities.Tenders;

public sealed class Supplier : Entity
{
    private Supplier()
    {
    }

    private Supplier(string name, string? identifierScheme, string? identifierId)
    {
        Name = name;
        IdentifierScheme = identifierScheme;
        IdentifierId = identifierId;
    }

    public string Name { get; private set; } = null!;

    public string? IdentifierScheme { get; private set; }

    public string? IdentifierId { get; private set; }

    public ICollection<TenderSupplier> Tenders { get; private set; } = [];

    public static Supplier Create(string name, string? identifierScheme, string? identifierId)
    {
        return new Supplier(name, identifierScheme, identifierId);
    }
}
