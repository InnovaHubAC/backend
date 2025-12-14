namespace Innova.Application.Services.Implementations
{
    public class IdeaService : IIdeaService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IIdentityService _identityService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<IdeaService> _logger;

        public IdeaService(
            IUnitOfWork unitOfWork, 
            IIdentityService identityService, 
            IFileStorageService fileStorageService,
            ILogger<IdeaService> logger)
        {
            _unitOfWork = unitOfWork;
            _identityService = identityService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> CreateIdeaAsync(CreateIdeaDto createIdeaDto)
        {
            var validationResponse = await ValidateCreateIdeaAsync(createIdeaDto);
            if (!validationResponse.Data)
            {
                _logger.LogWarning(
                    "Idea creation validation failed. UserId: {UserId}, DepartmentId: {DepartmentId}, ValidationDetails: {ValidationDetails}",
                    createIdeaDto.AppUserId,
                    createIdeaDto.DepartmentId,
                    validationResponse.Message);
                return validationResponse;
            }

            var result = await SaveIdeaAsync(createIdeaDto);
            
            if (result.Data)
            {
                _logger.LogInformation(
                    "Idea created successfully. UserId: {UserId}, DepartmentId: {DepartmentId}, AttachmentCount: {AttachmentCount}, IsAnonymous: {IsAnonymous}",
                    createIdeaDto.AppUserId,
                    createIdeaDto.DepartmentId,
                    createIdeaDto.Attachments?.Count ?? 0,
                    createIdeaDto.IsAnonymous);
            }

            return result;
        }

        public async Task<ApiResponse<bool>> UpdateIdeaAsync(UpdateIdeaDto updateIdeaDto)
        {
            var validationResponse = await ValidateUpdateIdeaAsync(updateIdeaDto);
            if (!validationResponse.Data)
            {
                _logger.LogWarning(
                    "Idea update validation failed. IdeaId: {IdeaId}, UserId: {UserId}, ValidationDetails: {ValidationDetails}",
                    updateIdeaDto.Id,
                    updateIdeaDto.AppUserId,
                    validationResponse.Message);
                return ApiResponse<bool>.Fail(validationResponse.StatusCode, validationResponse.Message!, validationResponse.Details);
            }

            var result = await UpdateAndSaveIdeaAsync(updateIdeaDto);

            if (result.Data)
            {
                _logger.LogInformation(
                    "Idea updated successfully. IdeaId: {IdeaId}, UserId: {UserId}, NewAttachmentCount: {NewAttachmentCount}, RemovedAttachmentCount: {RemovedAttachmentCount}",
                    updateIdeaDto.Id,
                    updateIdeaDto.AppUserId,
                    updateIdeaDto.Attachments?.Count ?? 0,
                    updateIdeaDto.RemovedAttachmentIds?.Count ?? 0);
            }

            return result;
        }

        public async Task<ApiResponse<IdeaDetailsDto>> GetIdeaDetailsAsync(int ideaId)
        {
            var idea = await _unitOfWork.IdeaRepository.GetByIdWithIncludesAsync(ideaId, new()
            {
                x => x.Attachments!,
                x => x.Department
            });

            if (idea is null)
            {
                _logger.LogWarning(
                    "Idea retrieval failed. Idea not found. IdeaId: {IdeaId}",
                    ideaId);
                return ApiResponse<IdeaDetailsDto>.Fail(404, "Idea not found");
            }

            IdeaDetailsDto ideaDetailsDto = await CreateIdeaDetailsDtoAsync(ideaId, idea);

            _logger.LogInformation(
                "Idea details retrieved successfully. IdeaId: {IdeaId}, DepartmentId: {DepartmentId}, AttachmentCount: {AttachmentCount}",
                ideaId,
                idea.DepartmentId,
                idea.Attachments?.Count ?? 0);

            return ApiResponse<IdeaDetailsDto>.Success(ideaDetailsDto);
        }

        public async Task<ApiResponse<bool>> DeleteIdeaAsync(int ideaId, string userId)
        {
            var idea = await _unitOfWork.IdeaRepository.GetByIdWithIncludesAsync(ideaId, new()
            {
                x => x.Attachments!
            });

            if (idea is null)
            {
                _logger.LogWarning(
                    "Idea deletion failed. Idea not found. IdeaId: {IdeaId}, UserId: {UserId}",
                    ideaId,
                    userId);
                return ApiResponse<bool>.Fail(404, "Idea not found");
            }

            if (idea.AppUserId != userId)
            {
                _logger.LogWarning(
                    "Idea deletion failed. Unauthorized access attempt. IdeaId: {IdeaId}, IdeaOwnerId: {IdeaOwnerId}, RequestingUserId: {RequestingUserId}",
                    ideaId,
                    idea.AppUserId,
                    userId);
                return ApiResponse<bool>.Fail(403, "You do not have permission to delete this idea");
            }

            var attachmentCount = idea.Attachments?.Count ?? 0;

            await _unitOfWork.IdeaRepository.DeleteAsync(idea);
            await _unitOfWork.CompleteAsync();

            // Delete attachments from storage
            DeleteAttachmentsFromStorage(idea.Attachments?.ToList());

            _logger.LogInformation(
                "Idea deleted successfully. IdeaId: {IdeaId}, UserId: {UserId}, DeletedAttachmentCount: {DeletedAttachmentCount}",
                ideaId,
                userId,
                attachmentCount);

            return ApiResponse<bool>.Success(true);
        }

        public async Task<ApiResponse<PaginationDto<IdeaDetailsDto>>>
         GetIdeasByUserIdAsync(string userId, PaginationParams paginationParams)
        {
            // TODO: rewrite this to utilize the query syntax to avoid looping and making two queries
            var ideas = await _unitOfWork.IdeaRepository.ListAsync(
                predicate: i => i.AppUserId == userId,
                orderBy: q => q.OrderByDescending(i => i.CreatedAt),
                includes: new() { i => i.Department, i => i.Attachments! });

            var user = await _identityService.GetUserForIdeaAsync(userId);
            if (user == null)
            {
                _logger.LogWarning(
                    "User ideas retrieval failed. User not found. UserId: {UserId}",
                    userId);
                return ApiResponse<PaginationDto<IdeaDetailsDto>>.Fail(404, "User not found");
            }

            var dtos = ideas.Adapt<IReadOnlyList<IdeaDetailsDto>>();

            foreach (var dto in dtos)
            {
                dto.User.UserName = user.Value.UserName;
                dto.User.FirstName = user.Value.FirstName;
                dto.User.LastName = user.Value.LastName;
                dto.User.Id = userId;
            }

            var pagination = new PaginationDto<IdeaDetailsDto>(paginationParams.PageIndex,
             paginationParams.PageSize, ideas.Count, dtos);

            _logger.LogInformation(
                "User ideas retrieved successfully. UserId: {UserId}, IdeaCount: {IdeaCount}, PageIndex: {PageIndex}, PageSize: {PageSize}",
                userId,
                ideas.Count,
                paginationParams.PageIndex,
                paginationParams.PageSize);

            return ApiResponse<PaginationDto<IdeaDetailsDto>>.Success(pagination);
        }

        public async Task<ApiResponse<PaginationDto<IdeaDetailsDto>>> GetAllIdeasAsync(PaginationParams paginationParams)
        {
            var (ideas, totalCount) = await _unitOfWork.IdeaRepository.GetAllIdeasPagedAsync(
                paginationParams.PageIndex,
                paginationParams.PageSize,
                paginationParams.Sort);

            var ideaDetailsDtos = new List<IdeaDetailsDto>();

            foreach (var idea in ideas)
            {
                var ideaDetailsDto = await CreateIdeaDetailsDtoAsync(idea.Id, idea);
                ideaDetailsDtos.Add(ideaDetailsDto);
            }

            var pagination = new PaginationDto<IdeaDetailsDto>(
                paginationParams.PageIndex,
                paginationParams.PageSize,
                totalCount,
                ideaDetailsDtos);

            _logger.LogInformation(
                "All ideas retrieved successfully. TotalCount: {TotalCount}, PageIndex: {PageIndex}, PageSize: {PageSize}, Sort: {Sort}",
                totalCount,
                paginationParams.PageIndex,
                paginationParams.PageSize,
                paginationParams.Sort ?? "default");

            return ApiResponse<PaginationDto<IdeaDetailsDto>>.Success(pagination);
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
            var createIdeaValidator = new CreateIdeaDtoValidator();
            var validationResult = createIdeaValidator.Validate(createIdeaDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();

                _logger.LogWarning(
                    "Idea DTO validation failed. UserId: {UserId}, ValidationErrors: {@ValidationErrors}",
                    createIdeaDto.AppUserId,
                    errors);

                return ApiResponse<bool>.Fail(400, "Validation Failed", errors);
            }

            return ApiResponse<bool>.Success(true);
        }

        private async Task<ApiResponse<bool>> ValidateDepartmentExistsAsync(int departmentId)
        {
            var departmentExists = await _unitOfWork.DepartmentRepository.AnyAsync(d => d.Id == departmentId);

            if (!departmentExists)
            {
                _logger.LogWarning(
                    "Idea validation failed. Department not found. DepartmentId: {DepartmentId}",
                    departmentId);
                return ApiResponse<bool>.Fail(404, "Department not found");
            }

            return ApiResponse<bool>.Success(true);
        }

        private async Task<ApiResponse<bool>> ValidateUserExistsAsync(string userId)
        {
            if (!await _identityService.UserExistsAsync(userId))
            {
                _logger.LogWarning(
                    "Idea validation failed. User not found. UserId: {UserId}",
                    userId);
                return ApiResponse<bool>.Fail(404, "User not found");
            }

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
                _logger.LogInformation(
                    "Saving idea attachments. AttachmentCount: {AttachmentCount}, UserId: {UserId}",
                    createIdeaDto.Attachments.Count,
                    createIdeaDto.AppUserId);

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
            var updateIdeaValidator = new UpdateIdeaDtoValidator();
            var validationResult = updateIdeaValidator.Validate(updateIdeaDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();

                _logger.LogWarning(
                    "Idea update DTO validation failed. IdeaId: {IdeaId}, UserId: {UserId}, ValidationErrors: {@ValidationErrors}",
                    updateIdeaDto.Id,
                    updateIdeaDto.AppUserId,
                    errors);

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
            {
                _logger.LogWarning(
                    "Idea update failed. Idea not found. IdeaId: {IdeaId}, UserId: {UserId}",
                    updateIdeaDto.Id,
                    updateIdeaDto.AppUserId);
                return ApiResponse<bool>.Fail(404, "Idea not found");
            }

            // Identify attachments to remove from storage after updating the idea entity.
            var attachmentsToRemoveFromStorage = idea.Attachments?
                .Where(a => updateIdeaDto.RemovedAttachmentIds != null && updateIdeaDto.RemovedAttachmentIds.Contains(a.Id))
                .ToList();

            if (attachmentsToRemoveFromStorage != null && attachmentsToRemoveFromStorage.Any())
            {
                _logger.LogInformation(
                    "Removing attachments from idea. IdeaId: {IdeaId}, AttachmentCount: {AttachmentCount}",
                    updateIdeaDto.Id,
                    attachmentsToRemoveFromStorage.Count);
            }

            await SaveUpdatedIdeaEntityAsync(updateIdeaDto, idea!);
            DeleteAttachmentsFromStorage(attachmentsToRemoveFromStorage);
            return ApiResponse<bool>.Success(true);
        }

        private void DeleteAttachmentsFromStorage(List<Attachment>? attachments)
        {
            if (attachments is null || !attachments.Any())
                return;

            _logger.LogInformation(
                "Deleting attachments from storage. AttachmentCount: {AttachmentCount}",
                attachments.Count);

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
                _logger.LogInformation(
                    "Adding new attachments to idea. IdeaId: {IdeaId}, NewAttachmentCount: {NewAttachmentCount}",
                    updateIdeaDto.Id,
                    updateIdeaDto.Attachments.Count);

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

        private async Task<IdeaDetailsDto> CreateIdeaDetailsDtoAsync(int ideaId, Idea idea)
        {
            var user = await _identityService.GetUserForIdeaAsync(idea.AppUserId);
            var ideaDetailsDto = idea.Adapt<IdeaDetailsDto>();
            ideaDetailsDto.IdeaAttachments = idea.Attachments!.Adapt<List<AttachmentDto>>();
            ideaDetailsDto.User.UserName = user.Value.UserName;
            ideaDetailsDto.User.FirstName = user.Value.FirstName;
            ideaDetailsDto.User.LastName = user.Value.LastName;
            ideaDetailsDto.User.Id = idea.AppUserId;

            VoteType? userVoteType = null;
            if (!string.IsNullOrEmpty(ideaDetailsDto.User.Id))
            {
                var userVote = await _unitOfWork.VoteRepository.GetUserVoteForIdeaAsync(ideaId, ideaDetailsDto.User.Id);
                if (userVote != null && userVote.VoteType != VoteType.Withdraw)
                {
                    userVoteType = userVote.VoteType;
                }
            }

            ideaDetailsDto.VoteStats = new VoteStatsDto
            {
                UpvoteCount = await _unitOfWork.VoteRepository.GetUpvoteCountForIdeaAsync(ideaId),
                DownvoteCount = await _unitOfWork.VoteRepository.GetDownvoteCountForIdeaAsync(ideaId),
                UserVoteType = userVoteType
            };
            return ideaDetailsDto;
        }
    }
}