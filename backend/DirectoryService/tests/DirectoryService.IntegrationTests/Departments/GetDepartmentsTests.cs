using DirectoryService.Application.Features.Departments.Get;
using DirectoryService.Application.Features.Departments.GetAncestors;
using DirectoryService.Application.Features.Departments.GetChildren;
using DirectoryService.Application.Features.Departments.GetTree;
using DirectoryService.Application.Features.Departments.SearchTree;
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

    [Fact]
    public async Task GetDepartments_should_fail_when_page_is_invalid()
    {
        var result = await DepartmentTestData.ExecuteAsync<GetDepartmentsHandler, PagedList<DepartmentListItemDto>>(Services, sut => sut.Handle(
            new GetDepartmentsQuery(0, 10, "name", "asc", null), CancellationToken.None));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetDepartmentsTree_should_return_only_root_departments_with_children_metadata()
    {
        var rootId = DepartmentId.New();
        var childId = DepartmentId.New();
        var secondRootId = DepartmentId.New();
        var rootLocationId = LocationId.New();
        var childLocationId = LocationId.New();
        var secondRootLocationId = LocationId.New();

        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            var root = DepartmentTestData.Department(rootId, rootLocationId, "Root", "root");
            var child = DirectoryService.Domain.Entities.Department.CreateChild(
                DirectoryService.Domain.ValueObjects.Name.Create("Child").Value,
                DirectoryService.Domain.ValueObjects.Slug.Create("child").Value,
                root,
                [DirectoryService.Domain.Entities.DepartmentLocation.Create(childId, childLocationId).Value],
                childId).Value;

            dbContext.Locations.AddRange(
                DepartmentTestData.Location(rootLocationId),
                DepartmentTestData.Location(childLocationId, "Child location"),
                DepartmentTestData.Location(secondRootLocationId, "Second root location"));
            dbContext.Departments.AddRange(root, child, DepartmentTestData.Department(secondRootId, secondRootLocationId, "Second root", "secondroot"));
        });

        var result = await DepartmentTestData.ExecuteAsync<GetDepartmentsTreeHandler, IReadOnlyList<DepartmentTreeDto>>(Services, sut => sut.Handle(
            new GetDepartmentsTreeQuery(), CancellationToken.None));

        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.ToString() : string.Empty);
        var items = result.Value;
        Assert.Equal(2, items.Count);
        var rootItem = Assert.Single(items, item => item.Id == rootId.Value);
        Assert.Equal("Root", rootItem.Name);
        Assert.Equal("root", rootItem.Slug);
        Assert.Equal("root", rootItem.Path);
        Assert.Equal(0, rootItem.Depth);
        Assert.True(rootItem.HasChildren);
        Assert.Equal(1, rootItem.ChildrenCount);
        Assert.DoesNotContain(items, item => item.Id == childId.Value);
    }

    [Fact]
    public async Task GetDepartmentChildren_should_return_only_direct_children()
    {
        var rootId = DepartmentId.New();
        var childId = DepartmentId.New();
        var grandchildId = DepartmentId.New();
        var rootLocationId = LocationId.New();
        var childLocationId = LocationId.New();
        var grandchildLocationId = LocationId.New();

        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            var root = DepartmentTestData.Department(rootId, rootLocationId, "Root", "root");
            var child = DirectoryService.Domain.Entities.Department.CreateChild(
                DirectoryService.Domain.ValueObjects.Name.Create("Child").Value,
                DirectoryService.Domain.ValueObjects.Slug.Create("child").Value,
                root,
                [DirectoryService.Domain.Entities.DepartmentLocation.Create(childId, childLocationId).Value],
                childId).Value;
            var grandchild = DirectoryService.Domain.Entities.Department.CreateChild(
                DirectoryService.Domain.ValueObjects.Name.Create("Grandchild").Value,
                DirectoryService.Domain.ValueObjects.Slug.Create("grandchild").Value,
                child,
                [DirectoryService.Domain.Entities.DepartmentLocation.Create(grandchildId, grandchildLocationId).Value],
                grandchildId).Value;

            dbContext.Locations.AddRange(
                DepartmentTestData.Location(rootLocationId),
                DepartmentTestData.Location(childLocationId, "Child location"),
                DepartmentTestData.Location(grandchildLocationId, "Grandchild location"));
            dbContext.Departments.AddRange(root, child, grandchild);
        });

        var result = await DepartmentTestData.ExecuteAsync<GetDepartmentChildrenHandler, IReadOnlyList<DepartmentTreeDto>>(Services, sut => sut.Handle(
            new GetDepartmentChildrenQuery(rootId.Value), CancellationToken.None));

        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.ToString() : string.Empty);
        var item = Assert.Single(result.Value);
        Assert.Equal(childId.Value, item.Id);
        Assert.Equal(1, item.Depth);
        Assert.True(item.HasChildren);
        Assert.Equal(1, item.ChildrenCount);
        Assert.DoesNotContain(result.Value, department => department.Id == grandchildId.Value);
    }

    [Fact]
    public async Task GetDepartmentChildren_should_return_empty_when_department_has_no_children()
    {
        var departmentId = DepartmentId.New();
        var locationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            dbContext.Departments.Add(DepartmentTestData.Department(departmentId, locationId));
        });

        var result = await DepartmentTestData.ExecuteAsync<GetDepartmentChildrenHandler, IReadOnlyList<DepartmentTreeDto>>(Services, sut => sut.Handle(
            new GetDepartmentChildrenQuery(departmentId.Value), CancellationToken.None));

        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.ToString() : string.Empty);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetDepartmentChildren_should_fail_when_department_does_not_exist()
    {
        var result = await DepartmentTestData.ExecuteAsync<GetDepartmentChildrenHandler, IReadOnlyList<DepartmentTreeDto>>(Services, sut => sut.Handle(
            new GetDepartmentChildrenQuery(Guid.NewGuid()), CancellationToken.None));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetDepartmentAncestors_should_return_ancestors_from_root_to_parent()
    {
        var rootId = DepartmentId.New();
        var parentId = DepartmentId.New();
        var departmentId = DepartmentId.New();
        var rootLocationId = LocationId.New();
        var parentLocationId = LocationId.New();
        var departmentLocationId = LocationId.New();

        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            var root = DepartmentTestData.Department(rootId, rootLocationId, "Root", "root");
            var parent = DirectoryService.Domain.Entities.Department.CreateChild(
                DirectoryService.Domain.ValueObjects.Name.Create("Parent").Value,
                DirectoryService.Domain.ValueObjects.Slug.Create("parent").Value,
                root,
                [DirectoryService.Domain.Entities.DepartmentLocation.Create(parentId, parentLocationId).Value],
                parentId).Value;
            var department = DirectoryService.Domain.Entities.Department.CreateChild(
                DirectoryService.Domain.ValueObjects.Name.Create("Department").Value,
                DirectoryService.Domain.ValueObjects.Slug.Create("department").Value,
                parent,
                [DirectoryService.Domain.Entities.DepartmentLocation.Create(departmentId, departmentLocationId).Value],
                departmentId).Value;

            dbContext.Locations.AddRange(
                DepartmentTestData.Location(rootLocationId),
                DepartmentTestData.Location(parentLocationId, "Parent location"),
                DepartmentTestData.Location(departmentLocationId, "Department location"));
            dbContext.Departments.AddRange(root, parent, department);
        });

        var result = await DepartmentTestData.ExecuteAsync<GetDepartmentAncestorsHandler, IReadOnlyList<DepartmentTreeDto>>(Services, sut => sut.Handle(
            new GetDepartmentAncestorsQuery(departmentId.Value), CancellationToken.None));

        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.ToString() : string.Empty);
        Assert.Equal([rootId.Value, parentId.Value], result.Value.Select(item => item.Id));
        Assert.DoesNotContain(result.Value, item => item.Id == departmentId.Value);
    }

    [Fact]
    public async Task GetDepartmentAncestors_should_return_empty_for_root_department()
    {
        var departmentId = DepartmentId.New();
        var locationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            dbContext.Departments.Add(DepartmentTestData.Department(departmentId, locationId));
        });

        var result = await DepartmentTestData.ExecuteAsync<GetDepartmentAncestorsHandler, IReadOnlyList<DepartmentTreeDto>>(Services, sut => sut.Handle(
            new GetDepartmentAncestorsQuery(departmentId.Value), CancellationToken.None));

        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.ToString() : string.Empty);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetDepartmentAncestors_should_fail_when_department_does_not_exist()
    {
        var result = await DepartmentTestData.ExecuteAsync<GetDepartmentAncestorsHandler, IReadOnlyList<DepartmentTreeDto>>(Services, sut => sut.Handle(
            new GetDepartmentAncestorsQuery(Guid.NewGuid()), CancellationToken.None));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task SearchDepartmentsTree_should_find_departments_without_case_sensitivity()
    {
        var rootId = DepartmentId.New();
        var locationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            dbContext.Departments.Add(DepartmentTestData.Department(rootId, locationId, "Accounting", "accounting"));
        });

        var result = await DepartmentTestData.ExecuteAsync<SearchDepartmentsTreeHandler, IReadOnlyList<DepartmentTreeDto>>(Services, sut => sut.Handle(
            new SearchDepartmentsTreeQuery("COUNT"), CancellationToken.None));

        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.ToString() : string.Empty);
        var item = Assert.Single(result.Value);
        Assert.Equal(rootId.Value, item.Id);
        Assert.Equal("accounting", item.Path);
        Assert.False(item.HasChildren);
    }

    [Fact]
    public async Task SearchDepartmentsTree_should_fail_when_query_is_shorter_than_two_characters()
    {
        var result = await DepartmentTestData.ExecuteAsync<SearchDepartmentsTreeHandler, IReadOnlyList<DepartmentTreeDto>>(Services, sut => sut.Handle(
            new SearchDepartmentsTreeQuery("a"), CancellationToken.None));

        Assert.True(result.IsFailure);
    }
}
