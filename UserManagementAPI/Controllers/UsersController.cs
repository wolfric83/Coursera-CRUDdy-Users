using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementAPI.Data;
using UserManagementAPI.DTOs;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return users;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPost]
    public async Task<ActionResult<User>> PostUser([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        var normalizedEmail = dto.Email.Trim();
        var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail.ToLower(), cancellationToken);

        if (emailExists)
        {
            return Conflict(new { message = "A user with this email address already exists." });
        }

        var user = new User
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = normalizedEmail,
            Department = dto.Department.Trim()
        };

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to create user.");
            return HandleDatabaseWriteFailure();
        }

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(int id, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (existingUser == null)
        {
            return NotFound();
        }

        var normalizedEmail = dto.Email.Trim();
        var duplicateUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail.ToLower() && u.Id != id, cancellationToken);

        if (duplicateUser != null)
        {
            return Conflict(new { message = "A user with this email address already exists." });
        }

        existingUser.FirstName = dto.FirstName.Trim();
        existingUser.LastName = dto.LastName.Trim();
        existingUser.Email = normalizedEmail;
        existingUser.Department = dto.Department.Trim();

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to update user with ID {UserId}.", id);
            return HandleDatabaseWriteFailure();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to delete user with ID {UserId}.", id);
            return HandleDatabaseWriteFailure();
        }

        return NoContent();
    }

    private ObjectResult HandleDatabaseWriteFailure()
    {
        return Problem(
            title: "An unexpected error occurred while processing the request.",
            statusCode: StatusCodes.Status500InternalServerError);
    }

    private async Task<bool> UserExists(int id, CancellationToken cancellationToken)
    {
        return await _context.Users.AnyAsync(e => e.Id == id, cancellationToken);
    }
}
