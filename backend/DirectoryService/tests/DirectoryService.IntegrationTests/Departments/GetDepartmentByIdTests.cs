using DirectoryService.Application.Features.Departments.GetById;
using DirectoryService.Contracts.Dtos;
using DirectoryService.Domain.Identifiers;

namespace DirectoryService.IntegrationTests.Departments;

public class GetDepartmentByIdTests : DirectoryServiceBaseTests
{
    public GetDepartmentByIdTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task GetById_should_return_department()
    {
        var departmentId = DepartmentId.New();
        var locationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            dbContext.Departments.Add(DepartmentTestData.Department(departmentId, locationId));
        });

        var result = await DepartmentTestData.ExecuteAsync<GetDepartmentByIdHandler, GetDepartmentResponse>(Services, sut => sut.Handle(
            new GetDepartmentByIdQuery(departmentId.Value), CancellationToken.None));

        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.ToString() : string.Empty);
        Assert.Equal(departmentId.Value, result.Value.Id);
        Assert.Equal("Department", result.Value.Name);
        Assert.Equal("department", result.Value.Slug);
    }

    [Fact]
    public async Task GetById_should_fail_when_department_not_found()
    {
        var result = await DepartmentTestData.ExecuteAsync<GetDepartmentByIdHandler, GetDepartmentResponse>(Services, sut => sut.Handle(
            new GetDepartmentByIdQuery(Guid.NewGuid()), CancellationToken.None));

        Assert.True(result.IsFailure);
    }
}
