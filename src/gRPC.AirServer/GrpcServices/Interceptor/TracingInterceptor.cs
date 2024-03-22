using Grpc.Core;
using Grpc.Core.Interceptors;

namespace gRPC.Server.GrpcServices.Interceptor;

internal class TracingInterceptor(ILogger<TracingInterceptor> logger) : Grpc.Core.Interceptors.Interceptor
{
    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        logger.LogInformation("Record request to '{serviceName}' and method '{methodName}'", context.Method.ServiceName, context.Method.Name);
        return base.AsyncUnaryCall(request, context, continuation);
    }

    public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context,
        BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
    {
        logger.LogInformation("Record request to '{serviceName}' and method '{methodName}'", context.Method.ServiceName, context.Method.Name);
        return base.BlockingUnaryCall(request, context, continuation);
    }

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        logger.LogInformation("Record request to method '{methodName}' from '{peer}'", context.Method, context.Peer);
        return base.UnaryServerHandler(request, context, continuation);
    }

    public override AsyncClientStreamingCall<TRequest, TResponse> AsyncClientStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context,
        AsyncClientStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        logger.LogInformation("Record client-streaming request to '{serviceName}' and method '{methodName}'", context.Method.ServiceName, context.Method.Name);
        return base.AsyncClientStreamingCall(context, continuation);
    }

    public override AsyncDuplexStreamingCall<TRequest, TResponse> AsyncDuplexStreamingCall<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context,
        AsyncDuplexStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        logger.LogInformation("Record duplex-streaming request to '{serviceName}' and method '{methodName}'", context.Method.ServiceName, context.Method.Name);
        return base.AsyncDuplexStreamingCall(context, continuation);
    }

    public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
    {
        logger.LogInformation("Record server-streaming request to '{serviceName}' and method '{methodName}'", context.Method.ServiceName, context.Method.Name);
        return base.AsyncServerStreamingCall(request, context, continuation);
    }

    public override Task<TResponse> ClientStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext context,
        ClientStreamingServerMethod<TRequest, TResponse> continuation)
    {
        logger.LogInformation("Record client-streaming request to method '{methodName}' from '{peer}'", context.Method, context.Peer);
        return base.ClientStreamingServerHandler(requestStream, context, continuation);
    }

    public override Task DuplexStreamingServerHandler<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream,
        IServerStreamWriter<TResponse> responseStream, ServerCallContext context, DuplexStreamingServerMethod<TRequest, TResponse> continuation)
    {
        logger.LogInformation("Record duplex-streaming request to method '{methodName}' from '{peer}'", context.Method, context.Peer);
        return base.DuplexStreamingServerHandler(requestStream, responseStream, context, continuation);
    }

    public override Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream,
        ServerCallContext context, ServerStreamingServerMethod<TRequest, TResponse> continuation)
    {
        logger.LogInformation("Record server-streaming request to method '{methodName}' from '{peer}'", context.Method, context.Peer);
        return base.ServerStreamingServerHandler(request, responseStream, context, continuation);
    }
}