namespace G.Sync.Entities
{
    public class Settings(string folder)
    {
        public string Folder { get; private set; } = folder;

        public void UpdateFolder(string newFolder)
        {
            if (string.IsNullOrWhiteSpace(newFolder))
                throw new ArgumentException("Folder cannot be empty.");

            Folder = newFolder;
        }
    }
}
