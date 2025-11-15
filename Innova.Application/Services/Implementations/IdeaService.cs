using Innova.Application.DTOs.Idea;
using Innova.Application.Validations.Idea;

namespace Innova.Application.Services.Implementations
{
    public class IdeaService : IIdeaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly IFileStorageService _fileStorageService;
        private readonly CreateIdeaDtoValidator _createIdeaValidator;

        public IdeaService(IUnitOfWork unitOfWork, IIdentityService identityService, IFileStorageService fileStorageService)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _fileStorageService = fileStorageService;
            _createIdeaValidator = new CreateIdeaDtoValidator();
        }


        private async Task<ApiResponse<bool>> ValidateCreateIdeaAsync(CreateIdeaDto createIdeaDto)
        {
            var dtoValidationResult = ValidateDto(createIdeaDto);
            if (!dtoValidationResult.Data)
                return dtoValidationResult;

            var departmentValidationResult = await ValidateDepartmentExistsAsync(createIdeaDto.DepartmentId);
            if (!departmentValidationResult.Data)
                return departmentValidationResult;

            var userValidationResult = await ValidateUserExistsAsync(createIdeaDto.AppUserId);
            if (!userValidationResult.Data)
                return userValidationResult;

            return ApiResponse<bool>.Success(true);
        }

        private ApiResponse<bool> ValidateDto(CreateIdeaDto createIdeaDto)
        {
            var validationResult = _createIdeaValidator.Validate(createIdeaDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResponse<bool>.Fail(400, "Validation Failed", errors);
            }

            return ApiResponse<bool>.Success(true);
        }

        private async Task<ApiResponse<bool>> ValidateDepartmentExistsAsync(int departmentId)
        {
            var departmentByIdSpecification = new GetDepartmentByIdSpecification(departmentId);
            var departmentCount = await _unitOfWork.DepartmentRepository.CountAsync(departmentByIdSpecification);

            if (departmentCount == 0)
                return ApiResponse<bool>.Fail(404, "Department not found");

            return ApiResponse<bool>.Success(true);
        }

        private async Task<ApiResponse<bool>> ValidateUserExistsAsync(string userId)
        {
            if (!await _identityService.UserExistsAsync(userId))
                return ApiResponse<bool>.Fail(404, "User not found");

            return ApiResponse<bool>.Success(true);
        }


        private async Task<ApiResponse<bool>> SaveIdeaAsync(CreateIdeaDto createIdeaDto)
        {
            Idea idea = await CreateIdeaEntityAsync(createIdeaDto);
            await _unitOfWork.IdeaRepository.AddAsync(idea);
            await _unitOfWork.CompleteAsync();
            return ApiResponse<bool>.Success(true);
        }

        private async Task<Idea> CreateIdeaEntityAsync(CreateIdeaDto createIdeaDto)
        {
            Idea idea = createIdeaDto.Adapt<Idea>();
            await SaveIdeaAttachmentsAsync(createIdeaDto, idea);
            return idea;
        }

        private async Task SaveIdeaAttachmentsAsync(CreateIdeaDto createIdeaDto, Idea idea)
        {
            if (createIdeaDto.Attachments != null && createIdeaDto.Attachments.Count > 0)
            {
                foreach (var file in createIdeaDto.Attachments)
                {
                    var extension = Path.GetExtension(file.FileName);
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var fileUrl = await _fileStorageService.SaveFileAsync(file.Data, fileName, file.ContentType);
                    var attachment = new Attachment
                    {
                        FileName = fileName,
                        FileType = file.ContentType,
                        FileUrl = fileUrl,
                    };
                    idea.Attachments!.Add(attachment);
                }
            }
        }

        public async Task<ApiResponse<bool>> CreateIdeaAsync(CreateIdeaDto createIdeaDto)
        {
            var validationResponse = await ValidateCreateIdeaAsync(createIdeaDto);
            if (!validationResponse.Data)
                return validationResponse;
            return await SaveIdeaAsync(createIdeaDto);
        }
    }
}