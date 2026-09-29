using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwap.Application.Common.Interfaces;
using SkillSwap.Application.Common.Models;
using SkillSwap.Application.DTOs;
using SkillSwap.Domain.Entities;
using SkillSwap.Domain.Enums;

namespace SkillSwap.Application.Features.Notifications;

public record GetNotificationsQuery(bool? UnreadOnly, int PageNumber = 1, int PageSize = 20) : IRequest<Result<PaginatedList<NotificationDTO>>>;
