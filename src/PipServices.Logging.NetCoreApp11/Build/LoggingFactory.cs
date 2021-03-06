﻿using System;

using PipServices.Commons.Build;
using PipServices.Commons.Refer;
using PipServices.Logging.Logic;
using PipServices.Logging.Persistence;
using PipServices.Logging.Services;

namespace PipServices.Logging.Build
{
    public class LoggingFactory: IFactory
    {
        private static readonly Lazy<LoggingMemoryPersistence> LoggingPersistance = new Lazy<LoggingMemoryPersistence>();

        public object CanCreate(object locater)
        {
            var descriptor = locater as Descriptor;

            if (descriptor != null)
            {
                if (descriptor.Equals(Descriptors.LoggingMemoryPersistence))
                {
                    return true;
                }

                if (descriptor.Equals(Descriptors.LoggingController))
                {
                    return true;
                }

                if (descriptor.Equals(Descriptors.LoggingRestService))
                {
                    return true;
                }
            }

            return null;
        }

        public object Create(object locater)
        {
            var descriptor = locater as Descriptor;

            if (descriptor != null)
            {
                if (descriptor.Equals(Descriptors.LoggingMemoryPersistence))
                {
                    return LoggingPersistance.Value;
                }

                if (descriptor.Equals(Descriptors.LoggingController))
                {
                    return new LoggingController();
                }

                if (descriptor.Equals(Descriptors.LoggingRestService))
                {
                    return new LoggingHttpServiceV1();
                }
            }

            return null;
        }
    }
}
