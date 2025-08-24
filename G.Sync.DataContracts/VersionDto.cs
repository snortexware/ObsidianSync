namespace G.Sync.DataContracts
{
    /// <summary>
    /// Data transfer object for version information.
    /// </summary>
    public class VersionDto
    {
        public required string NAME { get; set; }
        public int STATUS { get; set; }
        public DateTime APPLIEDON { get; set; }
        public DateTime CREATETIME { get; set; }

        public enum StatusTypes
        {
            Pending = 0,
            Applied = 1,
            Failed = 2
        }
    }
}
