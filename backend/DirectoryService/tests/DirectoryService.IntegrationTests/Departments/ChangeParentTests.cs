using DirectoryService.Application.Departments.ChangeParent;
using DirectoryService.Contracts.Dtos;
using DirectoryService.Domain.Entities;
using DirectoryService.Domain.Identifiers;
using DirectoryService.Domain.ValueObjects;

namespace DirectoryService.IntegrationTests.Departments;

public class ChangeParentTests : DirectoryServiceBaseTests
{
    public ChangeParentTests(DirectoryTestWebFactory factory) : base(factory) { }

    [Fact]
    public async Task ChangeParent_should_update_department_parent_and_path()
    {
        var parentId = DepartmentId.New();
        var newParentId = DepartmentId.New();
        var childId = DepartmentId.New();
        var locationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            var parent = DepartmentTestData.Department(parentId, locationId, "Parent", "parent");
            var newParent = DepartmentTestData.Department(newParentId, locationId, "New parent", "newparent");
            var child = Department.CreateChild(Name.Create("Child").Value, Slug.Create("child").Value, parent,
                [DepartmentLocation.Create(childId, locationId).Value], childId).Value;
            dbContext.Departments.AddRange(parent, newParent, child);
        });

        var result = await DepartmentTestData.ExecuteAsync<ChangeParentHandler, ChangeParentResponseDto>(Services, sut => sut.Handle(
            new ChangeParentCommand(childId.Value, newParentId.Value), CancellationToken.None));
        var child = await DepartmentTestData.FindAsync(Services, childId);

        Assert.True(result.IsSuccess);
        Assert.Equal(childId.Value, result.Value.Id);
        Assert.Equal(newParentId.Value, result.Value.ParentId);
        Assert.Equal("newparent.child", result.Value.Path);
        Assert.Equal(newParentId, child.Parent.Id);
        Assert.Equal("newparent.child", child.Path.Value);
    }

    [Fact]
    public async Task ChangeParent_should_update_paths_and_depths_for_all_descendants()
    {
        var oldParentId = DepartmentId.New();
        var newParentId = DepartmentId.New();
        var departmentId = DepartmentId.New();
        var descendantId = DepartmentId.New();
        var locationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            var oldParent = DepartmentTestData.Department(oldParentId, locationId, "Old parent", "oldparent");
            var newParent = DepartmentTestData.Department(newParentId, locationId, "New parent", "newparent");
            var department = Department.CreateChild(
                Name.Create("Department").Value,
                Slug.Create("department").Value,
                oldParent,
                [DepartmentLocation.Create(departmentId, locationId).Value],
                departmentId).Value;
            var descendant = Department.CreateChild(
                Name.Create("Descendant").Value,
                Slug.Create("descendant").Value,
                department,
                [DepartmentLocation.Create(descendantId, locationId).Value],
                descendantId).Value;
            dbContext.Departments.AddRange(oldParent, newParent, department, descendant);
        });

        var result = await DepartmentTestData.ExecuteAsync<ChangeParentHandler, ChangeParentResponseDto>(Services, sut => sut.Handle(
            new ChangeParentCommand(departmentId.Value, newParentId.Value), CancellationToken.None));
        var department = await DepartmentTestData.FindAsync(Services, departmentId);
        var descendant = await DepartmentTestData.FindAsync(Services, descendantId);

        Assert.True(result.IsSuccess);
        Assert.Equal("newparent.department", department.Path.Value);
        Assert.Equal(1, department.Depth.Value);
        Assert.Equal("newparent.department.descendant", descendant.Path.Value);
        Assert.Equal(2, descendant.Depth.Value);
    }

    [Fact]
    public async Task ChangeParent_should_move_subtree_to_root()
    {
        var parentId = DepartmentId.New();
        var departmentId = DepartmentId.New();
        var descendantId = DepartmentId.New();
        var locationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            var parent = DepartmentTestData.Department(parentId, locationId, "Parent", "parent");
            var department = Department.CreateChild(
                Name.Create("Department").Value,
                Slug.Create("department").Value,
                parent,
                [DepartmentLocation.Create(departmentId, locationId).Value],
                departmentId).Value;
            var descendant = Department.CreateChild(
                Name.Create("Descendant").Value,
                Slug.Create("descendant").Value,
                department,
                [DepartmentLocation.Create(descendantId, locationId).Value],
                descendantId).Value;
            dbContext.Departments.AddRange(parent, department, descendant);
        });

        var result = await DepartmentTestData.ExecuteAsync<ChangeParentHandler, ChangeParentResponseDto>(Services, sut => sut.Handle(
            new ChangeParentCommand(departmentId.Value, null), CancellationToken.None));
        var department = await DepartmentTestData.FindAsync(Services, departmentId);
        var descendant = await DepartmentTestData.FindAsync(Services, descendantId);

        Assert.True(result.IsSuccess);
        Assert.Null(department.Parent);
        Assert.Equal("department", department.Path.Value);
        Assert.Equal(0, department.Depth.Value);
        Assert.Equal("department.descendant", descendant.Path.Value);
        Assert.Equal(1, descendant.Depth.Value);
    }

    [Fact]
    public async Task ChangeParent_should_reject_cycle()
    {
        var rootId = DepartmentId.New();
        var childId = DepartmentId.New();
        var descendantId = DepartmentId.New();
        var locationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            var root = DepartmentTestData.Department(rootId, locationId, "Root", "root");
            var child = Department.CreateChild(Name.Create("Child").Value, Slug.Create("child").Value, root,
                [DepartmentLocation.Create(childId, locationId).Value], childId).Value;
            var descendant = Department.CreateChild(Name.Create("Descendant").Value, Slug.Create("descendant").Value, child,
                [DepartmentLocation.Create(descendantId, locationId).Value], descendantId).Value;
            dbContext.Departments.AddRange(root, child, descendant);
        });

        var result = await DepartmentTestData.ExecuteAsync<ChangeParentHandler, ChangeParentResponseDto>(Services, sut => sut.Handle(
            new ChangeParentCommand(rootId.Value, descendantId.Value), CancellationToken.None));

        Assert.True(result.IsFailure);
        Assert.Contains(result.Error.Messages, message => message.Code == "department.move.cycle");
    }

    [Fact]
    public async Task ChangeParent_should_reject_self_parent()
    {
        var departmentId = DepartmentId.New();
        var result = await DepartmentTestData.ExecuteAsync<ChangeParentHandler, ChangeParentResponseDto>(Services, sut => sut.Handle(
            new ChangeParentCommand(departmentId.Value, departmentId.Value), CancellationToken.None));

        Assert.True(result.IsFailure);
        Assert.Contains(result.Error.Messages, message => message.Code == "department.move.parent_is_self");
    }

    [Fact]
    public async Task ChangeParent_should_reject_soft_deleted_parent()
    {
        var parentId = DepartmentId.New();
        var departmentId = DepartmentId.New();
        var locationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            var deletedParent = DepartmentTestData.Department(parentId, locationId, "Deleted parent", "deletedparent");
            deletedParent.SoftDelete();
            var department = DepartmentTestData.Department(departmentId, locationId, "Department", "department");
            dbContext.Departments.AddRange(deletedParent, department);
        });

        var result = await DepartmentTestData.ExecuteAsync<ChangeParentHandler, ChangeParentResponseDto>(Services, sut => sut.Handle(
            new ChangeParentCommand(departmentId.Value, parentId.Value), CancellationToken.None));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ChangeParent_should_return_not_found_when_parent_does_not_exist()
    {
        var departmentId = DepartmentId.New();
        var locationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            dbContext.Departments.Add(DepartmentTestData.Department(departmentId, locationId));
        });

        var result = await DepartmentTestData.ExecuteAsync<ChangeParentHandler, ChangeParentResponseDto>(Services, sut => sut.Handle(
            new ChangeParentCommand(departmentId.Value, Guid.NewGuid()), CancellationToken.None));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task ChangeParent_should_be_idempotent_when_parent_is_unchanged()
    {
        var parentId = DepartmentId.New();
        var departmentId = DepartmentId.New();
        var locationId = LocationId.New();
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            var parent = DepartmentTestData.Department(parentId, locationId, "Parent", "parent");
            var department = Department.CreateChild(Name.Create("Department").Value, Slug.Create("department").Value, parent,
                [DepartmentLocation.Create(departmentId, locationId).Value], departmentId).Value;
            dbContext.Departments.AddRange(parent, department);
        });

        var before = await DepartmentTestData.FindAsync(Services, departmentId);
        var result = await DepartmentTestData.ExecuteAsync<ChangeParentHandler, ChangeParentResponseDto>(Services, sut => sut.Handle(
            new ChangeParentCommand(departmentId.Value, parentId.Value), CancellationToken.None));
        var after = await DepartmentTestData.FindAsync(Services, departmentId);

        Assert.True(result.IsSuccess);
        Assert.Equal(before.Path.Value, result.Value.Path);
        Assert.Equal(before.UpdatedAt, result.Value.UpdatedAt);
        Assert.Equal(before.UpdatedAt, after.UpdatedAt);
    }

    [Fact]
    public async Task ChangeParent_should_update_large_subtree_with_two_update_statements()
    {
        var oldParentId = DepartmentId.New();
        var newParentId = DepartmentId.New();
        var departmentId = DepartmentId.New();
        var locationId = LocationId.New();
        const int descendantCount = 100;
        await DepartmentTestData.SeedAsync(Services, dbContext =>
        {
            dbContext.Locations.Add(DepartmentTestData.Location(locationId));
            var oldParent = DepartmentTestData.Department(oldParentId, locationId, "Old parent", "oldparent");
            var newParent = DepartmentTestData.Department(newParentId, locationId, "New parent", "newparent");
            var department = Department.CreateChild(Name.Create("Department").Value, Slug.Create("department").Value, oldParent,
                [DepartmentLocation.Create(departmentId, locationId).Value], departmentId).Value;
            var departments = new List<Department> { oldParent, newParent, department };
            for (var index = 0; index < descendantCount; index++)
            {
                var descendantId = DepartmentId.New();
                departments.Add(Department.CreateChild(
                    Name.Create($"Descendant {index}").Value,
                    Slug.Create($"descendant{new string('a', index + 1)}").Value,
                    department,
                    [DepartmentLocation.Create(descendantId, locationId).Value],
                    descendantId).Value);
            }

            dbContext.Departments.AddRange(departments);
        });

        var suffix = Guid.NewGuid().ToString("N");
        var auditTable = $"change_parent_updates_{suffix}";
        var auditFunction = $"count_change_parent_updates_{suffix}";
        var auditTrigger = $"count_change_parent_updates_trigger_{suffix}";
        await DepartmentTestData.ExecuteSqlAsync(Services, $"""
            CREATE TABLE {auditTable} (id integer NOT NULL);
            CREATE OR REPLACE FUNCTION {auditFunction}() RETURNS trigger
            LANGUAGE plpgsql AS $$
            BEGIN
                INSERT INTO {auditTable} VALUES (1);
                RETURN NULL;
            END;
            $$;
            CREATE TRIGGER {auditTrigger}
            AFTER UPDATE ON departments
            FOR EACH STATEMENT EXECUTE FUNCTION {auditFunction}();
            """);

        try
        {
            var result = await DepartmentTestData.ExecuteAsync<ChangeParentHandler, ChangeParentResponseDto>(Services, sut => sut.Handle(
                new ChangeParentCommand(departmentId.Value, newParentId.Value), CancellationToken.None));
            var updateCount = await DepartmentTestData.ScalarAsync<int>(Services, $"SELECT COUNT(*) AS \"Value\" FROM {auditTable}");

            Assert.True(result.IsSuccess);
            Assert.Equal(2, updateCount);
        }
        finally
        {
            await DepartmentTestData.ExecuteSqlAsync(Services, $"""
                DROP TRIGGER IF EXISTS {auditTrigger} ON departments;
                DROP FUNCTION IF EXISTS {auditFunction}();
                DROP TABLE IF EXISTS {auditTable};
                """);
        }
    }

    [Fact]
    public async Task ChangeParent_should_fail_when_department_not_found()
    {
        var result = await DepartmentTestData.ExecuteAsync<ChangeParentHandler, ChangeParentResponseDto>(Services, sut => sut.Handle(
            new ChangeParentCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));

        Assert.True(result.IsFailure);
    }
}
