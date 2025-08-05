using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Ledon.BerryNet.Grpc.Extensions;

/// <summary>
/// gRPC 流式操作扩展方法
/// </summary>
public static class GrpcStreamExtensions
{
    /// <summary>
    /// 将异步流转换为可观察序列
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="stream">异步流</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步枚举</returns>
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IAsyncStreamReader<T> stream, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await stream.MoveNext(cancellationToken))
        {
            yield return stream.Current;
        }
    }

    /// <summary>
    /// 将异步流转换为列表
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="stream">异步流</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>消息列表</returns>
    public static async Task<List<T>> ToListAsync<T>(this IAsyncStreamReader<T> stream, 
        CancellationToken cancellationToken = default)
    {
        var result = new List<T>();
        while (await stream.MoveNext(cancellationToken))
        {
            result.Add(stream.Current);
        }
        return result;
    }

    /// <summary>
    /// 对流中的每个元素执行操作
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="stream">异步流</param>
    /// <param name="action">要执行的操作</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    public static async Task ForEachAsync<T>(this IAsyncStreamReader<T> stream, 
        Action<T> action, CancellationToken cancellationToken = default)
    {
        while (await stream.MoveNext(cancellationToken))
        {
            action(stream.Current);
        }
    }

    /// <summary>
    /// 对流中的每个元素执行异步操作
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="stream">异步流</param>
    /// <param name="action">要执行的异步操作</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    public static async Task ForEachAsync<T>(this IAsyncStreamReader<T> stream, 
        Func<T, Task> action, CancellationToken cancellationToken = default)
    {
        while (await stream.MoveNext(cancellationToken))
        {
            await action(stream.Current);
        }
    }

    /// <summary>
    /// 从流中获取第一个元素
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="stream">异步流</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>第一个元素</returns>
    public static async Task<T> FirstAsync<T>(this IAsyncStreamReader<T> stream, 
        CancellationToken cancellationToken = default)
    {
        if (await stream.MoveNext(cancellationToken))
        {
            return stream.Current;
        }
        throw new InvalidOperationException("流中没有元素");
    }

    /// <summary>
    /// 从流中获取第一个元素或默认值
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="stream">异步流</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>第一个元素或默认值</returns>
    public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncStreamReader<T> stream, 
        CancellationToken cancellationToken = default)
    {
        if (await stream.MoveNext(cancellationToken))
        {
            return stream.Current;
        }
        return default;
    }

    /// <summary>
    /// 计算流中的元素数量
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="stream">异步流</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>元素数量</returns>
    public static async Task<int> CountAsync<T>(this IAsyncStreamReader<T> stream, 
        CancellationToken cancellationToken = default)
    {
        int count = 0;
        while (await stream.MoveNext(cancellationToken))
        {
            count++;
        }
        return count;
    }

    /// <summary>
    /// 批量写入流
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="stream">异步流写入器</param>
    /// <param name="items">要写入的项目</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    public static async Task WriteAllAsync<T>(this IAsyncStreamWriter<T> stream, 
        IEnumerable<T> items, CancellationToken cancellationToken = default)
    {
        foreach (var item in items)
        {
            await stream.WriteAsync(item, cancellationToken);
        }
    }

    /// <summary>
    /// 带进度报告的流处理
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="stream">异步流</param>
    /// <param name="action">处理操作</param>
    /// <param name="progress">进度报告</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    public static async Task ProcessWithProgressAsync<T>(this IAsyncStreamReader<T> stream,
        Action<T> action, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        int processedCount = 0;
        while (await stream.MoveNext(cancellationToken))
        {
            action(stream.Current);
            processedCount++;
            progress?.Report(processedCount);
        }
    }

    /// <summary>
    /// 带错误处理的流处理
    /// </summary>
    /// <typeparam name="T">消息类型</typeparam>
    /// <param name="stream">异步流</param>
    /// <param name="action">处理操作</param>
    /// <param name="errorHandler">错误处理器</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务</returns>
    public static async Task ProcessWithErrorHandlingAsync<T>(this IAsyncStreamReader<T> stream,
        Func<T, Task> action, Func<Exception, T, Task<bool>>? errorHandler = null, 
        ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        while (await stream.MoveNext(cancellationToken))
        {
            try
            {
                await action(stream.Current);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "处理流消息时发生错误: {Message}", ex.Message);
                
                var shouldContinue = errorHandler != null ? await errorHandler(ex, stream.Current) : false;
                if (!shouldContinue)
                {
                    throw;
                }
            }
        }
    }
}
