using System.ComponentModel.DataAnnotations;

namespace ŽVPAIS_API
{
    // DTO medžiagos priskyrimui prie įvykio
    public class IncidentMaterialDto
    {
        public int MaterialId { get; set; }
        public double? Percentage { get; set; }
        public double? Mass { get; set; }
        public double? Volume { get; set; }

        // Galime pridėti ir materialName, jei norime grąžinti frontendui
        public string MaterialName { get; set; }
    }

    public class EventCreateDto
    {
        [Required]
        public string EventType { get; set; } // 'gaisas', 'medžiagų išsiliejimas', 'stichija'
        [Required]
        public DateTimeOffset EventDate { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        [Required]
        public string Polygon { get; set; }
        public decimal? SensitivityFactor { get; set; }
        public string Status { get; set; }
        public List<int> ObjectIds { get; set; }
        // Pridedame medžiagų sąrašą (nebūtinas, jei nėra medžiagų)
        public List<IncidentMaterialDto> Materials { get; set; }
    }

    public class EventResponseDto
    {
        public int IdEvent { get; set; }
        public string EventType { get; set; }
        public DateTimeOffset EventDate { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Polygon { get; set; }
        public string Status { get; set; }
        public decimal? SensitivityFactor { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public List<ObjectDto> Objects { get; set; }
        // Pridedame medžiagų sąrašą atsakymui
        public List<IncidentMaterialDto> Materials { get; set; }
    }
    public class ObjectMaterialCreateDto
    {
        public int MaterialId { get; set; }
        public double? Percentage { get; set; }
        public double? Mass { get; set; }
        public double? Volume { get; set; }
    }
    public class ObjectDto
    {
        public int IdObject { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double? TotalMass { get; set; }   // pridėta
        public double? TotalVolume { get; set; } // pridėta
        public List<ObjectMaterialDto> Materials { get; set; }
    }
    public class MaterialCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double? ToxicityFactor { get; set; }
        public string Unit { get; set; }
        public decimal? BaseRate { get; set; }               // added
        public decimal? HarmfulnessFactor { get; set; }
    }

    public class MaterialResponseDto
    {
        public int IdMaterial { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double? ToxicityFactor { get; set; }
        public string Unit { get; set; }
        public decimal? BaseRate { get; set; }               // added
        public decimal? HarmfulnessFactor { get; set; }
        public DateTime CreatedAt { get; set; }

    }
    public class ObjectCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double? TotalMass { get; set; }
        public double? TotalVolume { get; set; }
    }
    public class ObjectMaterialDto
    {
        public int IdObjectMaterial { get; set; }
        public int MaterialId { get; set; }
        public string MaterialName { get; set; } // tik atsakymui
        public double? Percentage { get; set; }
        public double? Mass { get; set; }
        public double? Volume { get; set; }
    }
}