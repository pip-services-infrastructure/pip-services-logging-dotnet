﻿using PipServices.Commons.Config;
using PipServices.Commons.Data;
using PipServices.Commons.Run;
using PipServices.Logging.Models;

using System.Threading.Tasks;

namespace PipServices.Logging.Memory
{
    public interface ILoggingMemoryPersistence : ICleanable, IConfigurable
    {
        Task<LogMessageV1[]> GetPageByFilterAsync(string correlationId, FilterParams filter, PagingParams paging);
        Task CreateAsync(string correlationId, LogMessageV1 message);
    }
}