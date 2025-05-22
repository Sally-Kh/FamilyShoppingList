using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.Models
{
    // Diese Klasse repräsentiert ein Item (Produkt), das in der Einkaufslisten-Anwendung verwendet wird.
    public class Item
    {
        // Die eindeutige ID des Items (wird normalerweise automatisch von der Datenbank zugewiesen)
        public int Id { get; set; }

        // Der Name des Items, der mit dem Required-Attribut als erforderlich markiert ist
        // Der Name kann nicht null sein, daher wird ein leerer String als Standardwert gesetzt
        [Required]
        public string Name { get; set; } = string.Empty;

        // Die Menge des Items, die standardmäßig auf 1 gesetzt wird
        public int Quantity { get; set; } = 1;

        // Die ID des zugewiesenen Benutzers (falls vorhanden)
        // Ein Item kann einem Benutzer zugewiesen sein, aber dies ist optional
        public int? AssignedUserId { get; set; }

        // Der Benutzer, dem dieses Item zugewiesen wurde (optional)
        // Wenn ein Item einem Benutzer zugewiesen ist, enthält es das zugehörige User-Objekt
        public User? AssignedUser { get; set; }

        // Die ID des Käufers (falls vorhanden)
        // Ein Item kann einem Käufer zugewiesen sein, aber dies ist optional
        public int? BuyerId { get; set; }

        // Der Käufer des Items (optional)
        // Wenn ein Item gekauft wurde, enthält es das zugehörige User-Objekt des Käufers
        public User? Buyer { get; set; }

        // Das Datum, an dem das Item gekauft wurde (optional)
        // Wenn das Item gekauft wurde, enthält es das Kaufdatum
        public DateTime? BoughtDate { get; set; }
    }
}
