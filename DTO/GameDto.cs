
namespace XCI.Explorer.DTO
{
    public class GameDto
    {
        public string FilePath { get; set; }
        public string ExactSize { get; set; }
        public string Size { get; set; }
        public string UsedSpace { get; set; }
        public string Capacity { get; set; }
        public double UsedSize { get; set; }
        public long[] SecureSize { get; set; }
        public long[] SecureOffset { get; set; }
        public long[] NormalSize { get; set; }
        public long[] NormalOffset { get; set; }
        public string ExactUsedSpace { get; set; }
    }
}
