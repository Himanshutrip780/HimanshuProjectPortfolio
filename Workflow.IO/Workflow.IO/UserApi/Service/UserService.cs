using AutoMapper;
using JwtAuthenticationManager.Model;
using JwtAuthenticationManager.Repository;
using UserApi.Model.Domian.Entities;
using UserApi.Model.Dto;
using UserApi.Repositories;
using Workflow.IO.Shared.Exceptions;
using Workflow.IO.Shared.Contracts;
using Workflow.IO.Shared.IntegrationEvents;

namespace UserApi.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserAccountRepository _userAccountRepository;
        private readonly IOrganizationRepository _organizationRepository;

        // â Œ REMOVED
        // private readonly IUserService _userService;
        // WHY:
        // This created circular dependency
        // Service should NEVER depend on itself

        //private readonly DbContext _context;
        //Your Service layer should NOT know about: EF Core DbContext persistence details
        //That responsibility belongs to Repository layer

        private readonly IMapper _mapper;
        private readonly ITenantContext _tenantContext;
        private readonly IIntegrationEventPublisher _integrationEventPublisher;

        public UserService(IUserRepository userRepository, IUserAccountRepository userAccountRepository, IOrganizationRepository organizationRepository, IMapper mapper, ITenantContext tenantContext, IIntegrationEventPublisher integrationEventPublisher)
        {
            _userRepository = userRepository;
            _userAccountRepository = userAccountRepository;
            _organizationRepository = organizationRepository;
            _mapper = mapper;
            _tenantContext = tenantContext;
            _integrationEventPublisher = integrationEventPublisher;
        }

        private async Task<UserDto> MapToUserDtoAsync(User user)
        {
            var dto = _mapper.Map<UserDto>(user);
            var account = await _userAccountRepository.GetByIdAsync(user.UserId);
            if (account != null)
            {
                dto.Email = account.Email;
                dto.Role = account.Role.ToString();
            }
            return dto;
        }

        public async Task<UserDto> RegisterUserAsync(RegisterUserRequestDTO request)
        {
            var exists = await _userAccountRepository.ExistsByEmailAsync(request.Email);
            if (exists)
            {
                throw new ConflictException("Email already exists");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 10);
            var userAccount = new UserAccount(request.Email, passwordHash);
            await _userAccountRepository.CreateAsync(userAccount);

            var user = new User(
                userAccount.Id,
                request.FirstName,
                request.LastName
            );
            await _userRepository.RegisterUserAsync(user);

            // Domain-Based Organization Setup
            var domain = request.Email.Split('@').Last().ToLowerInvariant();
            
            // List of common public domains
            var publicDomains = new HashSet<string> { "gmail.com", "yahoo.com", "hotmail.com", "outlook.com", "live.com", "icloud.com", "aol.com" };
            
            string orgName = domain;
            if (publicDomains.Contains(domain))
            {
                // Create a personal workspace
                orgName = $"{request.FirstName} {request.LastName}'s Workspace";
            }
            else
            {
                // Try to get company name dynamically from Clearbit API
                try
                {
                    using var httpClient = new HttpClient();
                    // Set a timeout so we don't block registration for too long if the API is slow
                    httpClient.Timeout = TimeSpan.FromSeconds(3);
                    
                    var response = await httpClient.GetAsync($"https://autocomplete.clearbit.com/v1/companies/suggest?query={domain}");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        using var doc = System.Text.Json.JsonDocument.Parse(content);
                        if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                        {
                            var firstHit = doc.RootElement[0];
                            if (firstHit.TryGetProperty("name", out var nameElement) && !string.IsNullOrWhiteSpace(nameElement.GetString()))
                            {
                                orgName = nameElement.GetString();
                            }
                        }
                    }
                }
                catch
                {
                    // Fallback to domain if API call fails
                    orgName = domain;
                }
            }

            var isNewOrg = false;
            var org = await _organizationRepository.GetOrganizationByNameAsync(orgName);
            if (org == null)
            {
                isNewOrg = true;
                org = new Organization
                {
                    Name = orgName,
                    Subdomain = orgName.Replace(" ", "").Replace("'", "").ToLowerInvariant(),
                    InviteCode = Guid.NewGuid().ToString().Substring(0, 6).ToUpper()
                };
                await _organizationRepository.CreateOrganizationAsync(org);
            }

            await _userRepository.AssignOrganizationAsync(user.UserId, org.OrganizationId);

            if (isNewOrg)
            {
                var workspaceId = Guid.NewGuid();
                var clientId = Guid.NewGuid();
                var projectId = Guid.NewGuid();
                var teamId = Guid.NewGuid();

                var eventPayload = new
                {
                    OrganizationId = org.OrganizationId,
                    OrganizationName = org.Name,
                    AdminUserId = user.UserId,
                    AdminEmail = userAccount.Email,
                    WorkspaceId = workspaceId,
                    ClientId = clientId,
                    ProjectId = projectId,
                    TeamId = teamId
                };

                var integrationEvent = new IntegrationEventRequest
                {
                    EventId = Guid.NewGuid(),
                    OccurredAtUtc = DateTime.UtcNow,
                    EventType = "TenantRegistered",
                    EntityType = "Organization",
                    EntityId = org.OrganizationId,
                    ActorId = user.UserId,
                    Description = $"Auto-provisioning workspace structures for organization '{org.Name}'",
                    PayloadJson = System.Text.Json.JsonSerializer.Serialize(eventPayload, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase })
                };

                await _integrationEventPublisher.PublishAsync(integrationEvent);
            }

            var dto = _mapper.Map<UserDto>(user);
            dto.Email = userAccount.Email;
            dto.Role = userAccount.Role.ToString();
            return dto;
        }

        // ✅ CHANGED
        // Returning DTO collection instead of Entities
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var dtos = new List<UserDto>();
            foreach (var user in users)
            {
                dtos.Add(await MapToUserDtoAsync(user));
            }
            return dtos;
        }

        // ✅ CHANGED
        // Returning DTO instead of Entity
        public async Task<UserDto?> GetUserByIdAsync(Guid userId)
        {
            User? user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            if (_tenantContext.CurrentOrganizationId.HasValue && user.OrganizationId != _tenantContext.CurrentOrganizationId.Value)
            {
                return null;
            }

            return await MapToUserDtoAsync(user);
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var account =
                await _userAccountRepository.GetByEmailAsync(
                    email.Trim().ToLower());

            if (account == null)
            {
                return null;
            }

            return await GetUserByIdAsync(account.Id);
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(
            Guid userId,
            string? emailFallback = null)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            var account = await _userAccountRepository.GetByIdAsync(userId);

            if (user == null &&
                !string.IsNullOrWhiteSpace(emailFallback))
            {
                account =
                    await _userAccountRepository.GetByEmailAsync(
                        emailFallback.Trim().ToLower());

                if (account != null)
                {
                    user =
                        await _userRepository.GetUserByIdAsync(
                            account.Id);
                }
            }

            if (user == null)
            {
                return null;
            }

            if (_tenantContext.CurrentOrganizationId.HasValue && user.OrganizationId != _tenantContext.CurrentOrganizationId.Value)
            {
                return null;
            }

            account ??= await _userAccountRepository.GetByIdAsync(
                user.UserId);

            return new UserProfileDto
            {
                UserId = user.UserId,
                Email = account?.Email ?? emailFallback ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                AvatarUrl = user.AvatarUrl,
                Role = account?.Role.ToString() ?? "User",
                Status = user.Status,
                IsDeleted = user.IsDeleted,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
            };
        }

        public async Task<IEnumerable<UserLookupDto>> LookupUsersAsync(
            string emailQuery,
            Guid currentUserId)
        {
            if (string.IsNullOrWhiteSpace(emailQuery) ||
                emailQuery.Trim().Length < 2)
            {
                return Enumerable.Empty<UserLookupDto>();
            }

            var currentUser = await _userRepository.GetUserByIdAsync(currentUserId);
            if (currentUser == null)
            {
                return Enumerable.Empty<UserLookupDto>();
            }
            var targetOrgId = currentUser.OrganizationId;

            var results = new List<UserLookupDto>();
            var seenUserIds = new HashSet<Guid>();

            // 1. Search by email
            var accounts =
                await _userAccountRepository.SearchByEmailAsync(
                    emailQuery);

            foreach (var account in accounts)
            {
                var user = await _userRepository.GetUserByIdAsync(account.Id);
                if (user == null || user.OrganizationId != targetOrgId)
                {
                    continue;
                }

                results.Add(new UserLookupDto
                {
                    UserId = user.UserId,
                    Email = account.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                });
                seenUserIds.Add(user.UserId);
            }

            // 2. Search by name (FirstName or LastName matches)
            var nameMatches = await _userRepository.SearchUsersByNameAsync(emailQuery);
            foreach (var user in nameMatches)
            {
                if (user.OrganizationId != targetOrgId)
                {
                    continue;
                }
                if (seenUserIds.Contains(user.UserId))
                {
                    continue;
                }

                var account = await _userAccountRepository.GetByIdAsync(user.UserId);
                if (account == null)
                {
                    continue;
                }

                results.Add(new UserLookupDto
                {
                    UserId = user.UserId,
                    Email = account.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                });
                seenUserIds.Add(user.UserId);
            }

            return results;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            // ✅ Save profile entity directly

            await _userRepository.CreateUserAsync(user);

            return user;
        }

        // ✅ CHANGED SIGNATURE
        // Better API design using DTO request
        public async Task<UserDto?> UpdateUserAsync(Guid userId, RegisterUserRequestDTO request)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            if (_tenantContext.CurrentOrganizationId.HasValue && user.OrganizationId != _tenantContext.CurrentOrganizationId.Value)
            {
                throw new ForbiddenException("Cannot update user from another organization");
            }

            var updatedUser =
                await _userRepository.UpdateUserAsync(
                    userId,
                    request.FirstName,
                    request.LastName,
                    null);

            if (updatedUser == null)
            {
                return null;
            }

            return await MapToUserDtoAsync(updatedUser);
        }

        public async Task<UserDto?> UpdateProfileAsync(
            Guid userId,
            UpdateProfileRequestDto request)
        {
            var updatedUser =
                await _userRepository.UpdateUserAsync(
                    userId,
                    request.FirstName,
                    request.LastName,
                    request.AvatarUrl);

            if (updatedUser == null)
            {
                return null;
            }

            return await MapToUserDtoAsync(updatedUser);
        }

        public async Task ChangePasswordAsync(
            Guid userId,
            ChangePasswordRequestDto request)
        {
            var account =
                await _userAccountRepository.GetByIdAsync(userId);

            if (account == null)
            {
                throw new NotFoundException("User account not found");
            }

            if (!BCrypt.Net.BCrypt.Verify(
                    request.CurrentPassword,
                    account.PasswordHash))
            {
                throw new UnauthorizedAccessException(
                    "Current password is incorrect");
            }

            account.ChangePassword(
                BCrypt.Net.BCrypt.HashPassword(request.NewPassword, 10));

            await _userAccountRepository.UpdateAsync(account);
        }

        // ✅ CHANGED
        // Returning bool instead of entity
        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            if (_tenantContext.CurrentOrganizationId.HasValue && user.OrganizationId != _tenantContext.CurrentOrganizationId.Value)
            {
                throw new ForbiddenException("Cannot delete user from another organization");
            }

            User? deletedUser = await _userRepository.DeleteUserAsync(userId);

            if (deletedUser == null)
            {
                return false;
            }

            return true;
        }

        //public List<User> GetUsers()
        //{
        //    return _userRepository.GetUsers();
        //}
    }
}
