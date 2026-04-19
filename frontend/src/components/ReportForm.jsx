import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../services/api';

const ReportForm = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEditing = !!id;

  const [events, setEvents] = useState([]);
  const [formData, setFormData] = useState({
    eventId: '',
    data: new Date().toISOString().split('T')[0],
    zalosDydis: '',
    piniginisDydis: '',
    notes: ''
  });
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(true);
  const [error, setError] = useState('');
  const [autoCalcStatus, setAutoCalcStatus] = useState('');

  useEffect(() => {
    const loadInitial = async () => {
      try {
        const eventsRes = await api.get('/events');
        setEvents(eventsRes.data);

        if (isEditing) {
          const reportRes = await api.get(`/reports/${id}`);
          const r = reportRes.data;
          setFormData({
            eventId: String(r.eventId),
            data: r.data ? r.data.split('T')[0] : new Date().toISOString().split('T')[0],
            zalosDydis: r.zalosDydis != null ? String(r.zalosDydis) : '',
            piniginisDydis: r.piniginisDydis != null ? String(r.piniginisDydis) : '',
            notes: r.notes || ''
          });
        }
      } catch (err) {
        setError('Nepavyko gauti duomenų.');
        console.error(err);
      } finally {
        setFetching(false);
      }
    };
    loadInitial();
  }, [id, isEditing]);

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleEventChange = async (e) => {
    const eventId = e.target.value;
    setFormData(prev => ({ ...prev, eventId }));
    if (!eventId) return;

    setAutoCalcStatus('Skaičiuojama...');
    try {
      const res = await api.get(`/calculation/event/${eventId}`);
      const total = res.data.totalDamage;
      setFormData(prev => ({
        ...prev,
        eventId,
        zalosDydis: total != null ? String(Number(total).toFixed(4)) : prev.zalosDydis,
        piniginisDydis: total != null ? String(Number(total).toFixed(4)) : prev.piniginisDydis
      }));
      setAutoCalcStatus(total != null ? `Automatiškai užpildyta: ${Number(total).toFixed(2)} EUR` : 'Skaičiavimo duomenų nėra.');
    } catch {
      setAutoCalcStatus('Nepavyko gauti skaičiavimo (galite įvesti rankiniu būdu).');
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!formData.eventId) {
      alert('Pasirinkite įvykį.');
      return;
    }
    try {
      setLoading(true);
      const payload = {
        eventId: Number(formData.eventId),
        data: new Date(formData.data).toISOString(),
        zalosDydis: formData.zalosDydis !== '' ? parseFloat(formData.zalosDydis) : null,
        piniginisDydis: formData.piniginisDydis !== '' ? parseFloat(formData.piniginisDydis) : null,
        notes: formData.notes || null
      };
      if (isEditing) {
        await api.put(`/reports/${id}`, payload);
      } else {
        await api.post('/reports', payload);
      }
      navigate('/reports');
    } catch (err) {
      setError('Klaida išsaugant ataskaitą.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (fetching) return <div>Kraunama...</div>;

  return (
    <div>
      <h2>{isEditing ? 'Redaguoti ataskaitą' : 'Nauja žalos vertinimo ataskaita'}</h2>
      {error && <div style={{ color: 'red', marginBottom: '12px' }}>{error}</div>}
      <form onSubmit={handleSubmit}>
        <div>
          <label>Įvykis:</label>
          <select
            name="eventId"
            value={formData.eventId}
            onChange={handleEventChange}
            required
            style={{ marginLeft: '8px' }}
          >
            <option value="">— Pasirinkite įvykį —</option>
            {events.map(ev => (
              <option key={ev.idEvent} value={ev.idEvent}>
                [{ev.idEvent}] {ev.eventType} — {new Date(ev.eventDate).toLocaleDateString('lt-LT')} {ev.location ? `(${ev.location})` : ''}
              </option>
            ))}
          </select>
          {autoCalcStatus && (
            <span style={{ marginLeft: '12px', color: autoCalcStatus.startsWith('Automatiškai') ? 'green' : '#888', fontSize: '0.9em' }}>
              {autoCalcStatus}
            </span>
          )}
        </div>

        <div style={{ marginTop: '12px' }}>
          <label>Vertinimo data:</label>
          <input
            type="date"
            name="data"
            value={formData.data}
            onChange={handleChange}
            required
            style={{ marginLeft: '8px' }}
          />
        </div>

        <div style={{ marginTop: '12px' }}>
          <label>Žalos dydis (EUR):</label>
          <input
            type="number"
            step="0.0001"
            min="0"
            name="zalosDydis"
            value={formData.zalosDydis}
            onChange={handleChange}
            placeholder="pvz. 12500.00"
            style={{ marginLeft: '8px', width: '160px' }}
          />
        </div>

        <div style={{ marginTop: '12px' }}>
          <label>Piniginis dydis (EUR):</label>
          <input
            type="number"
            step="0.0001"
            min="0"
            name="piniginisDydis"
            value={formData.piniginisDydis}
            onChange={handleChange}
            placeholder="pvz. 12500.00"
            style={{ marginLeft: '8px', width: '160px' }}
          />
        </div>

        <div style={{ marginTop: '12px' }}>
          <label>Pastabos:</label>
          <textarea
            name="notes"
            value={formData.notes}
            onChange={handleChange}
            rows="4"
            style={{ marginLeft: '8px', width: '400px', display: 'block', marginTop: '4px' }}
          />
        </div>

        <div style={{ marginTop: '16px' }}>
          <button type="submit" disabled={loading}>
            {loading ? 'Saugoma...' : (isEditing ? 'Atnaujinti' : 'Sukurti')}
          </button>
          <button type="button" onClick={() => navigate('/reports')} style={{ marginLeft: '8px' }}>
            Atšaukti
          </button>
        </div>
      </form>
    </div>
  );
};

export default ReportForm;
