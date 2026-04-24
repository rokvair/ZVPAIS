using Microsoft.EntityFrameworkCore;
using ŽVPAIS_API.Data;

namespace ŽVPAIS_API.Services
{
    public interface IGaussianPlumeService
    {
        Task<List<WasteTypeListItemDto>> GetWasteTypesAsync();
        Task<DispersionResultDto> CalculateAsync(DispersionRequestDto request);
        Task<EventDispersionResultDto> CalculateFromEventAsync(int eventId, WindParamsDto wind);
    }

    public class GaussianPlumeService : IGaussianPlumeService
    {
        private readonly AppDbContext _context;

        public GaussianPlumeService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>Returns all waste types ordered by EWC code for the dispersion form selector.</summary>
        public async Task<List<WasteTypeListItemDto>> GetWasteTypesAsync()
        {
            return await _context.WasteTypes
                .OrderBy(w => w.EwcCode)
                .Select(w => new WasteTypeListItemDto
                {
                    Id = w.Id,
                    EwcCode = w.EwcCode,
                    Description = w.Description,
                    IsHazardous = w.IsHazardous,
                    IsCombustible = w.IsCombustible
                })
                .ToListAsync();
        }

        /// <summary>
        /// Calculates ground-level dispersion for a manually specified waste type and mass.
        /// Emission rate Q is derived from the waste type morphology fractions and compound emission factors.
        /// </summary>
        public async Task<DispersionResultDto> CalculateAsync(DispersionRequestDto req)
        {
            var wasteType = await _context.WasteTypes.FindAsync(req.WasteTypeId)
                ?? throw new ArgumentException($"WasteType {req.WasteTypeId} not found");

            var compounds = await _context.EmissionCompounds.ToListAsync();

            var morphology = wasteType.Morphology; // category to fraction (0-100)
            double durationS = Math.Max(req.FireDurationHours * 3600.0, 1.0);
            double u = Math.Max(req.WindSpeedMs, 0.5); // guard against division by zero

            var results = new List<CompoundDispersionDto>();

            foreach (var compound in compounds)
            {
                var ef = compound.Ef; // category to kg compound per tonne burned

                double Q = 0;
                foreach (var (category, morphPct) in morphology)
                {
                    if (ef.TryGetValue(category, out double efVal) && efVal > 0 && morphPct > 0)
                    {
                        double massInCategory = req.TotalMassTonnes * (morphPct / 100.0);
                        Q += massInCategory * efVal * 1000.0 / durationS; // g/s
                    }
                }

                if (Q < 1e-12) continue;

                results.Add(new CompoundDispersionDto
                {
                    CompoundId = compound.Id,
                    CompoundName = compound.Name,
                    BaseRate = compound.BaseRate,
                    EmissionRateGs = Q,
                    GridPoints = ComputeGrid(Q, u, req.StabilityClass, req.SourceHeightM)
                });
            }

            results.Sort((a, b) => b.EmissionRateGs.CompareTo(a.EmissionRateGs));

            return new DispersionResultDto
            {
                FireLat = req.FireLat,
                FireLon = req.FireLon,
                WindDirectionDeg = req.WindDirectionDeg,
                WindSpeedMs = u,
                StabilityClass = req.StabilityClass,
                Compounds = results
            };
        }

        /// <summary>
        /// Calculates dispersion for an existing event by aggregating combustible material masses
        /// from all linked event objects, grouped by emission category.
        /// </summary>
        public async Task<EventDispersionResultDto> CalculateFromEventAsync(int eventId, WindParamsDto wind)
        {
            var eventObj = await _context.Events
                .Include(e => e.EventObjects)
                    .ThenInclude(eo => eo.Object)
                        .ThenInclude(o => o.ObjectMaterials)
                            .ThenInclude(om => om.Material)
                .FirstOrDefaultAsync(e => e.IdEvent == eventId)
                ?? throw new ArgumentException($"Event {eventId} not found");

            // Build category-to-total-mass map and collect per-material details
            var categoryMass = new Dictionary<string, double>();
            var materialRows = new List<MaterialCategoryDto>();
            var uncategorized = new List<string>();
            var zeroQty = new List<string>();

            foreach (var eo in eventObj.EventObjects)
            {
                var obj = eo.Object;
                foreach (var om in obj.ObjectMaterials ?? [])
                {
                    if (om.Material == null) continue;
                    double qty = ResolveQuantity(om, obj);
                    if (qty <= 0)
                    {
                        zeroQty.Add(om.Material.Name);
                        continue;
                    }

                    var cat = om.Material.EmissionCategory?.ToLowerInvariant();
                    materialRows.Add(new MaterialCategoryDto
                    {
                        MaterialName   = om.Material.Name,
                        EmissionCategory = cat,
                        MassTonnes     = qty
                    });

                    if (!string.IsNullOrEmpty(cat))
                        categoryMass[cat] = categoryMass.GetValueOrDefault(cat) + qty;
                    else
                        uncategorized.Add(om.Material.Name);
                }
            }

            double totalMass = materialRows.Sum(m => m.MassTonnes);

            if (categoryMass.Count == 0)
                return new EventDispersionResultDto
                {
                    TotalMassTonnes = totalMass,
                    Materials = materialRows,
                    UncategorizedMaterials = uncategorized,
                    ZeroQuantityMaterials = zeroQty,
                    Dispersion = new DispersionResultDto
                    {
                        FireLat = wind.FireLat, FireLon = wind.FireLon,
                        WindDirectionDeg = wind.WindDirectionDeg,
                        WindSpeedMs = wind.WindSpeedMs,
                        StabilityClass = wind.StabilityClass
                    }
                };

            var compounds = await _context.EmissionCompounds.ToListAsync();
            double durationS = Math.Max(wind.FireDurationHours * 3600.0, 1.0);
            double u = Math.Max(wind.WindSpeedMs, 0.5);

            var results = new List<CompoundDispersionDto>();
            foreach (var compound in compounds)
            {
                var ef = compound.Ef;
                double Q = 0;
                foreach (var (cat, mass) in categoryMass)
                    if (ef.TryGetValue(cat, out double efVal) && efVal > 0)
                        Q += mass * efVal * 1000.0 / durationS;

                if (Q < 1e-12) continue;
                results.Add(new CompoundDispersionDto
                {
                    CompoundId = compound.Id, CompoundName = compound.Name,
                    BaseRate = compound.BaseRate, EmissionRateGs = Q,
                    GridPoints = ComputeGrid(Q, u, wind.StabilityClass, wind.SourceHeightM)
                });
            }
            results.Sort((a, b) => b.EmissionRateGs.CompareTo(a.EmissionRateGs));

            return new EventDispersionResultDto
            {
                TotalMassTonnes = totalMass,
                Materials = materialRows,
                UncategorizedMaterials = uncategorized,
                ZeroQuantityMaterials = zeroQty,
                Dispersion = new DispersionResultDto
                {
                    FireLat = wind.FireLat, FireLon = wind.FireLon,
                    WindDirectionDeg = wind.WindDirectionDeg,
                    WindSpeedMs = u, StabilityClass = wind.StabilityClass,
                    Compounds = results
                }
            };
        }

        private static double ResolveQuantity(ŽVPAIS_API.Models.ObjectMaterial om, ŽVPAIS_API.Models.EnvironmentObject obj)
        {
            if (om.Mass.HasValue) return om.Mass.Value;
            if (om.Volume.HasValue) return om.Volume.Value;
            if (om.Percentage.HasValue)
            {
                double total = obj.TotalMass ?? obj.TotalVolume ?? 0;
                return om.Percentage.Value / 100.0 * total;
            }
            return 0;
        }

        private static List<GridPointDto> ComputeGrid(double Q, double u, string cls, double H)
        {
            double[] xValues = [100, 250, 500, 750, 1000, 1500, 2000, 3000, 5000, 7500, 10000, 15000, 20000];
            double[] yValues = [-3000, -2000, -1500, -1000, -750, -500, -250, 0, 250, 500, 750, 1000, 1500, 2000, 3000];

            var points = new List<GridPointDto>(xValues.Length * yValues.Length);

            foreach (double x in xValues)
            {
                double sigmaY = SigmaY(x, cls);
                double sigmaZ = SigmaZ(x, cls);

                foreach (double y in yValues)
                {
                    double C = Concentration(Q, u, sigmaY, sigmaZ, y, H);
                    if (C < 1e-9) continue; // below 0.001 ug/m3 - negligible

                    points.Add(new GridPointDto
                    {
                        DownwindM = x,
                        CrosswindM = y,
                        ConcentrationUgM3 = C * 1e6 // convert g/m3 to ug/m3
                    });
                }
            }

            return points;
        }

        // Steady-state Gaussian plume formula with ground reflection at receptor height z=0.
        // Returns concentration in g/m3.
        private static double Concentration(double Q, double u, double sigmaY, double sigmaZ, double y, double H)
        {
            if (sigmaY <= 0 || sigmaZ <= 0) return 0;
            double expY = Math.Exp(-0.5 * (y * y) / (sigmaY * sigmaY));
            double expZ = Math.Exp(-0.5 * (H * H) / (sigmaZ * sigmaZ));
            return Q / (Math.PI * sigmaY * sigmaZ * u) * expY * expZ;
        }

        // Horizontal dispersion coefficient (m) using Briggs (1973) open-country parameterisation.
        private static double SigmaY(double x, string cls) => cls.ToUpper() switch
        {
            "A" => 0.22 * x * Math.Pow(1 + 0.0001 * x, -0.5),
            "B" => 0.16 * x * Math.Pow(1 + 0.0001 * x, -0.5),
            "C" => 0.11 * x * Math.Pow(1 + 0.0001 * x, -0.5),
            "D" => 0.08 * x * Math.Pow(1 + 0.0001 * x, -0.5),
            "E" => 0.06 * x * Math.Pow(1 + 0.0001 * x, -0.5),
            "F" => 0.04 * x * Math.Pow(1 + 0.0001 * x, -0.5),
            _ => 0.08 * x * Math.Pow(1 + 0.0001 * x, -0.5)
        };

        // Vertical dispersion coefficient (m), capped at 5000 m to prevent numerical overflow at long range.
        private static double SigmaZ(double x, string cls) => cls.ToUpper() switch
        {
            "A" => Math.Min(0.20 * x, 5000),
            "B" => Math.Min(0.12 * x, 5000),
            "C" => Math.Min(0.08 * x * Math.Pow(1 + 0.0002 * x, -0.5), 5000),
            "D" => Math.Min(0.06 * x * Math.Pow(1 + 0.0015 * x, -0.5), 5000),
            "E" => 0.03 * x / (1 + 0.0003 * x),
            "F" => 0.016 * x / (1 + 0.0003 * x),
            _ => Math.Min(0.06 * x * Math.Pow(1 + 0.0015 * x, -0.5), 5000)
        };
    }
}
