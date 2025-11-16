namespace Innova.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DepartmentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<DepartmentDto>>>> GetAllDepartments()
    {
        var departmentsResponse = await _departmentService.GetAllDepartmentsAsync();
        return Ok(departmentsResponse);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> GetDepartmentById(int id)
    {
        var departmentResponse = await _departmentService.GetDepartmentByIdAsync(id);

        if (departmentResponse.Data == null)
        {
            return NotFound(ApiResponse<DepartmentDto>.Fail(404, $"Department with ID {id} not found"));
        }

        return Ok(departmentResponse);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> CreateDepartment([FromBody] CreateDepartmentDto createDto)
    {
        var departmentResponse = await _departmentService.CreateDepartmentAsync(createDto);

        if (departmentResponse.Data == null)
        {
            return BadRequest(departmentResponse);
        }

        return CreatedAtAction(
            nameof(GetDepartmentById),
            new { id = departmentResponse.Data!.Id },
            departmentResponse);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DepartmentDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<DepartmentDto>>> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto updateDto)
    {

        var departmentResponse = await _departmentService.UpdateDepartmentAsync(id, updateDto);

        if (departmentResponse.StatusCode == StatusCodes.Status404NotFound)
        {
            return NotFound(ApiResponse<DepartmentDto>.Fail(404, $"Department with ID {id} not found"));
        }

        if(departmentResponse.StatusCode == StatusCodes.Status400BadRequest)
        {
            return BadRequest(departmentResponse);
        }

        return Ok(departmentResponse);

    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDepartment(int id)
    {
        var response = await _departmentService.DeleteDepartmentAsync(id);

        if (response.Data!.IsDeleted == false)
        {
            return NotFound(ApiResponse<bool>.Fail(404, $"Department with ID {id} not found"));
        }

        return Ok(response);
    }
}

