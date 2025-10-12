using CSharpFunctionalExtensions;
using Microsoft.VisualBasic;
using SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public record Address
{
    private const int MAX_LENGTH = 150;

    public string City { get; }

    public string District { get; }

    public string Street { get; }

    public string Structure { get; }

    private Address(string city, string district, string street, string structure)
    {
        City = city;
        District = district;
        Street = street;
        Structure = structure;
    }

    public static Result<Address, Error> Create(
        string city,
        string district,
        string street,
        string structure)
    {
        if (string.IsNullOrWhiteSpace(city))
            return Errors.General.ValueIsRequired("city");

        if (city.Length > MAX_LENGTH)
            return Errors.General.ValueIsInvalid("city");

        if (string.IsNullOrWhiteSpace(district))
            return Errors.General.ValueIsRequired("district");

        if (district.Length > MAX_LENGTH)
            return Errors.General.ValueIsInvalid("district");

        if (string.IsNullOrWhiteSpace(street))
            return Errors.General.ValueIsRequired("street");

        if (street.Length > MAX_LENGTH)
            return Errors.General.ValueIsInvalid("street");

        if (string.IsNullOrWhiteSpace(structure))
            return Errors.General.ValueIsRequired("structure");

        if (structure.Length > MAX_LENGTH)
            return Errors.General.ValueIsInvalid("structure");

        return new Address(city, district, street, structure);
    }
}