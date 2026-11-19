namespace PropertyPlatform.Modules.Ledger;

/// <summary>Ledger account categories. Client money and agency money are never mixed in one entry's bank side.</summary>
public enum AccountType
{
    ClientBank,          // asset – client money held for landlords/tenants
    AgencyBank,          // asset – the agency's own money
    TenantReceivable,    // asset – rent owed by a tenant
    LandlordPayable,     // liability – money owed to a landlord
    DepositHeld,         // liability – tenancy deposits held (if not with a scheme)
    SupplierPayable,     // liability – contractor invoices
    AgencyFeeIncome,     // income – management fees / commission
}

public enum Side
{
    Debit,
    Credit,
}

/// <summary>Amounts are stored as whole pence to avoid floating-point rounding.</summary>
public sealed record JournalLine(AccountType Account, Guid? PartyId, Side Side, long AmountPence);

/// <summary>
/// An immutable, balanced double-entry journal. Never edited after posting –
/// corrections are made with <see cref="Reverse"/>.
/// </summary>
public sealed class JournalEntry
{
    private JournalEntry(Guid orgId, DateOnly date, string narrative, IReadOnlyList<JournalLine> lines, Guid? reverses)
    {
        Id = Guid.CreateVersion7();
        OrgId = orgId;
        Date = date;
        Narrative = narrative;
        Lines = lines;
        ReversesEntryId = reverses;
        PostedUtc = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; }
    public Guid OrgId { get; }
    public DateOnly Date { get; }
    public string Narrative { get; }
    public IReadOnlyList<JournalLine> Lines { get; }
    public Guid? ReversesEntryId { get; }
    public DateTimeOffset PostedUtc { get; }

    public long TotalPence => Lines.Where(l => l.Side == Side.Debit).Sum(l => l.AmountPence);

    public static JournalEntry Post(Guid orgId, DateOnly date, string narrative, IEnumerable<JournalLine> lines)
        => Create(orgId, date, narrative, lines, reverses: null);

    public JournalEntry Reverse(DateOnly date, string reason) =>
        Create(
            OrgId,
            date,
            $"Reversal of {Id}: {reason}",
            Lines.Select(l => l with { Side = l.Side == Side.Debit ? Side.Credit : Side.Debit }),
            reverses: Id);

    private static JournalEntry Create(Guid orgId, DateOnly date, string narrative, IEnumerable<JournalLine> lines, Guid? reverses)
    {
        if (orgId == Guid.Empty)
        {
            throw new LedgerException("A journal must belong to an organisation.");
        }

        if (string.IsNullOrWhiteSpace(narrative))
        {
            throw new LedgerException("A journal needs a narrative.");
        }

        var list = lines.ToList();
        if (list.Count < 2)
        {
            throw new LedgerException("A journal needs at least two lines.");
        }

        if (list.Any(l => l.AmountPence <= 0))
        {
            throw new LedgerException("Line amounts must be positive.");
        }

        var debits = list.Where(l => l.Side == Side.Debit).Sum(l => l.AmountPence);
        var credits = list.Where(l => l.Side == Side.Credit).Sum(l => l.AmountPence);
        if (debits != credits)
        {
            throw new LedgerException($"Journal does not balance: debits {debits}p, credits {credits}p.");
        }

        var touchesClient = list.Any(l => l.Account == AccountType.ClientBank);
        var touchesAgency = list.Any(l => l.Account == AccountType.AgencyBank);
        if (touchesClient && touchesAgency)
        {
            throw new LedgerException("Client money and agency money cannot move in the same journal; use a separate transfer.");
        }

        return new JournalEntry(orgId, date, narrative.Trim(), list.AsReadOnly(), reverses);
    }
}

public sealed class LedgerException(string message) : Exception(message);
