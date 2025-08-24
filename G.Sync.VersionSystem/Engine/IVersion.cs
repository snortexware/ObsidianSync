namespace G.Sync.VersionSystem.Engine
{
    public interface IVersion
    {
        public DateTime Date { get; }
        public void Run();
    }
}
