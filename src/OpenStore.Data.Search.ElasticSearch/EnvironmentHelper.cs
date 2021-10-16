using System;

namespace OpenStore.Data.Search.ElasticSearch;

internal class EnvironmentHelper
{
    public static int ProcessorCount => Math.Max(GetProcessorCount(Environment.ProcessorCount), 2);

    private static int GetProcessorCount(int processorCount)
    {
        if (InDocker)
            return (processorCount / 4) * 2 - 1;

        return processorCount;
    }

    private static bool InDocker => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
}