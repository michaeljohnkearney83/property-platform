namespace PropertyPlatform.SharedKernel.Compliance;

/// <summary>What kind of personal data a property holds. Drives logging, export and erasure rules.</summary>
public enum PersonalDataCategory
{
    /// <summary>Names, email addresses, phone numbers.</summary>
    Contact,

    /// <summary>Home or property addresses that identify where someone lives.</summary>
    Address,

    /// <summary>Dates of birth, ID document numbers, Right to Rent share codes.</summary>
    Identity,

    /// <summary>Bank details, rent arrears, payment history tied to a person.</summary>
    Financial,

    /// <summary>UK GDPR special category data (health, ethnicity, etc.). Needs a DPIA before it is stored.</summary>
    SpecialCategory,
}

/// <summary>
/// Marks a property that holds personal data under UK GDPR (see docs/compliance/data-protection.md).
/// Annotated properties must never be logged, must be included in subject access exports,
/// and must be handled by erasure. <c>ComplianceArchitectureTests</c> fails the build when a
/// property whose name looks like personal data is not annotated.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class PersonalDataAttribute(PersonalDataCategory category) : Attribute
{
    public PersonalDataCategory Category { get; } = category;
}

/// <summary>
/// Explicitly marks a property whose name looks like personal data but is not
/// (for example a company's registered email). The reason is required and reviewed in the PR.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class NotPersonalDataAttribute(string reason) : Attribute
{
    public string Reason { get; } = reason;
}
