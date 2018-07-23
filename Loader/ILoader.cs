namespace XCI.Explorer.Loader
{
    internal interface ILoader
    {
        void LoadRom(string filePath);
        void LoadPartitions(string filePath);
        void LoadNca();
        void LoadInfos();
    }
}