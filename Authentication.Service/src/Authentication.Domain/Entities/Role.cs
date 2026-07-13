namespace Authentication.Domain.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
