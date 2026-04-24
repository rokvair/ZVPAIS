import React, { useEffect, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { MapContainer, TileLayer, GeoJSON, useMap } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import api from '../services/api';
import { useAuth } from '../context/AuthContext';

delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

const TYPE_COLORS = {
  gaisras: '#e53935',
  'medžiagų išsiliejimas': '#fb8c00',
  stichija: '#43a047',
};
const getColor = (type) => TYPE_COLORS[type] || '#3388ff';

const STATUS_LABELS = {
  naujas: { label: 'Naujas', color: '#888' },
  'laukia peržiūros': { label: 'Laukia peržiūros', color: '#e65100' },
  tikrinamas: { label: 'Tikrinamas', color: '#1565c0' },
  patvirtintas: { label: 'Patvirtintas', color: '#2e7d32' },
  atmestas: { label: 'Atmestas', color: '#c62828' },
};

function StatusBadge({ status }) {
  const s = STATUS_LABELS[status] || { label: status, color: '#888' };
  return (
    <span style={{ background: s.color, color: '#fff', padding: '2px 7px', borderRadius: '3px', fontSize: '0.82em', whiteSpace: 'nowrap' }}>
      {s.label}
    </span>
  );
}

function MapFlyTo({ selectedEvent }) {
  const map = useMap();
  useEffect(() => {
    if (!selectedEvent?.polygon) return;
    try {
      const geo = JSON.parse(selectedEvent.polygon);
      const layer = L.geoJSON(geo);
      map.fitBounds(layer.getBounds(), { padding: [40, 40] });
    } catch { }
  }, [selectedEvent, map]);
  return null;
}

const EventManagement = () => {
  const { isSpecialist } = useAuth();
  const navigate = useNavigate();
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selectedEvent, setSelectedEvent] = useState(null);
  const [filterType, setFilterType] = useState('');
  const [filterStatus, setFilterStatus] = useState('');
  const [rejectNotes, setRejectNotes] = useState({});

  useEffect(() => { fetchEvents(); }, []);

  const fetchEvents = async () => {
    try {
      setLoading(true);
      const res = await api.get('/events');
      setEvents(res.data);
      setError('');
    } catch (err) {
      setError('Nepavyko gauti įvykių sąrašo.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Ar tikrai norite ištrinti šį įvykį?')) return;
    try {
      await api.delete(`/events/${id}`);
      setEvents(events.filter(e => e.idEvent !== id));
      if (selectedEvent?.idEvent === id) setSelectedEvent(null);
    } catch {
      alert('Klaida trinant įvykį.');
    }
  };

  const handleApprove = async (id) => {
    try {
      await api.post(`/events/${id}/approve`);
      setEvents(events.map(e => e.idEvent === id ? { ...e, status: 'patvirtintas' } : e));
    } catch (err) {
      alert('Klaida tvirtinant įvykį.');
      console.error(err);
    }
  };

  const handleReject = async (id) => {
    const notes = rejectNotes[id] || '';
    try {
      await api.post(`/events/${id}/reject`, { notes });
      setEvents(events.map(e => e.idEvent === id ? { ...e, status: 'atmestas' } : e));
      setRejectNotes(prev => { const n = { ...prev }; delete n[id]; return n; });
    } catch (err) {
      alert('Klaida atmetant įvykį.');
      console.error(err);
    }
  };

  const pending = events.filter(e => e.status === 'laukia peržiūros');

  const filtered = events.filter(e =>
    (!filterType || e.eventType === filterType) &&
    (!filterStatus || e.status === filterStatus)
  );

  const geoJsonData = {
    type: 'FeatureCollection',
    features: filtered
      .filter(e => e.polygon)
      .map(e => ({
        type: 'Feature',
        geometry: (() => { try { return JSON.parse(e.polygon); } catch { return null; } })(),
        properties: { id: e.idEvent, eventType: e.eventType, selected: selectedEvent?.idEvent === e.idEvent }
      }))
      .filter(f => f.geometry)
  };

  const polygonStyle = (feature) => ({
    fillColor: getColor(feature.properties.eventType),
    weight: feature.properties.selected ? 3 : 1.5,
    color: feature.properties.selected ? '#000' : '#fff',
    fillOpacity: feature.properties.selected ? 0.75 : 0.45,
  });

  const onEachFeature = (feature, layer) => {
    const ev = events.find(e => e.idEvent === feature.properties.id);
    if (ev) layer.on('click', () => setSelectedEvent(ev));
  };

  if (loading) return <div>Kraunama...</div>;
  if (error) return <div style={{ color: 'red' }}>{error}</div>;

  return (
    <div>
      {/* Pending review queue — Specialist only */}
      {isSpecialist && pending.length > 0 && (
        <div style={{ marginBottom: '20px', border: '2px solid #e65100', borderRadius: '6px', padding: '12px', background: '#fff8f5' }}>
          <h3 style={{ margin: '0 0 10px', color: '#e65100' }}>⚠ Laukia peržiūros ({pending.length})</h3>
          {pending.map(ev => (
            <div key={ev.idEvent} style={{ display: 'flex', alignItems: 'center', gap: '10px', marginBottom: '8px', flexWrap: 'wrap', borderBottom: '1px solid #ffe0cc', paddingBottom: '8px' }}>
              <span style={{ minWidth: '24px', fontWeight: 'bold' }}>#{ev.idEvent}</span>
              <span style={{ background: getColor(ev.eventType), color: '#fff', padding: '2px 6px', borderRadius: '3px', fontSize: '0.82em' }}>{ev.eventType}</span>
              <span>{new Date(ev.eventDate).toLocaleDateString('lt-LT')}</span>
              {ev.location && <span style={{ color: '#555' }}>{ev.location}</span>}
              <Link to={`/events/${ev.idEvent}/calculation`} style={{ marginLeft: 'auto' }}>Žiūrėti skaičiavimą</Link>
              <button onClick={() => handleApprove(ev.idEvent)} style={{ background: '#2e7d32', color: '#fff', border: 'none', padding: '4px 10px', borderRadius: '3px', cursor: 'pointer' }}>
                Patvirtinti
              </button>
              <input
                type="text"
                placeholder="Atmetimo priežastis..."
                value={rejectNotes[ev.idEvent] || ''}
                onChange={e => setRejectNotes(prev => ({ ...prev, [ev.idEvent]: e.target.value }))}
                style={{ width: '180px', padding: '3px 6px', fontSize: '0.85em' }}
              />
              <button onClick={() => handleReject(ev.idEvent)} style={{ background: '#c62828', color: '#fff', border: 'none', padding: '4px 10px', borderRadius: '3px', cursor: 'pointer' }}>
                Atmesti
              </button>
            </div>
          ))}
        </div>
      )}

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '12px' }}>
        <h2 style={{ margin: 0 }}>Įvykiai</h2>
        <Link to="/events/new">+ Naujas įvykis</Link>
      </div>

      {/* Filters */}
      <div style={{ display: 'flex', gap: '12px', marginBottom: '12px' }}>
        <select value={filterType} onChange={e => setFilterType(e.target.value)}>
          <option value="">Visi tipai</option>
          <option value="gaisras">Gaisras</option>
          <option value="medžiagų išsiliejimas">Medžiagų išsiliejimas</option>
          <option value="stichija">Stichija</option>
        </select>
        <select value={filterStatus} onChange={e => setFilterStatus(e.target.value)}>
          <option value="">Visi statusai</option>
          <option value="naujas">Naujas</option>
          <option value="laukia peržiūros">Laukia peržiūros</option>
          <option value="tikrinamas">Tikrinamas</option>
          <option value="patvirtintas">Patvirtintas</option>
          <option value="atmestas">Atmestas</option>
        </select>
      </div>

      <div style={{ display: 'flex', gap: '16px', alignItems: 'flex-start' }}>
        {/* Event table */}
        <div style={{ flex: '1', overflowX: 'auto' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9em' }}>
            <thead>
              <tr style={{ background: '#f5f5f5' }}>
                <th style={th}>ID</th>
                <th style={th}>Tipas</th>
                <th style={th}>Data</th>
                <th style={th}>Vieta</th>
                <th style={th}>Statusas</th>
                <th style={th}>Veiksmai</th>
              </tr>
            </thead>
            <tbody>
              {filtered.length === 0 && (
                <tr><td colSpan={6} style={{ padding: '12px', textAlign: 'center', color: '#888' }}>Įvykių nerasta</td></tr>
              )}
              {filtered.map(event => (
                <tr
                  key={event.idEvent}
                  onClick={() => setSelectedEvent(event)}
                  style={{
                    cursor: 'pointer',
                    background: selectedEvent?.idEvent === event.idEvent ? '#e8f4fd' : 'transparent',
                    borderBottom: '1px solid #eee'
                  }}
                >
                  <td style={td}>{event.idEvent}</td>
                  <td style={td}>
                    <span style={{ background: getColor(event.eventType), color: '#fff', padding: '2px 6px', borderRadius: '3px', fontSize: '0.85em' }}>
                      {event.eventType}
                    </span>
                  </td>
                  <td style={td}>{new Date(event.eventDate).toLocaleDateString('lt-LT')}</td>
                  <td style={td}>{event.location || '—'}</td>
                  <td style={td}><StatusBadge status={event.status} /></td>
                  <td style={td} onClick={e => e.stopPropagation()}>
                    <Link to={`/events/${event.idEvent}/calculation`} style={{ marginRight: '6px' }}>Žala</Link>
                    {isSpecialist && (
                      <>
                        <Link to={`/events/edit/${event.idEvent}`} style={{ marginRight: '6px' }}>Redaguoti</Link>
                        <button onClick={() => handleDelete(event.idEvent)} style={{ fontSize: '0.85em' }}>Trinti</button>
                      </>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        {/* Map panel */}
        <div style={{ width: '420px', flexShrink: 0 }}>
          <MapContainer
            center={[55.0, 24.0]}
            zoom={7}
            style={{ height: '520px', width: '100%', borderRadius: '4px', border: '1px solid #ddd' }}
          >
            <TileLayer
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            />
            <GeoJSON
              key={JSON.stringify(geoJsonData)}
              data={geoJsonData}
              style={polygonStyle}
              onEachFeature={onEachFeature}
            />
            <MapFlyTo selectedEvent={selectedEvent} />
          </MapContainer>

          {selectedEvent && (
            <div style={{ marginTop: '8px', padding: '10px', background: '#f9f9f9', border: '1px solid #ddd', borderRadius: '4px', fontSize: '0.9em' }}>
              <strong>{selectedEvent.eventType}</strong> · {new Date(selectedEvent.eventDate).toLocaleDateString('lt-LT')}<br />
              <StatusBadge status={selectedEvent.status} /><br />
              {selectedEvent.location && <span>{selectedEvent.location}<br /></span>}
              {selectedEvent.description && <span style={{ color: '#555' }}>{selectedEvent.description}</span>}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

const th = { padding: '7px 10px', textAlign: 'left', borderBottom: '2px solid #ddd', whiteSpace: 'nowrap' };
const td = { padding: '7px 10px' };

export default EventManagement;
