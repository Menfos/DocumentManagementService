// <copyright file="IServiceLogger.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;

namespace DocumentManagementService.Logger
{
    public interface IServiceLogger
    {
        void LogInfo(string message);

        void LogWarning(string message, Exception exception = null);

        void LogError(string message, Exception exception = null);
    }
}