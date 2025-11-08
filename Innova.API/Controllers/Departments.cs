namespace Innova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Departments : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public Departments(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DepartmentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DepartmentDto>>>> GetAllDepartments()
    {
        var departments = await _departmentService.GetAllDepartmentsAsync();
        return Ok(ApiResponse<IReadOnlyList<DepartmentDto>>.Success(departments));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetDepartmentById(int id)
    {
        var department = await _departmentService.GetDepartmentByIdAsync(id);

        if (department == null)
        {
            return NotFound(ApiResponse<DepartmentDto>.Fail(404, $"Department with ID {id} not found"));
        }

        return Ok(ApiResponse<DepartmentDto>.Success(department));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> CreateDepartment([FromBody] CreateDepartmentDto createDto)
    {
        var department = await _departmentService.CreateDepartmentAsync(createDto);
        return CreatedAtAction(
            nameof(GetDepartmentById),
            new { id = department.Id },
            new ApiResponse<DepartmentDto>(201, department, "Department created successfully"));
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto updateDto)
    {

        var department = await _departmentService.UpdateDepartmentAsync(id, updateDto);

        if (department == null)
        {
            return NotFound(ApiResponse<DepartmentDto>.Fail(404, $"Department with ID {id} not found"));
        }

        return Ok(new ApiResponse<DepartmentDto>(200, department, "Department updated successfully"));

    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDepartment(int id)
    {
        var result = await _departmentService.DeleteDepartmentAsync(id);

        if (!result)
        {
            return NotFound(ApiResponse<bool>.Fail(404, $"Department with ID {id} not found"));
        }

        return Ok(new ApiResponse<bool>(200, true, "Department deleted successfully"));
    }
}

