namespace Motohut_API
{
    public class FirebaseStorageItem
    {
        public string? Name { get; set; }
        public long Size { get; set; }
        public string? ContentType { get; set; }
        public byte[]? Data { get; set; }
    }
}
