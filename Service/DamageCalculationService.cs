using Microsoft.EntityFrameworkCore;
using ŽVPAIS_API.Data;
using ŽVPAIS_API.Models;

namespace ŽVPAIS_API.Services
{
    public interface IDamageCalculationService
    {
        Task<decimal> CalculateDamageForEvent(int eventId);
    }

    public class DamageCalculationService : IDamageCalculationService
    {
        private readonly AppDbContext _context;

        public DamageCalculationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculateDamageForEvent(int eventId)
        {
            var eventObj = await _context.Events
                .Include(e => e.EventObjects)
                    .ThenInclude(eo => eo.Object)
                        .ThenInclude(o => o.ObjectMaterials)
                            .ThenInclude(om => om.Material)
                .FirstOrDefaultAsync(e => e.IdEvent == eventId);

            if (eventObj == null) return 0;

            decimal totalDamage = 0;

            foreach (var eventObject in eventObj.EventObjects)
            {
                var obj = eventObject.Object;
                if (obj.ObjectMaterials == null) continue;

                foreach (var objMaterial in obj.ObjectMaterials)
                {
                    var material = objMaterial.Material;
                    if (material == null) continue;

                    // 1. Medžiaga turi turėti bazinį įkainį ir kenksmingumo koeficientą
                    if (!material.BaseRate.HasValue || !material.HarmfulnessFactor.HasValue)
                        continue;

                    // 2. Nustatome absoliutų kiekį (masė, tūris arba procentas)
                    double quantity = 0;

                    if (objMaterial.Mass.HasValue)
                    {
                        quantity = objMaterial.Mass.Value;
                    }
                    else if (objMaterial.Volume.HasValue)
                    {
                        quantity = objMaterial.Volume.Value;
                    }
                    else if (objMaterial.Percentage.HasValue)
                    {
                        // Procentas – reikia bendros objekto masės arba tūrio
                        if (obj.TotalMass.HasValue)
                        {
                            quantity = obj.TotalMass.Value * objMaterial.Percentage.Value / 100.0;
                        }
                        else if (obj.TotalVolume.HasValue)
                        {
                            quantity = obj.TotalVolume.Value * objMaterial.Percentage.Value / 100.0;
                        }
                        else
                        {
                            // Neturime nei masės, nei tūrio – negalime apskaičiuoti
                            continue;
                        }
                    }
                    else
                    {
                        // Kiekis nenurodytas – praleidžiame
                        continue;
                    }

                    if (quantity <= 0) continue;

                    // 3. Skaičiuojame žalą pagal medžiagą
                    decimal materialDamage = (decimal)quantity
                                           * material.BaseRate.Value
                                           * material.HarmfulnessFactor.Value;
                    totalDamage += materialDamage;
                }
            }

            // 4. Pritaikome aplinkos jautrumo koeficientą (jei nurodytas)
            if (eventObj.SensitivityFactor.HasValue && eventObj.SensitivityFactor.Value > 0)
            {
                totalDamage *= eventObj.SensitivityFactor.Value;
            }

            return totalDamage;
        }
    }
}