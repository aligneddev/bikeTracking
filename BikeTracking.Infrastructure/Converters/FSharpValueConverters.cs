namespace BikeTracking.Infrastructure.Converters;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using static BikeTracking.Domain.FSharp.ValueObjects;

/// <summary>
/// EF Core value converters for F# discriminated unions.
/// Converts DUs to/from strings for database storage.
/// </summary>
public static class FSharpValueConverters
{
    public static class DistanceUnitConverter
    {
        public static readonly ValueConverter<DistanceUnit, string> Instance = new(
            v => v.IsKilometers ? "kilometers" : "miles",
            v =>
                v.Equals("kilometers", StringComparison.OrdinalIgnoreCase)
                    ? DistanceUnit.Kilometers
                    : DistanceUnit.Miles
        );
    }

    public static class DeletionStatusConverter
    {
        public static readonly ValueConverter<DeletionStatus, string> Instance = new(
            v => v.IsMarkedForDeletion ? "marked_for_deletion" : "active",
            v =>
                v.Equals("marked_for_deletion", StringComparison.OrdinalIgnoreCase)
                    ? DeletionStatus.MarkedForDeletion
                    : DeletionStatus.Active
        );
    }

    public static class CommunityStatusConverter
    {
        public static readonly ValueConverter<CommunityStatus, string> Instance =
            new(
                v => v.IsPrivate ? "private" : (v.IsShareable ? "shareable" : "public"),
                v =>
                    v.Equals("public", StringComparison.OrdinalIgnoreCase)
                        ? CommunityStatus.Public
                        : (
                            v.Equals("shareable", StringComparison.OrdinalIgnoreCase)
                                ? CommunityStatus.Shareable
                                : CommunityStatus.Private
                        )
            );
    }
}
