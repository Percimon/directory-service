using DirectoryService.Application.Features.Departments.Get;
using DirectoryService.Contracts.Dtos;
using DirectoryService.Contracts.Responses;
using DirectoryService.Domain.Identifiers;

namespace DirectoryService.IntegrationTests.Departments;

public class GetDepartmentsTests : DirectoryServiceBaseTests
{
    public GetDepartmentsTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task GetDepartments_should_filter_by_search()
    {
        var firstDepartmentId = DepartmentId.New();
        var secondDepartmentId = DepartmentId.New();
        var firstLocationId = LocationId.New();
        var secondLocationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.AddRange(DepartmentTestData.Location(firstLocationId), DepartmentTestData.Location(secondLocationId, "Location_2"));
            dbContext.Departments.Add(DepartmentTestData.Department(firstDepartmentId, firstLocationId, "Accounting", "accounting"));
            dbContext.Departments.Add(DepartmentTestData.Department(secondDepartmentId, secondLocationId, "Engineering", "engineering"));
        });

        var result = await DepartmentTestData.ExecuteAsync<GetDepartmentsHandler, PagedList<DepartmentListItemDto>>(Services, sut => sut.Handle(
            new GetDepartmentsQuery(1, 10, "name", "asc", "account"), CancellationToken.None));
        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.ToString() : string.Empty);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(firstDepartmentId.Value, item.Id);
        Assert.Equal(1, result.Value.TotalCount);
    }
}
