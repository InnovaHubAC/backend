using Innova.Application.DTOs.Attachment;
using Innova.Application.DTOs.Idea;
using Innova.Application.Validations.Idea;
using System.Linq.Expressions;

namespace Innova.Application.Services.Implementations
{
    public class IdeaService : IIdeaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly IFileStorageService _fileStorageService;
        private readonly CreateIdeaDtoValidator _createIdeaValidator;
        private readonly UpdateIdeaDtoValidator _updateIdeaValidator;

        public IdeaService(IUnitOfWork unitOfWork, IIdentityService identityService, IFileStorageService fileStorageService)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _fileStorageService = fileStorageService;
            _createIdeaValidator = new CreateIdeaDtoValidator();
            _updateIdeaValidator = new UpdateIdeaDtoValidator();
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

        public async Task<ApiResponse<bool>> UpdateIdeaAsync(UpdateIdeaDto updateIdeaDto)
        {
            var validationResponse = await ValidateUpdateIdeaAsync(updateIdeaDto);
            if (!validationResponse.Data)
                return ApiResponse<bool>.Fail(validationResponse.StatusCode, validationResponse.Message!, validationResponse.Details);

            return await UpdateAndSaveIdeaAsync(updateIdeaDto);
        }

        private async Task<ApiResponse<bool>> ValidateUpdateIdeaAsync(UpdateIdeaDto updateIdeaDto)
        {
            var dtoValidationResult = ValidateUpdateDto(updateIdeaDto);
            if (!dtoValidationResult.Data)
                return dtoValidationResult;

            var departmentValidationResult = await ValidateDepartmentExistsAsync(updateIdeaDto.DepartmentId);
            if (!departmentValidationResult.Data)
                return departmentValidationResult;

            var userValidationResult = await ValidateUserExistsAsync(updateIdeaDto.AppUserId);
            if (!userValidationResult.Data)
                return userValidationResult;

            return ApiResponse<bool>.Success(true);
        }

        private ApiResponse<bool> ValidateUpdateDto(UpdateIdeaDto updateIdeaDto)
        {
            var validationResult = _updateIdeaValidator.Validate(updateIdeaDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResponse<bool>.Fail(400, "Validation Failed", errors);
            }

            return ApiResponse<bool>.Success(true);
        }

        private async Task<ApiResponse<bool>> UpdateAndSaveIdeaAsync(UpdateIdeaDto updateIdeaDto)
        {
            var idea = await _unitOfWork.IdeaRepository.GetByIdWithIncludesAsync(updateIdeaDto.Id, new()
            {
                x => x.Attachments!,
                x => x.Department
            });

            if (idea is null)
                return ApiResponse<bool>.Fail(404, "Idea not found");

            // Identify attachments to remove from storage after updating the idea entity.
            var attachmentsToRemoveFromStorage = idea.Attachments?
                .Where(a => updateIdeaDto.RemovedAttachmentIds != null && updateIdeaDto.RemovedAttachmentIds.Contains(a.Id))
                .ToList();
            await SaveUpdatedIdeaEntityAsync(updateIdeaDto, idea!);
            DeleteAttachmentsFromStorage(attachmentsToRemoveFromStorage);
            return ApiResponse<bool>.Success(true);
        }

        private void DeleteAttachmentsFromStorage(List<Attachment>? attachments)
        {
            if (attachments is null || !attachments.Any())
                return;
            attachments.ForEach(attachment => _fileStorageService.RemoveFile(attachment.FileUrl, attachment.FileType));
        }
        private async Task SaveUpdatedIdeaEntityAsync(UpdateIdeaDto updateIdeaDto, Idea idea)
        {
            updateIdeaDto.Adapt(idea, TypeAdapterConfig.GlobalSettings);
            idea.UpdatedAt = DateTime.UtcNow;
            await HandleAttachmentUpdatesAsync(updateIdeaDto, idea);
            _unitOfWork.IdeaRepository.Update(idea!);
            await _unitOfWork.CompleteAsync();
        }

        private async Task HandleAttachmentUpdatesAsync(UpdateIdeaDto updateIdeaDto, Idea idea)
        {
            RemoveMarkedAttachments(updateIdeaDto, idea);
            await AddNewAttachmentsAsync(updateIdeaDto, idea);
        }

        private async Task AddNewAttachmentsAsync(UpdateIdeaDto updateIdeaDto, Idea idea)
        {
            // Add new attachments
            if (updateIdeaDto.Attachments != null && updateIdeaDto.Attachments.Any())
            {
                foreach (var file in updateIdeaDto.Attachments)
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

        private static void RemoveMarkedAttachments(UpdateIdeaDto updateIdeaDto, Idea idea)
        {
            if (updateIdeaDto.RemovedAttachmentIds != null && updateIdeaDto.RemovedAttachmentIds.Any())
            {
                var attachmentsToRemove = idea.Attachments!
                    .Where(a => updateIdeaDto.RemovedAttachmentIds.Contains(a.Id))
                    .ToList();
                attachmentsToRemove.ForEach(attachment => idea.Attachments!.Remove(attachment));
            }
        }

        public async Task<ApiResponse<IdeaDetailsDto>> GetIdeaDetailsAsync(int ideaId)
        {
            var idea = await _unitOfWork.IdeaRepository.GetByIdWithIncludesAsync(ideaId, new()
            {
                x => x.Attachments!,
                x => x.Department
            });

            if (idea is null)
                return ApiResponse<IdeaDetailsDto>.Fail(404, "Idea not found");

            var user = await _identityService.GetUserForIdeaAsync(idea.AppUserId);

            var ideaDetailsDto = idea.Adapt<IdeaDetailsDto>();
            ideaDetailsDto.IdeaAttachments = idea.Attachments!.Adapt<List<AttachmentDto>>();
            ideaDetailsDto.UserName = user.Value.UserName;
            ideaDetailsDto.FirstName = user.Value.FirstName;
            ideaDetailsDto.LastName = user.Value.LastName;
            ideaDetailsDto.UserId = idea.AppUserId;
            return ApiResponse<IdeaDetailsDto>.Success(ideaDetailsDto);
        }

        public async Task<ApiResponse<bool>> DeleteIdeaAsync(int ideaId,string userId)
        {
            var idea = await _unitOfWork.IdeaRepository.GetByIdWithIncludesAsync(ideaId, new()
            {
                x => x.Attachments!
            });

            if (idea is null)
                return ApiResponse<bool>.Fail(404, "Idea not found");

            if (idea.AppUserId != userId)
            {
                return ApiResponse<bool>.Fail(403, "You do not have permission to delete this idea");
            }

            await _unitOfWork.IdeaRepository.DeleteAsync(idea);
            await _unitOfWork.CompleteAsync();

            // Delete attachments from storage
            DeleteAttachmentsFromStorage(idea.Attachments?.ToList());
            return ApiResponse<bool>.Success(true);
        }
    }
}