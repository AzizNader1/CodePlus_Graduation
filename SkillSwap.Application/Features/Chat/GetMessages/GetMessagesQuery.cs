using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Chat;

public record GetMessagesQuery(Guid ConversationId, int PageNumber = 1, int PageSize = 30) : IRequest<Result<PaginatedList<MessageDTO>>>;
