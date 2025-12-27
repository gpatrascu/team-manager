namespace api.Application.Abstractions;

// Base marker interface for requests
public interface IRequest<TResponse> { }

// Handler interface for requests that return a value
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken = default);
}

// Handler interface for requests that don't return a value (void)
public interface IRequestHandler<in TRequest>
    where TRequest : IRequest<Unit>
{
    Task Handle(TRequest request, CancellationToken cancellationToken = default);
}

// Unit type for void returns
public readonly struct Unit
{
    public static readonly Unit Value = new();
}
