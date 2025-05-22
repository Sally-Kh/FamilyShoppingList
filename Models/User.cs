using System.ComponentModel.DataAnnotations;

namespace FamilyShoppingList.Models
{
    // Diese Klasse repräsentiert einen Benutzer (User) in der Einkaufslisten-Anwendung.
    public class User
    {
        // Die ID des Benutzers (wird normalerweise automatisch von der Datenbank zugewiesen)
        public int Id { get; set; }

        // Der Name des Benutzers, der mit dem Required-Attribut als erforderlich markiert ist
        // Der Name kann nicht null sein, daher wird ein leerer String als Standardwert gesetzt
        [Required]
        public string Name { get; set; } = string.Empty;

        // Eine Sammlung (Liste) von Items, die der Benutzer noch kaufen möchte
        // Initialisiert als leere Liste, wenn kein Wert angegeben wird
        public ICollection<Item> ItemsToBuy { get; set; } = new List<Item>();

        // Eine Sammlung (Liste) von Items, die der Benutzer bereits gekauft hat
        // Initialisiert als leere Liste, wenn kein Wert angegeben wird
        public ICollection<Item> BoughtItems { get; set; } = new List<Item>();
    }
}
