namespace Sorter.BigFiles.App;

public static class SemaphorePool
{
    public static int AvailableCores = Environment.ProcessorCount;
    public static Semaphore SemaphoreProcessing = new(AvailableCores, AvailableCores);
}