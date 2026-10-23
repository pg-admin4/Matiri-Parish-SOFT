namespace MauiApp1.Models
{
    public enum UserRole
    {
        StandardUser = 0, // Can enter new records and search
        Secretary = 1,    // Can enter and edit recent records
        Admin = 2         // Full access (edit, delete, manage users)
    }
}