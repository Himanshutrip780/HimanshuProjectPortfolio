namespace UserApi.Model.Dto
{
    public class UserLookupDto
    {
        public Guid UserId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
    }
}
