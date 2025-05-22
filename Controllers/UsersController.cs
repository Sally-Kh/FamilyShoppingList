using Microsoft.AspNetCore.Mvc;
using FamilyShoppingList.Models;
using Microsoft.EntityFrameworkCore;

namespace FamilyShoppingList.Controllers
{
    // API-Controller für Benutzerverwaltung
    [ApiController]
    [Route("api/[controller]")] // Basisroute: /api/users
    public class UsersController : ControllerBase
    {
        // Datenbankkontext für Datenzugriff
        private readonly AppDbContext _context;

        // Konstruktor mit Dependency Injection des Datenbankkontexts
        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users - Holt alle Benutzer
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            // Gibt alle Benutzer als Liste zurück
            return await _context.Users.ToListAsync();
        }

        // POST: api/users - Erstellt einen neuen Benutzer
        [HttpPost]
        public async Task<ActionResult<User>> AddUser([FromBody] User user)
        {
            // Validierung: Name muss vorhanden sein
            if (string.IsNullOrEmpty(user.Name))
            {
                return BadRequest("Name ist erforderlich");
            }

            // Fügt neuen Benutzer hinzu
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Gibt den erstellten Benutzer mit Standort-Header zurück
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
        }

        // DELETE: api/users/{id} - Löscht einen Benutzer
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Sucht den Benutzer in der Datenbank
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(); // 404 wenn nicht gefunden
            }

            // Holt alle Artikel, die diesem Benutzer zugewiesen sind
            var items = await _context.Items
                .Where(i => i.AssignedUserId == id || i.BuyerId == id)
                .ToListAsync();

            // Entfernt Benutzerzuweisungen von Artikeln
            foreach (var item in items)
            {
                if (item.AssignedUserId == id) item.AssignedUserId = null;
                if (item.BuyerId == id) item.BuyerId = null;
            }

            // Löscht den Benutzer
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Erfolgsstatus ohne Inhalt (204 No Content)
            return NoContent();
        }
    }
}