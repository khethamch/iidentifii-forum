namespace Forum.Application.Dtos;

public record Page<T>(IReadOnlyList<T> Items, int Total, int PageNumber, int PageSize);
