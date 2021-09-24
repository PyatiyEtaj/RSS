namespace RevitService.Models
{
    public class GoogleRevitDocument
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public bool NeedToLoadLinks { get; set; }
        public string ObjectName { get; set; }

        public string Section { get; set; }
        public string Devide { get; set; }
        public string Information { get; set; }
    }
}
