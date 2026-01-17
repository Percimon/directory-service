using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Requests;

public record UpdateLocationsRequest(IReadOnlyList<Guid> LocationIds);