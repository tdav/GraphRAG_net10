namespace GraphRAG.Domain.Entities.Core;

/// <summary>
/// User entity representing a system user (doctor, nurse, admin)
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// User email (unique)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// User role (Doctor, Nurse, Admin)
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Is user active
    /// </summary>
    public bool IsActive { get; set; }

    public User()
    {
        IsActive = true;
    }
}
