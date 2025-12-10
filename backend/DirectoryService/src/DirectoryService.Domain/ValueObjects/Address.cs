using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.ValueObjects;

public record Address
{

    public string City { get; }

    public string District { get; }

    public string Street { get; }

    public string Structure { get; }

    private Address(
        string city,
        string district,
        string street,
        string structure)
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
            return GeneralErrors.ValueIsRequired("city");

        if (city.Length > Constants.TextLength.LENGTH_150)
            return GeneralErrors.ValueIsInvalid("city");

        if (string.IsNullOrWhiteSpace(district))
            return GeneralErrors.ValueIsRequired("district");

        if (district.Length > Constants.TextLength.LENGTH_150)
            return GeneralErrors.ValueIsInvalid("district");

        if (string.IsNullOrWhiteSpace(street))
            return GeneralErrors.ValueIsRequired("street");

        if (street.Length > Constants.TextLength.LENGTH_150)
            return GeneralErrors.ValueIsInvalid("street");

        if (string.IsNullOrWhiteSpace(structure))
            return GeneralErrors.ValueIsRequired("structure");

        if (structure.Length > Constants.TextLength.LENGTH_150)
            return GeneralErrors.ValueIsInvalid("structure");

        return new Address(city, district, street, structure);
    }
}