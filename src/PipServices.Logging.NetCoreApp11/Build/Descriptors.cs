using PipServices.Commons.Refer;

namespace PipServices.Logging.Build
{
    public static class Descriptors
    {
        private const string Group = "pip-services-logging";

        public static Descriptor LoggingFactory { get; } = new Descriptor(Group, "factory", "default", "default", "1.0");

        public static Descriptor LoggingMemoryPersistence { get; } = new Descriptor(Group, "persistence", "memory", "logging", "1.0");

        public static Descriptor LoggingController { get; } = new Descriptor(Group, "controller", "default", "logging", "1.0");

        public static Descriptor LoggingRestService { get; } = new Descriptor(Group, "service", "rest", "logging", "1.0");
        public static Descriptor LoggingRestClient { get; } = new Descriptor(Group, "client", "rest", "logging", "1.0");
        public static Descriptor LoggingDirectClient { get; } = new Descriptor(Group, "client", "direct", "logging", "1.0");
        public static Descriptor LoggingClient { get; } = new Descriptor(Group, "client", "*", "logging", "*");
    }
}
