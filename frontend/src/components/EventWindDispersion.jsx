import React, { useEffect, useState } from 'react';
import { MapContainer, TileLayer, GeoJSON, Polygon, Marker, useMap, useMapEvents } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import api from '../services/api';

delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

const STABILITY_CLASSES = [
  { value: 'A', label: 'A – labai nestabili' },
  { value: 'B', label: 'B – nestabili' },
  { value: 'C', label: 'C – silpnai nestabili' },
  { value: 'D', label: 'D – neutrali' },
  { value: 'E', label: 'E – silpnai stabili' },
  { value: 'F', label: 'F – stabili' },
];

const CONC_BANDS = [
  { min: 0,     max: 1,        color: '#c7e9b4', label: '< 1' },
  { min: 1,     max: 10,       color: '#7fcdbb', label: '1–10' },
  { min: 10,    max: 100,      color: '#41b6c4', label: '10–100' },
  { min: 100,   max: 1000,     color: '#2c7fb8', label: '100–1 000' },
  { min: 1000,  max: 10000,    color: '#f97316', label: '1 000–10 000' },
  { min: 10000, max: Infinity, color: '#dc2626', label: '≥ 10 000' },
];

const CATEGORY_LABELS = {
  polymers: 'Polimerai', plastics: 'Plastikai', resins: 'Dervos', paper: 'Popierius', textile: 'Tekstilė',
  wood: 'Mediena', oil: 'Alyva/nafta', rubber: 'Guma', liquid_fuel: 'Skystieji degalai',
  carbon: 'Anglis', halogenated: 'Halogenintieji jk', liquid_organic: 'Lakieji organiniai jk',
};

function getConcColor(v) {
  for (const b of CONC_BANDS) if (v >= b.min && v < b.max) return b.color;
  return '#dc2626';
}

function polygonCentroid(polygonJson) {
  try {
    const geo = typeof polygonJson === 'string' ? JSON.parse(polygonJson) : polygonJson;
    const coords = geo.type === 'Feature' ? geo.geometry?.coordinates?.[0]
                 : geo.type === 'Polygon' ? geo.coordinates?.[0] : null;
    if (!coords?.length) return null;
    return {
      lat: Math.round(coords.reduce((s, c) => s + c[1], 0) / coords.length * 10000) / 10000,
      lon: Math.round(coords.reduce((s, c) => s + c[0], 0) / coords.length * 10000) / 10000,
    };
  } catch { return null; }
}

function offsetToLatLon(fireLat, fireLon, downwindM, crosswindM, windFromDeg) {
  const downwindRad = ((windFromDeg + 180) % 360) * Math.PI / 180;
  const crosswindRad = ((windFromDeg + 270) % 360) * Math.PI / 180;
  const cosLat = Math.cos(fireLat * Math.PI / 180);
  const lat = fireLat + (downwindM * Math.cos(downwindRad) + crosswindM * Math.cos(crosswindRad)) / 111111;
  const lon = fireLon + (downwindM * Math.sin(downwindRad) + crosswindM * Math.sin(crosswindRad)) / (111111 * cosLat);
  return [lat, lon];
}

function convexHull(points) {
  if (points.length < 3) return points;
  const sorted = [...points].sort((a, b) => a[0] !== b[0] ? a[0] - b[0] : a[1] - b[1]);
  const cross = (O, A, B) => (A[0] - O[0]) * (B[1] - O[1]) - (A[1] - O[1]) * (B[0] - O[0]);
  const lower = [];
  for (const p of sorted) {
    while (lower.length >= 2 && cross(lower[lower.length - 2], lower[lower.length - 1], p) <= 0) lower.pop();
    lower.push(p);
  }
  const upper = [];
  for (let i = sorted.length - 1; i >= 0; i--) {
    const p = sorted[i];
    while (upper.length >= 2 && cross(upper[upper.length - 2], upper[upper.length - 1], p) <= 0) upper.pop();
    upper.push(p);
  }
  lower.pop();
  upper.pop();
  return lower.concat(upper);
}

function LocationPicker({ onPick, enabled }) {
  const map = useMapEvents({ click: e => { if (enabled) onPick(e.latlng.lat, e.latlng.lng); } });
  useEffect(() => { map.getContainer().style.cursor = enabled ? 'crosshair' : ''; }, [enabled, map]);
  return null;
}

function FitPolygon({ polygon }) {
  const map = useMap();
  useEffect(() => {
    if (!polygon) return;
    try { map.fitBounds(L.geoJSON(JSON.parse(polygon)).getBounds(), { padding: [30, 30] }); } catch { }
  }, [polygon, map]);
  return null;
}

const STABILITY_DESCRIPTIONS = {
  A: { text: 'Labai nestabili — stipri saulė, vėjas < 2 m/s (vasaros vidurdienis, giedras dangus)', color: '#dc2626' },
  B: { text: 'Nestabili — vidutinė saulė, vėjas 2–3 m/s', color: '#ea580c' },
  C: { text: 'Silpnai nestabili — silpna saulė, vėjas 3–5 m/s', color: '#d97706' },
  D: { text: 'Neutrali — debesuota arba vėjas > 6 m/s (dažniausia)', color: '#059669' },
  E: { text: 'Silpnai stabili — giedra naktis, vėjas 2–3 m/s', color: '#0284c7' },
  F: { text: 'Stabili — giedra naktis, vėjas < 2 m/s (dūmų stulpas lieka žemai)', color: '#7c3aed' },
};

function deriveStabilityClass(windSpeed, shortwaveRad, cloudCoverPct, isDay) {
  if (isDay) {
    const ins = shortwaveRad > 500 ? 'strong' : shortwaveRad > 250 ? 'moderate' : 'slight';
    if (windSpeed < 2) return ins === 'slight' ? 'B' : 'A';
    if (windSpeed < 3) return ins === 'strong' ? 'A' : ins === 'moderate' ? 'B' : 'C';
    if (windSpeed < 5) return ins === 'strong' ? 'B' : 'C';
    if (windSpeed < 6) return ins === 'strong' ? 'C' : 'D';
    return ins === 'strong' ? 'C' : 'D';
  } else {
    if (windSpeed >= 5) return 'D';
    if (cloudCoverPct > 50) return windSpeed < 2 ? 'E' : 'D';
    return windSpeed < 2 ? 'F' : 'E';
  }
}

function suggestClass(windSpeed) {
  if (windSpeed >= 6) return 'D';
  if (windSpeed >= 3) return 'C';
  if (windSpeed >= 2) return 'B';
  return 'A';
}

function StabilityHint({ cls, windSpeed }) {
  const info = STABILITY_DESCRIPTIONS[cls?.toUpperCase()] ?? STABILITY_DESCRIPTIONS.D;
  const suggested = suggestClass(windSpeed);
  const mismatch = suggested !== cls?.toUpperCase();
  return (
    <div style={{ fontSize: '0.73rem', marginTop: 4, padding: '4px 6px', background: '#f8fafc', border: `1px solid ${info.color}30`, borderLeft: `3px solid ${info.color}`, borderRadius: 4, color: '#444' }}>
      <span>{info.text}</span>
      {mismatch && (
        <span style={{ display: 'block', marginTop: 2, color: '#b45309' }}>
          ⚠ Pagal vėjo greitį rekomenduojama klasė: <strong>{suggested}</strong>
        </span>
      )}
    </div>
  );
}

export default function EventWindDispersion({ breakdown, eventData }) {
  const centroid = polygonCentroid(eventData?.polygon);

  const [form, setForm] = useState({
    fireDurationHours: 1,
    windSpeedMs: 3,
    windDirectionDeg: 270,
    stabilityClass: 'D',
    sourceHeightM: 10,
    fireLat: centroid?.lat ?? 54.6872,
    fireLon: centroid?.lon ?? 25.2797,
  });
  const [picking,    setPicking]    = useState(false);
  const [result,     setResult]     = useState(null);
  const [selectedId, setSelectedId] = useState(null);
  const [loading,    setLoading]    = useState(false);
  const [windLoading, setWindLoading] = useState(false);
  const [error,      setError]      = useState('');
  const [windMsg,    setWindMsg]    = useState('');

  useEffect(() => {
    const c = polygonCentroid(eventData?.polygon);
    if (c) setForm(f => ({ ...f, fireLat: c.lat, fireLon: c.lon }));
  }, [eventData]);

  const setField = (name, value) => setForm(f => ({ ...f, [name]: value }));

  const handleChange = e => {
    const { name, value } = e.target;
    setField(name, name === 'stabilityClass' ? value : (value === '' ? '' : Number(value)));
  };

  // Returns { speed, dir, stabilityClass, date, hour } or throws
  const fetchWindFromApi = async (lat, lon, eventDate) => {
    if (!eventDate) throw new Error('Nėra įvykio datos.');
    const dt = new Date(eventDate);
    const date = dt.toISOString().split('T')[0];
    const url = `https://archive-api.open-meteo.com/v1/archive?latitude=${lat}&longitude=${lon}&start_date=${date}&end_date=${date}&hourly=windspeed_10m,winddirection_10m,shortwave_radiation,cloudcover,is_day&wind_speed_unit=ms&timezone=UTC`;
    const res = await fetch(url);
    if (!res.ok) {
      const txt = await res.text().catch(() => '');
      throw new Error(`Open-Meteo klaida ${res.status}${txt ? ': ' + txt.slice(0, 120) : ''}`);
    }
    const data = await res.json();
    const hour = dt.getUTCHours();
    const speed      = data.hourly?.windspeed_10m?.[hour];
    const dir        = data.hourly?.winddirection_10m?.[hour];
    const radiation  = data.hourly?.shortwave_radiation?.[hour] ?? 0;
    const cloudCover = data.hourly?.cloudcover?.[hour] ?? 50;
    const isDay      = data.hourly?.is_day?.[hour] === 1;
    if (speed == null || dir == null) throw new Error(`Duomenys nepasiekiami (indeksas ${hour})`);
    const stabilityClass = deriveStabilityClass(speed, radiation, cloudCover, isDay);
    return { speed: Math.round(speed * 10) / 10, dir: Math.round(dir), stabilityClass, date, hour };
  };

  const fetchWindData = async () => {
    setWindLoading(true); setWindMsg('');
    try {
      const { speed, dir, stabilityClass, date, hour } = await fetchWindFromApi(form.fireLat, form.fireLon, eventData?.eventDate);
      setForm(f => ({ ...f, windSpeedMs: speed, windDirectionDeg: dir, stabilityClass }));
      setWindMsg(`✓ ${date} ${hour}:00 UTC — ${speed} m/s iš ${dir}°, klasė ${stabilityClass}`);
    } catch (err) {
      setWindMsg(`Klaida: ${err.message}`);
    } finally { setWindLoading(false); }
  };

  const runCalculation = async (overrideForm) => {
    const f = overrideForm ?? form;
    setLoading(true); setError(''); setResult(null);
    try {
      if (!eventData?.idEvent) { setError('Nėra įvykio duomenų.'); setLoading(false); return; }
      const res = await api.post(`/wind-dispersion/calculate-from-event/${eventData.idEvent}`, {
        fireDurationHours: Number(f.fireDurationHours),
        windSpeedMs:       Number(f.windSpeedMs),
        windDirectionDeg:  Number(f.windDirectionDeg),
        stabilityClass:    f.stabilityClass,
        sourceHeightM:     Number(f.sourceHeightM),
        fireLat:           Number(f.fireLat),
        fireLon:           Number(f.fireLon),
      });
      setResult(res.data);
      setSelectedId(res.data.dispersion?.compounds?.[0]?.compoundId ?? null);
    } catch (err) {
      const d = err.response?.data;
      setError(typeof d === 'string' ? d : d?.title || err.message || 'Klaida skaičiuojant.');
    } finally { setLoading(false); }
  };

  const handleSubmit = async e => { e.preventDefault(); await runCalculation(); };

  // Fetch wind data then immediately calculate
  const fetchAndCalculate = async () => {
    setWindLoading(true); setWindMsg(''); setError(''); setResult(null);
    try {
      const { speed, dir, stabilityClass, date, hour } = await fetchWindFromApi(form.fireLat, form.fireLon, eventData?.eventDate);
      const newForm = { ...form, windSpeedMs: speed, windDirectionDeg: dir, stabilityClass };
      setForm(newForm);
      setWindMsg(`✓ ${date} ${hour}:00 UTC — ${speed} m/s iš ${dir}°, klasė ${stabilityClass}`);
      setWindLoading(false);
      await runCalculation(newForm);
    } catch (err) {
      setWindMsg(`Klaida: ${err.message}`);
      setWindLoading(false);
    }
  };

  const dispersion = result?.dispersion;
  const selectedCompound = dispersion?.compounds?.find(c => c.compoundId === selectedId);
  const geoJsonData = (() => { try { return eventData?.polygon ? JSON.parse(eventData.polygon) : null; } catch { return null; } })();
  const mapCenter = [Number(form.fireLat) || 54.6872, Number(form.fireLon) || 25.2797];

  // Material summary from event
  const hasMaterials  = result?.materials?.length > 0;
  const categorized   = result?.materials?.filter(m => m.emissionCategory) ?? [];
  const uncategorized = result?.uncategorizedMaterials ?? [];
  const zeroQty       = result?.zeroQuantityMaterials ?? [];

  return (
    <div>
      {/* Material summary banner */}
      {result && (
        <div style={{ marginBottom: '1rem', padding: '8px 12px', background: '#f0f9ff', border: '1px solid #bae6fd', borderRadius: 6, fontSize: '0.83rem' }}>
          <strong>Medžiagos iš įvykio objektų</strong> — bendra masė: <strong>{result.totalMassTonnes} t</strong>
          {categorized.length > 0 && (
            <div style={{ marginTop: 4 }}>
              {Object.entries(
                categorized.reduce((acc, m) => {
                  acc[m.emissionCategory] = (acc[m.emissionCategory] || 0) + m.massTonnes;
                  return acc;
                }, {})
              ).map(([cat, mass]) => (
                <span key={cat} style={{ marginRight: 12 }}>
                  <strong>{CATEGORY_LABELS[cat] ?? cat}</strong>: {mass.toFixed(3)} t
                </span>
              ))}
            </div>
          )}
          {uncategorized.length > 0 && (
            <div style={{ marginTop: 2, color: '#b45309' }}>
              ⚠ Be emisijų kategorijos: {uncategorized.join(', ')} — priskirti kategoriją medžiagos formoje.
            </div>
          )}
          {zeroQty.length > 0 && (
            <div style={{ marginTop: 2, color: '#b45309' }}>
              ⚠ Nulinė masė (praleista): {zeroQty.join(', ')} — nurodyti masę arba tūrį objekto medžiagų sąraše.
            </div>
          )}
          {!hasMaterials && (
            <div style={{ color: '#b45309' }}>⚠ Įvykio objektuose nerasta medžiagų su masėmis.</div>
          )}
        </div>
      )}

      <div style={{ display: 'flex', gap: '1.5rem', flexWrap: 'wrap' }}>
        {/* ── Form ── */}
        <div style={{ flex: '0 0 280px', minWidth: 250 }}>
          <form onSubmit={handleSubmit}>
            {/* Wind data fetch */}
            <div style={{ marginBottom: '0.75rem', padding: '8px', background: '#f5f5f5', borderRadius: 6 }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 4 }}>
                <strong style={{ fontSize: '0.85rem' }}>Vėjo duomenys</strong>
                <div style={{ display: 'flex', gap: 4 }}>
                  <button type="button" onClick={fetchWindData} disabled={windLoading || loading}
                    style={{ fontSize: '0.75rem', padding: '2px 8px', background: '#0ea5e9', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                    {windLoading ? '…' : 'Gauti'}
                  </button>
                  <button type="button" onClick={fetchAndCalculate} disabled={windLoading || loading}
                    style={{ fontSize: '0.75rem', padding: '2px 8px', background: '#059669', color: '#fff', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                    {windLoading || loading ? '…' : 'Gauti ir skaičiuoti'}
                  </button>
                </div>
              </div>
              {windMsg && <div style={{ fontSize: '0.75rem', color: windMsg.startsWith('✓') ? '#166534' : '#b45309' }}>{windMsg}</div>}
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.4rem', marginTop: '0.4rem' }}>
                <div><label>Greitis (m/s)</label>
                  <input type="number" name="windSpeedMs" min="0.5" max="30" step="0.1" value={form.windSpeedMs} onChange={handleChange} /></div>
                <div><label>Kryptis (°)</label>
                  <input type="number" name="windDirectionDeg" min="0" max="359" step="1" value={form.windDirectionDeg} onChange={handleChange} /></div>
              </div>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.4rem' }}>
              <div><label>Trukmė (val.)</label>
                <input type="number" name="fireDurationHours" min="0.1" step="0.1" value={form.fireDurationHours} onChange={handleChange} /></div>
              <div><label>Šaltinio aukštis (m)</label>
                <input type="number" name="sourceHeightM" min="0" max="200" step="1" value={form.sourceHeightM} onChange={handleChange} /></div>
            </div>

            <label style={{ marginTop: '0.5rem', display: 'block' }}>Stabilumo klasė</label>
            <select name="stabilityClass" value={form.stabilityClass} onChange={handleChange}>
              {STABILITY_CLASSES.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}
            </select>
            <StabilityHint cls={form.stabilityClass} windSpeed={form.windSpeedMs} />

            <div style={{ marginTop: '0.5rem', background: '#f5f5f5', borderRadius: 6, padding: '0.5rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 4 }}>
                <strong style={{ fontSize: '0.85rem' }}>Gaisro vieta</strong>
                <button type="button" onClick={() => setPicking(p => !p)}
                  style={{ fontSize: '0.75rem', padding: '2px 8px', background: picking ? '#2563eb' : '#e5e7eb', color: picking ? '#fff' : '#333', border: 'none', borderRadius: 4, cursor: 'pointer' }}>
                  {picking ? 'Spustelėkite…' : '📍 Žemėlapyje'}
                </button>
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.4rem' }}>
                <div style={{ minWidth: 0 }}><label>Platuma</label>
                  <input type="number" name="fireLat" step="0.0001" value={form.fireLat} onChange={handleChange} style={{ width: '100%', boxSizing: 'border-box' }} /></div>
                <div style={{ minWidth: 0 }}><label>Ilguma</label>
                  <input type="number" name="fireLon" step="0.0001" value={form.fireLon} onChange={handleChange} style={{ width: '100%', boxSizing: 'border-box' }} /></div>
              </div>
            </div>

            {error && <div className="error-message" style={{ marginTop: '0.5rem' }}>{error}</div>}
            <button type="submit" disabled={loading} style={{ marginTop: '0.75rem', width: '100%' }}>
              {loading ? 'Skaičiuojama…' : 'Skaičiuoti sklaidą'}
            </button>
          </form>

          {/* Legend */}
          <div style={{ marginTop: '1rem' }}>
            <strong style={{ fontSize: '0.82rem' }}>Koncentracija (µg/m³)</strong>
            {CONC_BANDS.map(b => (
              <div key={b.min} style={{ display: 'flex', alignItems: 'center', gap: 5, fontSize: '0.78rem', marginTop: 2 }}>
                <span style={{ width: 12, height: 12, borderRadius: '50%', background: b.color, display: 'inline-block', border: '1px solid #ccc' }} />
                {b.label}
              </div>
            ))}
          </div>
        </div>

        {/* ── Map + results ── */}
        <div style={{ flex: '1 1 420px', minWidth: 0, overflow: 'hidden' }}>
          <div style={{ height: 400, borderRadius: 8, overflow: 'hidden', border: '1px solid #ddd', marginBottom: '0.75rem', position: 'relative', zIndex: 0 }}>
            <MapContainer center={mapCenter} zoom={12} style={{ height: '100%' }}>
              <TileLayer
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
              />
              <FitPolygon polygon={eventData?.polygon} />
              <LocationPicker onPick={(lat, lon) => { setField('fireLat', Math.round(lat * 10000) / 10000); setField('fireLon', Math.round(lon * 10000) / 10000); setPicking(false); }} enabled={picking} />
              {geoJsonData && <GeoJSON data={geoJsonData} style={{ fillColor: '#3388ff', weight: 2, color: '#1a5fa8', fillOpacity: 0.25 }} />}
              <Marker position={mapCenter} />
              {dispersion && selectedCompound && (() => {
                const latlons = selectedCompound.gridPoints.map(pt => {
                  const [lat, lon] = offsetToLatLon(
                    dispersion.fireLat, dispersion.fireLon,
                    pt.downwindM, pt.crosswindM, dispersion.windDirectionDeg
                  );
                  return isFinite(lat) && isFinite(lon) ? [lat, lon, pt.concentrationUgM3] : null;
                }).filter(Boolean);

                return CONC_BANDS.map(band => {
                  const pts = latlons.filter(([,,c]) => c >= band.min).map(([lat, lon]) => [lat, lon]);
                  if (pts.length < 3) return null;
                  const hull = convexHull(pts);
                  if (hull.length < 3) return null;
                  return (
                    <Polygon key={band.min} positions={hull}
                      pathOptions={{ color: band.color, fillColor: band.color, fillOpacity: 0.45, weight: 1, opacity: 0.8 }} />
                  );
                });
              })()}
            </MapContainer>
          </div>

          {dispersion && (
            dispersion.compounds?.length === 0 ? (
              <div style={{ padding: '0.75rem', background: '#fff8e1', border: '1px solid #f9a825', borderRadius: 6, fontSize: '0.83rem' }}>
                Emisijų nerasta. Patikrinkite, ar įvykio objektų medžiagoms priskirta emisijų kategorija (Medžiagos → redaguoti → Emisijų kategorija).
              </div>
            ) : (
              <>
                <p style={{ fontSize: '0.82rem', margin: '0 0 0.4rem', wordBreak: 'break-word' }}>
                  <strong>{dispersion.compounds.length}</strong> junginiai. Vėjas: {dispersion.windSpeedMs} m/s iš {dispersion.windDirectionDeg}°, klasė {dispersion.stabilityClass}.
                </p>
                <div style={{ overflowX: 'auto', maxHeight: 220, overflowY: 'auto' }}>
                  <table style={{ width: '100%', fontSize: '0.8rem', borderCollapse: 'collapse' }}>
                    <thead style={{ position: 'sticky', top: 0, background: '#f0f0f0' }}>
                      <tr>
                        <th style={{ textAlign: 'left', padding: '3px 8px' }}>Junginys</th>
                        <th style={{ textAlign: 'right', padding: '3px 8px' }}>Q (g/s)</th>
                        <th style={{ textAlign: 'right', padding: '3px 8px' }}>T_n (€/t)</th>
                        <th style={{ textAlign: 'center', padding: '3px 8px' }}>↗</th>
                      </tr>
                    </thead>
                    <tbody>
                      {dispersion.compounds.slice(0, 20).map(c => (
                        <tr key={c.compoundId}
                          style={{ background: c.compoundId === selectedId ? '#e8f4fd' : 'white', cursor: 'pointer' }}
                          onClick={() => setSelectedId(c.compoundId)}>
                          <td style={{ padding: '3px 8px' }}>{c.compoundName}</td>
                          <td style={{ textAlign: 'right', padding: '3px 8px' }}>{c.emissionRateGs.toExponential(2)}</td>
                          <td style={{ textAlign: 'right', padding: '3px 8px' }}>{c.baseRate != null ? c.baseRate.toLocaleString() : '–'}</td>
                          <td style={{ textAlign: 'center', padding: '3px 8px' }}>
                            <button style={{ fontSize: '0.7rem', padding: '1px 4px' }}
                              onClick={e => { e.stopPropagation(); setSelectedId(c.compoundId); }}>●</button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </>
            )
          )}
        </div>
      </div>
    </div>
  );
}
