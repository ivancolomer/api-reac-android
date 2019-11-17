namespace REAC_AndroidAPI.Entities
{
    public class User
    {
        public bool IsOwner { get; set; }
        public uint UserID { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string ProfilePhoto { get; set; }
    }
}
