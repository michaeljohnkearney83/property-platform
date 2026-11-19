using PropertyPlatform.Modules.Ledger;

namespace PropertyPlatform.Tests;

public class LedgerTests
{
    private static readonly Guid Org = Guid.NewGuid();
    private static readonly DateOnly Today = new(2026, 11, 19);

    [Fact]
    public void Rent_receipt_balances_and_posts()
    {
        var entry = JournalEntry.Post(Org, Today, "Rent received Nov", [
            new(AccountType.ClientBank, null, Side.Debit, 120_000),
            new(AccountType.TenantReceivable, Guid.NewGuid(), Side.Credit, 120_000),
        ]);

        Assert.Equal(120_000, entry.TotalPence);
        Assert.Equal(2, entry.Lines.Count);
    }

    [Fact]
    public void Unbalanced_journal_is_rejected()
    {
        var ex = Assert.Throws<LedgerException>(() => JournalEntry.Post(Org, Today, "Bad", [
            new(AccountType.ClientBank, null, Side.Debit, 100),
            new(AccountType.TenantReceivable, null, Side.Credit, 99),
        ]));
        Assert.Contains("does not balance", ex.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Non_positive_amounts_are_rejected(long amount) =>
        Assert.Throws<LedgerException>(() => JournalEntry.Post(Org, Today, "Bad", [
            new(AccountType.ClientBank, null, Side.Debit, amount),
            new(AccountType.TenantReceivable, null, Side.Credit, amount),
        ]));

    [Fact]
    public void Client_and_agency_money_cannot_mix()
    {
        Assert.Throws<LedgerException>(() => JournalEntry.Post(Org, Today, "Mixed", [
            new(AccountType.ClientBank, null, Side.Debit, 100),
            new(AccountType.AgencyBank, null, Side.Credit, 100),
        ]));
    }

    [Fact]
    public void Reversal_swaps_sides_and_links_original()
    {
        var original = JournalEntry.Post(Org, Today, "Fee", [
            new(AccountType.LandlordPayable, null, Side.Debit, 12_000),
            new(AccountType.AgencyFeeIncome, null, Side.Credit, 12_000),
        ]);

        var reversal = original.Reverse(Today, "charged in error");

        Assert.Equal(original.Id, reversal.ReversesEntryId);
        Assert.Equal(Side.Credit, reversal.Lines[0].Side);
        Assert.Equal(Side.Debit, reversal.Lines[1].Side);
        Assert.Equal(original.TotalPence, reversal.TotalPence);
    }
}
