using System;
using UnityEngine;

public class GrpcLoger : Grpc.Core.Logging.ILogger
{
    Grpc.Core.Logging.ILogger Grpc.Core.Logging.ILogger.ForType<T>()
    {
        return this;
    }

    void Grpc.Core.Logging.ILogger.Debug(string message)
    {
        Debug.LogWarning(message);
    }

    void Grpc.Core.Logging.ILogger.Debug(string format, params object[] formatArgs)
    {
        Debug.LogWarning(string.Format(format, formatArgs));

    }

    void Grpc.Core.Logging.ILogger.Info(string message)
    {
        Debug.Log(message);
    }

    void Grpc.Core.Logging.ILogger.Info(string format, params object[] formatArgs)
    {
        Debug.Log(string.Format(format, formatArgs));
    }

    void Grpc.Core.Logging.ILogger.Warning(string message)
    {
        Debug.LogWarning(message);
    }

    void Grpc.Core.Logging.ILogger.Warning(string format, params object[] formatArgs)
    {
        Debug.LogWarning(string.Format(format, formatArgs));
    }

    void Grpc.Core.Logging.ILogger.Warning(Exception exception, string message)
    {
        Debug.LogException(exception);
    }

    void Grpc.Core.Logging.ILogger.Error(string message)
    {
        Debug.LogError(message);
    }

    void Grpc.Core.Logging.ILogger.Error(string format, params object[] formatArgs)
    {
        Debug.LogErrorFormat(format, formatArgs);
    }

    void Grpc.Core.Logging.ILogger.Error(Exception exception, string message)
    {
        Debug.LogException(exception);
    }

}

