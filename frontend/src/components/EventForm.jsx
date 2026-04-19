import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../services/api';
import PolygonPicker from './PolygonPicker';
import ObjectSelector from './ObjectSelector';
import { useAuth } from '../context/AuthContext';

const EventForm = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEditing = !!id;
  const { isSpecialist } = useAuth();

  const [formData, setFormData] = useState({
    eventType: 'gaisas',
    eventDate: '',
    description: '',
    location: '',
    polygon: null,
    status: 'naujas'
  });
  const [selectedEventObjects, setSelectedEventObjects] = useState([]);
  const [loading, setLoading] = useState(false);
  const [dataLoaded, setDataLoaded] = useState(!id);
  const [fetchError, setFetchError] = useState('');
  const [submitError, setSubmitError] = useState('');

  useEffect(() => {
    if (isEditing) {
      const fetchEvent = async () => {
        try {
          setLoading(true);
          const response = await api.get(`/events/${id}`);
          const event = response.data;
          setFormData({
            eventType: event.eventType,
            eventDate: event.eventDate.split('T')[0],
            description: event.description || '',
            location: event.location || '',
            polygon: event.polygon ? JSON.parse(event.polygon) : null,
            status: event.status || 'naujas'
          });
          if (event.objects) {
            setSelectedEventObjects(event.objects.map(obj => ({
              objectId: obj.idObject,
              componentType: obj.componentType || '',
              kKat: obj.kKat ?? ''
            })));
          }
          setDataLoaded(true);
        } catch (err) {
          setFetchError('Nepavyko gauti įvykio duomenų.');
          console.error(err);
        } finally {
          setLoading(false);
        }
      };
      fetchEvent();
    }
  }, [id, isEditing]);

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handlePolygonChange = (polygon) => {
    setFormData({ ...formData, polygon });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!formData.polygon) {
      alert('Prašome pažymėti paveiktą teritoriją žemėlapyje.');
      return;
    }

    try {
      setLoading(true);
      const payload = {
        ...formData,
        polygon: JSON.stringify(formData.polygon),
        eventObjects: selectedEventObjects.map(eo => ({
          objectId: Number(eo.objectId),
          componentType: eo.componentType || null,
          kKat: eo.kKat !== '' ? parseFloat(eo.kKat) : null
        })),
        materials: []
      };
      if (isEditing) {
        await api.put(`/events/${id}`, payload);
      } else {
        await api.post('/events', payload);
      }
      navigate('/events');
    } catch (err) {
      setSubmitError('Klaida išsaugant įvykį.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (fetchError) return <div style={{ color: 'red' }}>{fetchError}</div>;

  return (
    <div>
      <h2>{isEditing ? 'Redaguoti įvykį' : 'Naujas įvykis'}</h2>
      {submitError && <div style={{ color: 'red', marginBottom: '8px' }}>{submitError}</div>}
      <form onSubmit={handleSubmit}>
        <div>
          <label>Tipas:</label>
          <select name="eventType" value={formData.eventType} onChange={handleChange} required>
            <option value="gaisas">Gaisras</option>
            <option value="medžiagų išsiliejimas">Medžiagų išsiliejimas</option>
            <option value="stichija">Stichija</option>
          </select>
        </div>

        <div>
          <label>Data:</label>
          <input type="date" name="eventDate" value={formData.eventDate} onChange={handleChange} required />
        </div>

        <div>
          <label>Aprašymas:</label>
          <textarea name="description" value={formData.description} onChange={handleChange} rows="3" />
        </div>

        <div>
          <label>Vieta (tekstas):</label>
          <input type="text" name="location" value={formData.location} onChange={handleChange} />
        </div>

        <div>
          <label>Pažymėkite paveiktą teritoriją:</label>
          {dataLoaded
            ? <PolygonPicker onPolygonChange={handlePolygonChange} initialPolygon={formData.polygon} />
            : <div>Kraunama žemėlapį...</div>
          }
          {formData.polygon && <div>Teritorija pažymėta.</div>}
        </div>

        <ObjectSelector
          selectedEventObjects={selectedEventObjects}
          onEventObjectsChange={setSelectedEventObjects}
          specialistMode={isEditing || isSpecialist}
        />

        <button type="submit" disabled={loading}>
          {loading ? 'Saugoma...' : (isEditing ? 'Atnaujinti' : 'Sukurti')}
        </button>
        <button type="button" onClick={() => navigate('/events')}>Atšaukti</button>
      </form>
    </div>
  );
};

export default EventForm;
