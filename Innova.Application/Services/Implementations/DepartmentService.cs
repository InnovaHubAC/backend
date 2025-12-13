namespace Innova.Application.Services.Implementations;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public DepartmentService(
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<ApiResponse<PaginationDto<DepartmentDto>>> GetAllDepartmentsAsync(PaginationParams paginationParams)
    {
        var cacheKey = $"Department:GetAll:Page{paginationParams.PageIndex}:Size{paginationParams.PageSize}:Sort{paginationParams.Sort ?? "default"}";
        
        var cachedResult = _cacheService.Get<ApiResponse<PaginationDto<DepartmentDto>>>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var orderBy = paginationParams.Sort?.ToLower() switch
        {
            "name" => new Func<IQueryable<Department>, IOrderedQueryable<Department>>(q => q.OrderBy(d => d.Name)),
            "name_desc" => new Func<IQueryable<Department>, IOrderedQueryable<Department>>(q => q.OrderByDescending(d => d.Name)),
            _ => new Func<IQueryable<Department>, IOrderedQueryable<Department>>(q => q.OrderBy(d => d.Id))
        };

        var (departments, totalCount) = await _unitOfWork.DepartmentRepository.ListPagedAsync(
            paginationParams.PageIndex,
            paginationParams.PageSize,
            orderBy: orderBy);

        var dtos = departments.Adapt<IReadOnlyList<DepartmentDto>>();

        var paginationDtos = new PaginationDto<DepartmentDto>(
            paginationParams.PageIndex,
            paginationParams.PageSize,
            totalCount,
            dtos
        );

        var response = new ApiResponse<PaginationDto<DepartmentDto>>(200, paginationDtos, "Departments retrieved successfully.");
        
        _cacheService.Set(cacheKey, response, _cacheService.SetMemoryCacheEntryOptions(TimeSpan.FromMinutes(30),TimeSpan.FromMinutes(15)));

        return response;
    }

    public async Task<ApiResponse<DepartmentDto>> GetDepartmentByIdAsync(int id)
    {
        var cacheKey = $"Department:GetById:{id}";
        
        var cachedResult = _cacheService.Get<ApiResponse<DepartmentDto>>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(id);
        if (department == null)
        {
            return new ApiResponse<DepartmentDto>(404, null, $"Department with id {id} not found.");
        }
        
        var dto = department.Adapt<DepartmentDto>();
        var response = new ApiResponse<DepartmentDto>(200, dto, "Department retrieved successfully.");
        
        _cacheService.Set(cacheKey, response, _cacheService.SetMemoryCacheEntryOptions(TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(10)));
        
        return response;
    }

    public async Task<ApiResponse<DepartmentDto>> CreateDepartmentAsync(CreateDepartmentDto createDto)
    {
        var existing = await _unitOfWork.DepartmentRepository.FirstOrDefaultAsync(
            d => d.Name.ToLower() == createDto.Name.ToLower());
            
        if (existing != null)
        {
            return new ApiResponse<DepartmentDto>(400, null, $"Department with name '{createDto.Name}' already exists.");
        }

        var department = createDto.Adapt<Department>();
        await _unitOfWork.DepartmentRepository.AddAsync(department);
        await _unitOfWork.CompleteAsync();

        InvalidateAllDepartmentListCache();

        var dto = department.Adapt<DepartmentDto>();
        return new ApiResponse<DepartmentDto>(201, dto, "Department created successfully.");
    }

    public async Task<ApiResponse<DepartmentDto>> UpdateDepartmentAsync(int id, UpdateDepartmentDto updateDto)
    {
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(id);
        if (department == null)
        {
            return new ApiResponse<DepartmentDto>(404, null, $"Department with id {id} not found.");
        }

        var existing = await _unitOfWork.DepartmentRepository.FirstOrDefaultAsync(
            d => d.Name.ToLower() == updateDto.Name.ToLower());
        if (existing != null && existing.Id != id)
        {
            return new ApiResponse<DepartmentDto>(400, null, $"Department with name '{updateDto.Name}' already exists.");
        }

        department.Name = updateDto.Name;
        department.Description = updateDto.Description;

        _unitOfWork.DepartmentRepository.Update(department);
        await _unitOfWork.CompleteAsync();

        _cacheService.Remove($"Department:GetById:{id}");
        InvalidateAllDepartmentListCache();

        var dto = department.Adapt<DepartmentDto>();
        return new ApiResponse<DepartmentDto>(200, dto, "Department updated successfully.");
    }

    public async Task<ApiResponse<DeletedDto>> DeleteDepartmentAsync(int id)
    {
        var department = await _unitOfWork.DepartmentRepository.GetByIdAsync(id);
        if (department == null)
        {
            return new ApiResponse<DeletedDto>(404, new DeletedDto { IsDeleted = false }, $"Department with id {id} not found.");
        }

        await _unitOfWork.DepartmentRepository.DeleteAsync(department);
        await _unitOfWork.CompleteAsync();

        _cacheService.Remove($"Department:GetById:{id}");
        InvalidateAllDepartmentListCache();

        return new ApiResponse<DeletedDto>(200, new DeletedDto { IsDeleted = true }, "Department deleted successfully.");
    }

    private void InvalidateAllDepartmentListCache()
    {
        _cacheService.RemoveByPrefix("Department:GetAll:");
    }
}
