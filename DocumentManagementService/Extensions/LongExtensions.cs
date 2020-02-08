namespace DocumentManagementService.Extensions
{
    public static class LongExtensions
    {
        public static double ConvertBytesToMegabytes(this long sizeInBytes) => sizeInBytes * 0.001;
    }
}