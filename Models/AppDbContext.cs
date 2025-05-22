using Microsoft.EntityFrameworkCore;

namespace FamilyShoppingList.Models
{
    // Haupt-Datenbankkontext für die Anwendung
    // Erbt von Entity Framework Core's DbContext
    public class AppDbContext : DbContext
    {
        // DbSet Properties definieren die Tabellen in der Datenbank:
        
        // Tabelle für Benutzer/User
        public DbSet<User> Users { get; set; }
        
        // Tabelle für Einkaufsartikel/Items
        public DbSet<Item> Items { get; set; }

        // Konstruktor mit Dependency Injection der Konfigurationsoptionen
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) 
        { 
            // Basis-Konstruktor wird mit den Optionen aufgerufen
        }

        // Wird aufgerufen wenn das Datenbankmodell erstellt wird
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguration der Beziehung zwischen Item und AssignedUser:
            modelBuilder.Entity<Item>()
                .HasOne(i => i.AssignedUser)       // Ein Artikel hat einen zugewiesenen Benutzer
                .WithMany(u => u.ItemsToBuy)       // Ein Benutzer kann viele Artikel zugewiesen bekommen
                .HasForeignKey(i => i.AssignedUserId) // Fremdschlüssel ist AssignedUserId
                .OnDelete(DeleteBehavior.SetNull);  // Bei Löschung wird Fremdschlüssel auf NULL gesetzt

            // Konfiguration der Beziehung zwischen Item und Buyer:
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Buyer)              // Ein Artikel hat einen Käufer
                .WithMany(u => u.BoughtItems)      // Ein Benutzer kann viele Artikel gekauft haben
                .HasForeignKey(i => i.BuyerId)     // Fremdschlüssel ist BuyerId
                .OnDelete(DeleteBehavior.SetNull); // Bei Löschung wird Fremdschlüssel auf NULL gesetzt
        }
    }
}