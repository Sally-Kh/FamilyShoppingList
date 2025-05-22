using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FamilyShoppingList.Models;
using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.Controllers
{
    // Basis-Controller mit API-Attributen
    [ApiController]
    [Route("api/[controller]")] // Basis-Route: /api/items
    public class ItemsController : ControllerBase
    {
        // Datenbankkontext und Logger für Dependency Injection
        private readonly AppDbContext _context;
        private readonly ILogger<ItemsController> _logger;

        // Konstruktor mit Dependency Injection
        public ItemsController(AppDbContext context, ILogger<ItemsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/items - Holt alle Einträge
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemResponse>>> GetItems()
        {
            try
            {
                return await _context.Items
                    .AsNoTracking() // Nur lesender Zugriff
                    .Include(i => i.AssignedUser) // Lädt zugewiesenen Benutzer
                    .Include(i => i.Buyer) // Lädt Käufer
                    .Select(i => new ItemResponse // Projektion auf Response-Objekt
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Quantity = i.Quantity,
                        AssignedUserId = i.AssignedUserId,
                        AssignedUserName = i.AssignedUser != null ? i.AssignedUser.Name : null,
                        BuyerId = i.BuyerId,
                        BuyerName = i.Buyer != null ? i.Buyer.Name : null,
                        BoughtDate = i.BoughtDate,
                        Status = i.BoughtDate.HasValue ? "Bought" : "ToBuy"
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Abrufen der Artikel");
                return StatusCode(500, "Interner Serverfehler");
            }
        }

        // POST: api/items - Erstellt neuen Eintrag
        [HttpPost]
        public async Task<ActionResult<ItemResponse>> CreateItem([FromBody] ItemRequest request)
        {
            // Validierung der Eingabedaten
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Neues Item-Objekt erstellen
                var item = new Item
                {
                    Name = request.Name!,
                    Quantity = request.Quantity,
                    AssignedUserId = request.AssignedUserId
                };

                // Prüft ob zugewiesener Benutzer existiert
                if (request.AssignedUserId.HasValue && 
                    !await _context.Users.AnyAsync(u => u.Id == request.AssignedUserId))
                {
                    return BadRequest("Ungültige Benutzerzuweisung");
                }

                _context.Items.Add(item);
                await _context.SaveChangesAsync();

                // Gibt den erstellten Eintrag zurück
                var response = await GetItemResponse(item.Id);
                return CreatedAtAction(nameof(GetItems), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Erstellen des Artikels");
                return StatusCode(500, "Interner Serverfehler");
            }
        }

        // PUT: api/items/{id}/mark-bought - Markiert Artikel als gekauft
        [HttpPut("{id}/mark-bought")]
        public async Task<ActionResult<ItemResponse>> MarkAsBought(int id, [FromBody] MarkBoughtRequest request)
        {
            try
            {
                // Artikel suchen
                var item = await _context.Items.FindAsync(id);
                if (item == null)
                {
                    return NotFound();
                }

                // Prüft ob Käufer existiert
                if (!await _context.Users.AnyAsync(u => u.Id == request.BuyerId))
                {
                    return BadRequest("Ungültiger Käufer");
                }

                // Aktualisiert Kaufinformationen
                item.BuyerId = request.BuyerId;
                item.BoughtDate = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                return await GetItemResponse(item.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fehler beim Markieren des Artikels {id} als gekauft");
                return StatusCode(500, "Interner Serverfehler");
            }
        }

        // DELETE: api/items/{id} - Löscht einen Artikel
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            try
            {
                // Artikel suchen
                var item = await _context.Items.FindAsync(id);
                if (item == null)
                {
                    return NotFound();
                }

                _context.Items.Remove(item);
                await _context.SaveChangesAsync();

                return NoContent(); // 204-Erfolgsstatus
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Fehler beim Löschen des Artikels {id}");
                return StatusCode(500, "Interner Serverfehler");
            }
        }

        // Hilfsmethode: Erstellt ein ItemResponse-Objekt
        private async Task<ItemResponse> GetItemResponse(int id)
        {
            var item = await _context.Items
                .Where(i => i.Id == id)
                .Include(i => i.AssignedUser)
                .Include(i => i.Buyer)
                .FirstAsync();

            return new ItemResponse
            {
                Id = item.Id,
                Name = item.Name,
                Quantity = item.Quantity,
                AssignedUserId = item.AssignedUserId,
                AssignedUserName = item.AssignedUser?.Name,
                BuyerId = item.BuyerId,
                BuyerName = item.Buyer?.Name,
                BoughtDate = item.BoughtDate,
                Status = item.BoughtDate.HasValue ? "Bought" : "ToBuy"
            };
        }
    }

    // Request-Modell für Artikel-Erstellung
    public class ItemRequest
    {
        [Required(ErrorMessage = "Name ist erforderlich")]
        [StringLength(100, ErrorMessage = "Name darf maximal 100 Zeichen haben")]
        public string? Name { get; set; }
        
        [Range(1, 100, ErrorMessage = "Menge muss zwischen 1 und 100 liegen")]
        public int Quantity { get; set; } = 1;
        
        public int? AssignedUserId { get; set; } // Optional: Zugewiesener Benutzer
    }

    // Request-Modell für Kauf-Markierung
    public class MarkBoughtRequest
    {
        [Required(ErrorMessage = "Käufer-ID ist erforderlich")]
        public int BuyerId { get; set; }
    }

    // Response-Modell für Artikel
    public class ItemResponse
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public int? AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }
        public int? BuyerId { get; set; }
        public string? BuyerName { get; set; }
        public DateTime? BoughtDate { get; set; }
        public string? Status { get; set; } // "Bought" oder "ToBuy"
    }
}