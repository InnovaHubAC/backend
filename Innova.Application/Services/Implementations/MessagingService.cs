namespace Innova.Application.Services.Implementations;

// IMPORTANT:: For caching in message service, we need to see the app in production to detect correctly which jobs should be cached and we need to decide on a caching strategy.

public class MessagingService : IMessagingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityService _identityService;
    private readonly ILogger<MessagingService> _logger;

    public MessagingService(
        IUnitOfWork unitOfWork,
        IIdentityService identityService,
        ILogger<MessagingService> logger)
    {
        _unitOfWork = unitOfWork;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<ApiResponse<MessageDto>> SendMessageAsync(string senderId, SendMessageDto sendMessageDto)
    {
        // TODO: use fluent validation instead of manual checks
        var receiver = await _identityService.GetUserByIdAsync(sendMessageDto.ReceiverId);
        if (receiver == null || !receiver.HasValue)
        {
            _logger.LogWarning(
                "Message send failed. Receiver not found. SenderId: {SenderId}, ReceiverId: {ReceiverId}",
                senderId,
                sendMessageDto.ReceiverId);
            return ApiResponse<MessageDto>.Fail(404, "Receiver not found");
        }

        var sender = await _identityService.GetUserByIdAsync(senderId);
        if (sender == null || !sender.HasValue)
        {
            _logger.LogWarning(
                "Message send failed. Sender not found. SenderId: {SenderId}",
                senderId);
            return ApiResponse<MessageDto>.Fail(404, "Sender not found");
        }

        var conversation = await _unitOfWork.ConversationRepository
            .GetConversationBetweenUsersAsync(senderId, sendMessageDto.ReceiverId);

        if (conversation == null)
        {
            conversation = new Conversation
            {
                ParticipantOneId = senderId,
                ParticipantTwoId = sendMessageDto.ReceiverId
            };
            await _unitOfWork.ConversationRepository.AddAsync(conversation);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "New conversation created. ConversationId: {ConversationId}, ParticipantOneId: {ParticipantOneId}, ParticipantTwoId: {ParticipantTwoId}",
                conversation.Id,
                senderId,
                sendMessageDto.ReceiverId);
        }

        var message = new Message
        {
            ConversationId = conversation.Id,
            SenderId = senderId,
            ReceiverId = sendMessageDto.ReceiverId,
            Content = sendMessageDto.Content
        };

        await _unitOfWork.MessageRepository.AddAsync(message);
        
        // update conversation last message time
        conversation.LastMessageAt = DateTime.UtcNow;
        _unitOfWork.ConversationRepository.Update(conversation);
        
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "Message sent successfully. MessageId: {MessageId}, ConversationId: {ConversationId}, SenderId: {SenderId}, ReceiverId: {ReceiverId}",
            message.Id,
            conversation.Id,
            senderId,
            sendMessageDto.ReceiverId);

        var messageDto = new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            SenderName = $"{sender.Value.FirstName} {sender.Value.LastName}".Trim(),
            ReceiverId = message.ReceiverId,
            ReceiverName = $"{receiver.Value.FirstName} {receiver.Value.LastName}".Trim(),
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead
        };

        // TODO: notify receiver via SignalR

        return ApiResponse<MessageDto>.Success(messageDto);
    }

    public async Task<ApiResponse<IEnumerable<ConversationDto>>> GetUserConversationsAsync(string userId)
    {
        var conversations = await _unitOfWork.ConversationRepository.GetUserConversationsAsync(userId);

        _logger.LogInformation(
            "User conversations retrieved successfully. UserId: {UserId}, ConversationCount: {ConversationCount}",
            userId,
            conversations.Count);

        var conversationDtos = new List<ConversationDto>();

        // TODO: this due to we are not including user details in conversation entity
        // so we may use query syntax to join user table later for optimization  
        foreach (var conversation in conversations)
        {
            var otherParticipantId = conversation.ParticipantOneId == userId 
                ? conversation.ParticipantTwoId 
                : conversation.ParticipantOneId;

            var otherParticipant = await _identityService.GetUserByIdAsync(otherParticipantId);
            var lastMessage = conversation.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
            var unreadCount = conversation.Messages.Count(m => m.ReceiverId == userId && !m.IsRead);

            var conversationDto = new ConversationDto
            {
                Id = conversation.Id,
                OtherParticipantId = otherParticipantId,
                OtherParticipantName = otherParticipant.HasValue 
                    ? $"{otherParticipant.Value.FirstName} {otherParticipant.Value.LastName}".Trim() 
                    : "Unknown User",
                CreatedAt = conversation.CreatedAt,
                LastMessageAt = conversation.LastMessageAt,
                UnreadCount = unreadCount,
                LastMessage = lastMessage != null ? new MessageDto
                {
                    Id = lastMessage.Id,
                    ConversationId = lastMessage.ConversationId,
                    SenderId = lastMessage.SenderId,
                    ReceiverId = lastMessage.ReceiverId,
                    Content = lastMessage.Content,
                    SentAt = lastMessage.SentAt,
                    IsRead = lastMessage.IsRead
                } : null
            };

            conversationDtos.Add(conversationDto);
        }

        return ApiResponse<IEnumerable<ConversationDto>>.Success(
            conversationDtos.OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt));
    }

    public async Task<ApiResponse<ConversationDetailDto>> GetConversationAsync(
        int conversationId, string userId, int page = 1, int pageSize = 10)
    {
        pageSize = Math.Max(10, pageSize);
        var conversation = await _unitOfWork.ConversationRepository
            .GetConversationWithMessagesAsync(conversationId, pageSize, page);

        if (conversation == null)
        {
            _logger.LogWarning(
                "Conversation retrieval failed. Conversation not found. ConversationId: {ConversationId}, UserId: {UserId}",
                conversationId,
                userId);
            return ApiResponse<ConversationDetailDto>.Fail(404, "Conversation not found");
        }

        // Ensure user is part of this conversation
        if (conversation.ParticipantOneId != userId && conversation.ParticipantTwoId != userId)
        {
            _logger.LogWarning(
                "Conversation retrieval failed. Unauthorized access attempt. ConversationId: {ConversationId}, UserId: {UserId}, ParticipantOneId: {ParticipantOneId}, ParticipantTwoId: {ParticipantTwoId}",
                conversationId,
                userId,
                conversation.ParticipantOneId,
                conversation.ParticipantTwoId);
            return ApiResponse<ConversationDetailDto>.Fail(403, "You are not part of this conversation");
        }

        var otherParticipantId = conversation.ParticipantOneId == userId 
            ? conversation.ParticipantTwoId 
            : conversation.ParticipantOneId;

        var otherParticipant = await _identityService.GetUserByIdAsync(otherParticipantId);

        var messageDtos = conversation.Messages
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Content = m.Content,
                SentAt = m.SentAt,
                ReadAt = m.ReadAt,
                IsRead = m.IsRead
            }).ToList();

        _logger.LogInformation(
            "Conversation retrieved successfully. ConversationId: {ConversationId}, UserId: {UserId}, MessageCount: {MessageCount}, Page: {Page}, PageSize: {PageSize}",
            conversationId,
            userId,
            messageDtos.Count,
            page,
            pageSize);

        var conversationDetail = new ConversationDetailDto
        {
            Id = conversation.Id,
            OtherParticipantId = otherParticipantId,
            OtherParticipantName = otherParticipant.HasValue 
                ? $"{otherParticipant.Value.FirstName} {otherParticipant.Value.LastName}".Trim() 
                : "Unknown User",
            CreatedAt = conversation.CreatedAt,
            LastMessageAt = conversation.LastMessageAt,
            Messages = messageDtos,
            CurrentPage = page,
            PageSize = pageSize
        };

        return ApiResponse<ConversationDetailDto>.Success(conversationDetail);
    }

    public async Task<ApiResponse<ConversationDetailDto>> GetOrCreateConversationAsync(
        string currentUserId, string otherUserId)
    {
        // TODO: use fluent validation instead of manual checks
        var otherUser = await _identityService.GetUserByIdAsync(otherUserId);
        if (otherUser == null)
        {
            _logger.LogWarning(
                "Get or create conversation failed. Other user not found. CurrentUserId: {CurrentUserId}, OtherUserId: {OtherUserId}",
                currentUserId,
                otherUserId);
            return ApiResponse<ConversationDetailDto>.Fail(404, "User not found");
        }

        var conversation = await _unitOfWork.ConversationRepository
            .GetConversationBetweenUsersAsync(currentUserId, otherUserId);

        if (conversation == null)
        {
            conversation = new Conversation
            {
                ParticipantOneId = currentUserId,
                ParticipantTwoId = otherUserId
            };
            await _unitOfWork.ConversationRepository.AddAsync(conversation);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "New conversation created via get-or-create. ConversationId: {ConversationId}, CurrentUserId: {CurrentUserId}, OtherUserId: {OtherUserId}",
                conversation.Id,
                currentUserId,
                otherUserId);
        }

        return await GetConversationAsync(conversation.Id, currentUserId);
    }

    public async Task<ApiResponse<bool>> MarkMessagesAsReadAsync(int conversationId, string userId)
    {
        var conversation = await _unitOfWork.ConversationRepository.GetByIdAsync(conversationId);
        
        if (conversation == null)
        {
            _logger.LogWarning(
                "Mark messages as read failed. Conversation not found. ConversationId: {ConversationId}, UserId: {UserId}",
                conversationId,
                userId);
            return ApiResponse<bool>.Fail(404, "Conversation not found");
        }

        // Ensure user is part of this conversation
        if (conversation.ParticipantOneId != userId && conversation.ParticipantTwoId != userId)
        {
            _logger.LogWarning(
                "Mark messages as read failed. Unauthorized access attempt. ConversationId: {ConversationId}, UserId: {UserId}, ParticipantOneId: {ParticipantOneId}, ParticipantTwoId: {ParticipantTwoId}",
                conversationId,
                userId,
                conversation.ParticipantOneId,
                conversation.ParticipantTwoId);
            return ApiResponse<bool>.Fail(403, "You are not part of this conversation");
        }

        await _unitOfWork.MessageRepository.MarkMessagesAsReadAsync(conversationId, userId);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "Messages marked as read successfully. ConversationId: {ConversationId}, UserId: {UserId}",
            conversationId,
            userId);

        // TODO: notify the other participant that messages were read

        return ApiResponse<bool>.Success(true);
    }

    public async Task<ApiResponse<int>> GetUnreadMessageCountAsync(string userId)
    {
        var count = await _unitOfWork.MessageRepository.GetUnreadMessageCountAsync(userId);

        _logger.LogInformation(
            "Unread message count retrieved successfully. UserId: {UserId}, UnreadCount: {UnreadCount}",
            userId,
            count);

        return ApiResponse<int>.Success(count);
    }
}
