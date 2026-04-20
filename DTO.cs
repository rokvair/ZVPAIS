using System.ComponentModel.DataAnnotations;

namespace ŽVPAIS_API
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(6)]
        public string Password { get; set; }
        public bool IsSpecialist { get; set; }
        public string? Name { get; set; }
        public string? FieldOfExpertise { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public int UserId { get; set; }
    }

    public class UserResponseDto
    {
        public int IdUser { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? SpecialistName { get; set; }
        public string? FieldOfExpertise { get; set; }
    }


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

    public class EventObjectAssignDto
    {
        public int ObjectId { get; set; }
        public string? ComponentType { get; set; }
        public decimal? KKat { get; set; }
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
        public string Status { get; set; }
        public List<EventObjectAssignDto> EventObjects { get; set; }
        public List<IncidentMaterialDto> Materials { get; set; }
    }

    public class EventObjectResponseDto
    {
        public int IdObject { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? ComponentType { get; set; }
        public decimal? KKat { get; set; }
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
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public List<EventObjectResponseDto> Objects { get; set; }
        public List<IncidentMaterialDto> Materials { get; set; }
    }
    public class ObjectMaterialCreateDto
    {
        public int MaterialId { get; set; }
        public double? Percentage { get; set; }
        public double? Mass { get; set; }
        public double? Volume { get; set; }
        public double? RecoveredQuantity { get; set; }
    }
    public class ObjectDto
    {
        public int IdObject { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double? TotalMass { get; set; }
        public double? TotalVolume { get; set; }
        public List<ObjectMaterialDto> Materials { get; set; }
    }
    public class MaterialCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double? ToxicityFactor { get; set; }
        public string Unit { get; set; }
        // T_n: tariff per tonne from Methodology Table 1 (water/soil) or Table 3 (air)
        public decimal? BaseRate { get; set; }
        // "standard" | "bds7" | "suspended"
        public string? SubstanceType { get; set; }
    }

    public class MaterialResponseDto
    {
        public int IdMaterial { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double? ToxicityFactor { get; set; }
        public string Unit { get; set; }
        public decimal? BaseRate { get; set; }
        public string? SubstanceType { get; set; }
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
        public double? RecoveredQuantity { get; set; }
    }

    // --- Calculation breakdown DTOs ---

    public class MaterialDamageBreakdownDto
    {
        public int MaterialId { get; set; }
        public string? MaterialName { get; set; }
        public decimal TN { get; set; }       // tariff (EUR/t)
        public decimal IN { get; set; }       // indexing coefficient
        public decimal QN { get; set; }       // net quantity (emitted − recovered, t)
        public decimal KKat { get; set; }     // category coefficient
        public string? ComponentType { get; set; }
        public string? SubstanceType { get; set; }
        public decimal ZN { get; set; }             // computed damage for this material
        public decimal PollutionSize { get; set; }   // taršos dydis = T_n × Q_n × K_kat (without I_n)
    }

    public class ObjectDamageBreakdownDto
    {
        public int ObjectId { get; set; }
        public string? ObjectName { get; set; }
        public string? ComponentType { get; set; }
        public decimal? KKat { get; set; }
        public List<MaterialDamageBreakdownDto> Materials { get; set; } = [];
        public decimal ObjectDamage { get; set; }
        public decimal ObjectPollutionSize { get; set; }
    }

    public class EventDamageBreakdownDto
    {
        public int EventId { get; set; }
        public decimal IndexingCoefficient { get; set; }
        public List<ObjectDamageBreakdownDto> Objects { get; set; } = [];
        public decimal TotalDamage { get; set; }
        public decimal TotalPollutionSize { get; set; }
    }

    // --- Pollution severity DTOs ---

    public class MaterialPollutionDto
    {
        public int MaterialId { get; set; }
        public string? MaterialName { get; set; }
        public double QN { get; set; }              // net quantity (t)
        public double? ToxicityFactor { get; set; }
        public decimal KKat { get; set; }
        public double SeverityContribution { get; set; } // Q_n × ToxFactor × K_kat
    }

    public class ObjectPollutionDto
    {
        public int ObjectId { get; set; }
        public string? ObjectName { get; set; }
        public string? ComponentType { get; set; }
        public decimal? KKat { get; set; }
        public List<MaterialPollutionDto> Materials { get; set; } = [];
        public double ObjectSeverity { get; set; }
    }

    public class EventPollutionDto
    {
        public int EventId { get; set; }
        public List<ObjectPollutionDto> Objects { get; set; } = [];
        public double TotalSeverityIndex { get; set; }
    }

    public class IndexingCoefficientCreateDto
    {
        public int Year { get; set; }
        public int Quarter { get; set; }
        public decimal Coefficient { get; set; }
    }

    public class IndexingCoefficientDto
    {
        public int IdIndexingCoefficient { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public decimal Coefficient { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PdfRequestDto
    {
        public string? MapImageBase64 { get; set; }
    }

    public class ReportCreateDto
    {
        [Required]
        public int EventId { get; set; }
        public DateTime Data { get; set; }
        public double? ZalosDydis { get; set; }
        public double? PiniginisDydis { get; set; }
        public string? Notes { get; set; }
    }

    public class ReportResponseDto
    {
        public int IdDamageEvaluation { get; set; }
        public DateTime Data { get; set; }
        public double? ZalosDydis { get; set; }
        public double? PiniginisDydis { get; set; }
        public int EventId { get; set; }
        public string? EventType { get; set; }
        public DateTimeOffset EventDate { get; set; }
        public string? EventLocation { get; set; }
        public string? EventStatus { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}