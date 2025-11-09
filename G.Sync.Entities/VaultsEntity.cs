namespace G.Sync.Entities
{
    public class VaultsEntity
    {
        public long Id { get; private set; }
        public string Name { get; private set; }
        public string Path { get; private set; }
        public long Timestamp { get; private set; }
        public bool Open { get; private set; }

        public VaultsEntity(string path, long timestamp, bool open, string name)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Vault path cannot be empty.", nameof(path));

            Path = path;
            Timestamp = timestamp;
            Open = open;
            Name = name;
        }

        public void SetOpen(bool open)
        {
            Open = open;
        }
    }
}
