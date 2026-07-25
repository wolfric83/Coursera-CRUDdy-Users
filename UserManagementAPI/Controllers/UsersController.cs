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

    public UsersController(UserDbContext context)
    {
        _context = context;
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
        await _context.SaveChangesAsync(cancellationToken);

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
        await _context.SaveChangesAsync(cancellationToken);

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
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private async Task<bool> UserExists(int id, CancellationToken cancellationToken)
    {
        return await _context.Users.AnyAsync(e => e.Id == id, cancellationToken);
    }
}
