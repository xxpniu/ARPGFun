using System;
using UnityEngine;
using ILogger = Grpc.Core.Logging.ILogger;

public class GrpcLoger : ILogger
{
    ILogger ILogger.ForType<T>()
    {
        return this;
    }

    void ILogger.Debug(string message)
    {
        Debug.LogWarning(message);
    }

    void ILogger.Debug(string format, params object[] formatArgs)
    {
        Debug.LogWarning(string.Format(format, formatArgs));
    }

    void ILogger.Info(string message)
    {
        Debug.Log(message);
    }

    void ILogger.Info(string format, params object[] formatArgs)
    {
        Debug.Log(string.Format(format, formatArgs));
    }

    void ILogger.Warning(string message)
    {
        Debug.LogWarning(message);
    }

    void ILogger.Warning(string format, params object[] formatArgs)
    {
        Debug.LogWarning(string.Format(format, formatArgs));
    }

    void ILogger.Warning(Exception exception, string message)
    {
        Debug.LogException(exception);
    }

    void ILogger.Error(string message)
    {
        Debug.LogError(message);
    }

    void ILogger.Error(string format, params object[] formatArgs)
    {
        Debug.LogErrorFormat(format, formatArgs);
    }

    void ILogger.Error(Exception exception, string message)
    {
        Debug.LogException(exception);
    }
}