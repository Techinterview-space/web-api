﻿using System.Collections.Generic;
using Domain.ValueObjects.Pagination;
using Web.Api.Features.Github.Dtos;

namespace Web.Api.Features.Github.GetGithubProfileChats;

#pragma warning disable SA1313
public record GetGithubProfileChatsResponse(
    int CurrentPage,
    int PageSize,
    int TotalItems,
    IReadOnlyCollection<GithubProfileBotChatDto> Results)
    : Pageable<GithubProfileBotChatDto>(CurrentPage, PageSize, TotalItems, Results);