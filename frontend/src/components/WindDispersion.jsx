import React, { useEffect, useState } from 'react';
import { MapContainer, TileLayer, CircleMarker, Marker, useMap, useMapEvents } from 'react-leaflet';
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
  { value: 'A', label: 'A – labai nestabili (saulėta, silpnas vėjas)' },
  { value: 'B', label: 'B – nestabili' },
  { value: 'C', label: 'C – silpnai nestabili' },
  { value: 'D', label: 'D – neutrali (apsiniaukę / naktis)' },
  { value: 'E', label: 'E – silpnai stabili' },
  { value: 'F', label: 'F – stabili (aiški naktis, silpnas vėjas)' },
];

const CONC_BANDS = [
  { min: 0,      max: 1,        color: '#ffffcc', label: '< 1' },
  { min: 1,      max: 10,       color: '#a1dab4', label: '1 – 10' },
  { min: 10,     max: 100,      color: '#41b6c4', label: '10 – 100' },
  { min: 100,    max: 1000,     color: '#2c7fb8', label: '100 – 1 000' },
  { min: 1000,   max: 10000,    color: '#f97316', label: '1 000 – 10 000' },
  { min: 10000,  max: Infinity, color: '#dc2626', label: '≥ 10 000' },
];

function getConcColor(ugM3) {
  for (const b of CONC_BANDS) {
    if (ugM3 >= b.min && ugM3 < b.max) return b.color;
  }
  return '#dc2626';
}

// Converts plume-coordinate offset (downwind, crosswind metres) to absolute [lat, lon].
// windDirectionDeg is FROM which direction the wind blows (meteorological convention, e.g. 270 = from west, plume goes east).
function offsetToLatLon(fireLat, fireLon, xM, yM, windFromDeg) {
  const downwindDeg = (windFromDeg + 180) % 360;
  const downwindRad = (downwindDeg * Math.PI) / 180;
  const crosswindRad = ((downwindDeg + 90) % 360) * (Math.PI / 180);
  const cosLat = Math.cos((fireLat * Math.PI) / 180);
  const lat = fireLat + (xM * Math.cos(downwindRad) + yM * Math.cos(crosswindRad)) / 111111;
  const lon = fireLon + (xM * Math.sin(downwindRad) + yM * Math.sin(crosswindRad)) / (111111 * cosLat);
  return [lat, lon];
}

// Lets user click the map to set fire location
function LocationPicker({ onPick, enabled }) {
  const map = useMapEvents({
    click(e) {
      if (enabled) onPick(e.latlng.lat, e.latlng.lng);
    },
  });
  useEffect(() => {
    map.getContainer().style.cursor = enabled ? 'crosshair' : '';
  }, [enabled, map]);
  return null;
}

function RecenterMap({ center }) {
  const map = useMap();
  useEffect(() => {
    if (center) map.setView(center, map.getZoom());
  }, [center, map]);
  return null;
}

export default function WindDispersion() {
  const [wasteTypes, setWasteTypes] = useState([]);
  const [form, setForm] = useState({
    wasteTypeId: '',
    totalMassTonnes: 10,
    fireDurationHours: 1,
    windSpeedMs: 3,
    windDirectionDeg: 270,
    stabilityClass: 'D',
    sourceHeightM: 10,
    fireLat: 54.6872,
    fireLon: 25.2797,
  });
  const [pickingLocation, setPickingLocation] = useState(false);
  const [result, setResult] = useState(null);
  const [selectedCompoundId, setSelectedCompoundId] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    api.get('/wind-dispersion/waste-types').then(r => setWasteTypes(r.data)).catch(() => {});
  }, []);

  const handleChange = e => {
    const { name, value } = e.target;
    setForm(f => ({
      ...f,
      [name]: name === 'stabilityClass' ? value : (value === '' ? '' : Number(value)),
    }));
  };

  const handleMapPick = (lat, lon) => {
    setForm(f => ({ ...f, fireLat: Math.round(lat * 10000) / 10000, fireLon: Math.round(lon * 10000) / 10000 }));
    setPickingLocation(false);
  };

  const handleSubmit = async e => {
    e.preventDefault();
    if (!form.wasteTypeId) { setError('Pasirinkite atliekų tipą.'); return; }
    setLoading(true);
    setError('');
    setResult(null);
    try {
      const res = await api.post('/wind-dispersion/calculate', {
        wasteTypeId: Number(form.wasteTypeId),
        totalMassTonnes: Number(form.totalMassTonnes),
        fireDurationHours: Number(form.fireDurationHours),
        windSpeedMs: Number(form.windSpeedMs),
        windDirectionDeg: Number(form.windDirectionDeg),
        stabilityClass: form.stabilityClass,
        sourceHeightM: Number(form.sourceHeightM),
        fireLat: Number(form.fireLat),
        fireLon: Number(form.fireLon),
      });
      setResult(res.data);
      setSelectedCompoundId(res.data.compounds[0]?.compoundId ?? null);
    } catch (err) {
      const d = err.response?.data;
      const msg = typeof d === 'string' ? d : d?.title || d?.detail || err.message || 'Klaida atliekant skaičiavimą.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  const selectedCompound = result?.compounds.find(c => c.compoundId === selectedCompoundId);
  const mapCenter = [Number(form.fireLat) || 54.6872, Number(form.fireLon) || 25.2797];

  return (
    <div style={{ display: 'flex', gap: '1.5rem', flexWrap: 'wrap' }}>
      {/* ── Left panel: form ── */}
      <div style={{ minWidth: 320, flex: '0 0 340px' }}>
        <h2 style={{ marginTop: 0 }}>Vėjo sklaidos modelis</h2>
        <p style={{ fontSize: '0.82rem', color: '#666', marginTop: 0 }}>
          Gauso sklaidos modelis (Pasquill–Gifford). Pasirinkite gaisro vietą žemėlapyje ir parametrus.
        </p>

        <form onSubmit={handleSubmit}>
          <label>Atliekų tipas (EWC kodas)</label>
          <select name="wasteTypeId" value={form.wasteTypeId} onChange={handleChange} required>
            <option value="">-- pasirinkite --</option>
            {wasteTypes.map(w => (
              <option key={w.id} value={w.id}>
                {w.ewcCode} – {w.description.length > 55 ? w.description.slice(0, 55) + '…' : w.description}
              </option>
            ))}
          </select>
          <small style={{ color: '#888', display: 'block', marginTop: 2 }}>
            Degančių medžiagų emisijas turi: popieriaus/plastiko/tekstilės/polimerų atliekos.
          </small>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.5rem', marginTop: '0.5rem' }}>
            <div>
              <label>Masė (t)</label>
              <input type="number" name="totalMassTonnes" min="0.01" step="0.1" value={form.totalMassTonnes} onChange={handleChange} />
            </div>
            <div>
              <label>Trukmė (val.)</label>
              <input type="number" name="fireDurationHours" min="0.1" step="0.1" value={form.fireDurationHours} onChange={handleChange} />
            </div>
            <div>
              <label>Vėjo greitis (m/s)</label>
              <input type="number" name="windSpeedMs" min="0.5" max="30" step="0.5" value={form.windSpeedMs} onChange={handleChange} />
            </div>
            <div>
              <label>Vėjo kryptis (°)</label>
              <input type="number" name="windDirectionDeg" min="0" max="359" step="1" value={form.windDirectionDeg} onChange={handleChange} />
              <small style={{ color: '#888' }}>Iš kurios pučia (0=N, 90=E, 270=W)</small>
            </div>
            <div>
              <label>Šaltinio aukštis (m)</label>
              <input type="number" name="sourceHeightM" min="0" max="200" step="1" value={form.sourceHeightM} onChange={handleChange} />
            </div>
          </div>

          <label style={{ marginTop: '0.5rem', display: 'block' }}>Atmosferos stabilumo klasė</label>
          <select name="stabilityClass" value={form.stabilityClass} onChange={handleChange}>
            {STABILITY_CLASSES.map(s => <option key={s.value} value={s.value}>{s.label}</option>)}
          </select>

          <div style={{ marginTop: '0.75rem', padding: '0.5rem', background: '#f5f5f5', borderRadius: 6 }}>
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', marginBottom: 4 }}>
              <strong style={{ fontSize: '0.85rem' }}>Gaisro vieta</strong>
              <button
                type="button"
                onClick={() => setPickingLocation(p => !p)}
                style={{
                  fontSize: '0.75rem', padding: '2px 8px',
                  background: pickingLocation ? '#2563eb' : '#e5e7eb',
                  color: pickingLocation ? '#fff' : '#333',
                  border: 'none', borderRadius: 4, cursor: 'pointer',
                }}
              >
                {pickingLocation ? '↕ Spustelėkite žemėlapyje…' : '📍 Parinkti žemėlapyje'}
              </button>
            </div>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.5rem' }}>
              <div>
                <label>Platuma</label>
                <input type="number" name="fireLat" step="0.0001" value={form.fireLat} onChange={handleChange} />
              </div>
              <div>
                <label>Ilguma</label>
                <input type="number" name="fireLon" step="0.0001" value={form.fireLon} onChange={handleChange} />
              </div>
            </div>
          </div>

          {error && <div className="error-message" style={{ marginTop: '0.5rem' }}>{error}</div>}
          <button type="submit" disabled={loading} style={{ marginTop: '0.75rem', width: '100%' }}>
            {loading ? 'Skaičiuojama…' : 'Skaičiuoti sklaidą'}
          </button>
        </form>

        {/* Legend */}
        <div style={{ marginTop: '1rem' }}>
          <strong style={{ fontSize: '0.85rem' }}>Koncentracija (µg/m³)</strong>
          <div style={{ display: 'flex', flexDirection: 'column', gap: 2, marginTop: 4 }}>
            {CONC_BANDS.map(b => (
              <div key={b.min} style={{ display: 'flex', alignItems: 'center', gap: 6, fontSize: '0.8rem' }}>
                <span style={{ width: 14, height: 14, borderRadius: '50%', background: b.color, display: 'inline-block', border: '1px solid #ccc' }} />
                {b.label}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* ── Right panel: map + results table ── */}
      <div style={{ flex: '1 1 500px', minWidth: 300 }}>
        {/* Map */}
        <div style={{ height: 420, borderRadius: 8, overflow: 'hidden', border: '1px solid #ddd', marginBottom: '1rem' }}>
          <MapContainer center={mapCenter} zoom={11} style={{ height: '100%' }}>
            <TileLayer
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            />
            <LocationPicker onPick={handleMapPick} enabled={pickingLocation} />
            <RecenterMap center={result ? [result.fireLat, result.fireLon] : null} />

            {/* Fire location marker */}
            <Marker position={mapCenter} />

            {/* Concentration dots */}
            {result && selectedCompound && selectedCompound.gridPoints.map((pt, i) => {
              const [lat, lon] = offsetToLatLon(result.fireLat, result.fireLon, pt.downwindM, pt.crosswindM, result.windDirectionDeg);
              if (!isFinite(lat) || !isFinite(lon)) return null;
              const color = getConcColor(pt.concentrationUgM3);
              return (
                <CircleMarker
                  key={i}
                  center={[lat, lon]}
                  radius={5}
                  pathOptions={{ color, fillColor: color, fillOpacity: 0.8, weight: 0 }}
                />
              );
            })}
          </MapContainer>
        </div>

        {/* Results table */}
        {result && (
          <>
            {result.compounds.length === 0 ? (
              <div style={{ padding: '1rem', background: '#fff8e1', border: '1px solid #f9a825', borderRadius: 6 }}>
                <strong>Emisijų nerasta.</strong> Pasirinktas atliekų tipas neturi degimo emisijų junginiams su turimais EF koeficientais.
                Pasirinkite atliekas, kurių morfologijoje yra: <em>polymers, plastics, paper, textile</em>.
              </div>
            ) : (
              <>
                <p style={{ fontSize: '0.82rem', margin: '0 0 0.5rem' }}>
                  Rasta <strong>{result.compounds.length}</strong> junginių su emisijomis.
                  {result.compounds.length > 20 && ' Rodomi pirmi 20.'}
                  &nbsp;Vėjas: <strong>{result.windSpeedMs} m/s</strong> iš <strong>{result.windDirectionDeg}°</strong>, klasė <strong>{result.stabilityClass}</strong>.
                </p>
                <div style={{ overflowX: 'auto' }}>
                  <table style={{ width: '100%', fontSize: '0.82rem', borderCollapse: 'collapse' }}>
                    <thead>
                      <tr style={{ background: '#f0f0f0' }}>
                        <th style={{ textAlign: 'left', padding: '4px 8px' }}>Junginys</th>
                        <th style={{ textAlign: 'right', padding: '4px 8px' }}>Q (g/s)</th>
                        <th style={{ textAlign: 'right', padding: '4px 8px' }}>T_n (€/t)</th>
                        <th style={{ textAlign: 'center', padding: '4px 8px' }}>Žemėlapis</th>
                      </tr>
                    </thead>
                    <tbody>
                      {result.compounds.slice(0, 20).map(c => (
                        <tr
                          key={c.compoundId}
                          style={{ background: c.compoundId === selectedCompoundId ? '#e8f4fd' : 'white', cursor: 'pointer' }}
                          onClick={() => setSelectedCompoundId(c.compoundId)}
                        >
                          <td style={{ padding: '4px 8px' }}>{c.compoundName}</td>
                          <td style={{ textAlign: 'right', padding: '4px 8px' }}>{c.emissionRateGs.toExponential(2)}</td>
                          <td style={{ textAlign: 'right', padding: '4px 8px' }}>
                            {c.baseRate != null ? c.baseRate.toLocaleString() : '–'}
                          </td>
                          <td style={{ textAlign: 'center', padding: '4px 8px' }}>
                            <button
                              style={{ fontSize: '0.75rem', padding: '1px 6px' }}
                              onClick={e => { e.stopPropagation(); setSelectedCompoundId(c.compoundId); }}
                            >
                              ●
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </>
            )}
          </>
        )}
      </div>
    </div>
  );
}
