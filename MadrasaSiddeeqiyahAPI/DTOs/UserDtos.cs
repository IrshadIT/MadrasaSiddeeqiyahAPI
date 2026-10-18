namespace MadrasaSiddeeqiyahAPI.DTOs
{
    public class UserReadDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UserCreateDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";
    }

    public class UserUpdateDto
    {
        public string? Password { get; set; }
        public string? Role { get; set; }
    }
}
