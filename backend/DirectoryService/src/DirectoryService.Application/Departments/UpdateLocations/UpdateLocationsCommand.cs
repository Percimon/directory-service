using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectoryService.Application.Departments.UpdateLocations;

public record UpdateLocationsCommand(Guid DepartmentId, IReadOnlyList<Guid> LocationIds);