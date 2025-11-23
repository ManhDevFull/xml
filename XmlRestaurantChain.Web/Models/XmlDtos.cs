using System.Xml.Serialization;

namespace XmlRestaurantChain.Web.Models;

[XmlRoot("MenuCatalog")]
public class MenuExportXml
{
    [XmlElement("Restaurant")]
    public List<RestaurantMenuXml> Restaurants { get; set; } = new();
}

public class RestaurantMenuXml
{
    [XmlAttribute]
    public int Id { get; set; }

    [XmlAttribute]
    public string Name { get; set; } = string.Empty;

    [XmlElement("Category")]
    public List<MenuCategoryXml> Categories { get; set; } = new();
}

public class MenuCategoryXml
{
    [XmlAttribute]
    public string Name { get; set; } = string.Empty;

    [XmlElement("Item")]
    public List<MenuItemXml> Items { get; set; } = new();
}

public class MenuItemXml
{
    [XmlAttribute]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute]
    public decimal Price { get; set; }

    [XmlElement]
    public string? Description { get; set; }
}

[XmlRoot("ReservationFeed")]
public class ReservationExportXml
{
    [XmlElement("Reservation")]
    public List<ReservationXml> Reservations { get; set; } = new();
}

public class ReservationXml
{
    [XmlAttribute]
    public int Id { get; set; }

    [XmlAttribute]
    public string CustomerName { get; set; } = string.Empty;

    [XmlAttribute]
    public string Phone { get; set; } = string.Empty;

    [XmlAttribute]
    public DateTime ReservedAt { get; set; }

    [XmlAttribute]
    public int PartySize { get; set; }

    [XmlAttribute]
    public string Status { get; set; } = string.Empty;

    [XmlAttribute]
    public string Restaurant { get; set; } = string.Empty;

    [XmlAttribute]
    public string Table { get; set; } = string.Empty;

    [XmlElement]
    public string? Notes { get; set; }
}

[XmlRoot("InventorySnapshot")]
public class InventoryExportXml
{
    [XmlElement("Item")]
    public List<InventoryItemXml> Items { get; set; } = new();
}

public class InventoryItemXml
{
    [XmlAttribute]
    public string Name { get; set; } = string.Empty;

    [XmlAttribute]
    public string Unit { get; set; } = string.Empty;

    [XmlAttribute]
    public decimal Quantity { get; set; }

    [XmlAttribute]
    public decimal ReorderLevel { get; set; }

    [XmlAttribute]
    public string Restaurant { get; set; } = string.Empty;

    [XmlAttribute]
    public string Supplier { get; set; } = string.Empty;
}

[XmlRoot("MenuImport")]
public class MenuImportXml
{
    [XmlAttribute]
    public int RestaurantId { get; set; }

    [XmlElement("Category")]
    public List<MenuCategoryXml> Categories { get; set; } = new();
}
