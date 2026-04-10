import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../services/api';
import PolygonPicker from './PolygonPicker';
import ObjectSelector from './ObjectSelector';

const EventForm = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEditing = !!id;

  const [formData, setFormData] = useState({
    eventType: 'gaisas',
    eventDate: '',
    description: '',
    location: '',
    polygon: null,
    status: 'naujas'
  });
  const [selectedObjectIds, setSelectedObjectIds] = useState([]);
  const [sensitivityFactor, setSensitivityFactor] = useState('1.0');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

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
            polygon: event.polygon,
            status: event.status || 'naujas'
          });
          if (event.objects) {
            setSelectedObjectIds(event.objects.map(obj => obj.idObject));
          }
          // Load sensitivity factor if exists, default to '1.0'
          if (event.sensitivityFactor !== undefined && event.sensitivityFactor !== null) {
            setSensitivityFactor(event.sensitivityFactor.toString());
          } else {
            setSensitivityFactor('1.0');
          }
        } catch (err) {
          setError('Nepavyko gauti įvykio duomenų.');
          console.error(err);
        } finally {
          setLoading(false);
        }
      };
      fetchEvent();
    }
  }, [id, isEditing]);

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handlePolygonChange = (polygon) => {
    setFormData({
      ...formData,
      polygon: polygon
    });
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
        objectIds: selectedObjectIds.map(id => Number(id)),
        materials: [], // required empty list
        sensitivityFactor: parseFloat(sensitivityFactor) // add sensitivity factor
      };
      if (isEditing) {
        await api.put(`/events/${id}`, payload);
      } else {
        await api.post('/events', payload);
      }
      navigate('/events');
    } catch (err) {
      setError('Klaida išsaugant įvykį.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div>Kraunama...</div>;
  if (error) return <div style={{ color: 'red' }}>{error}</div>;

  return (
    <div>
      <h2>{isEditing ? 'Redaguoti įvykį' : 'Naujas įvykis'}</h2>
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
          <label>Statusas:</label>
          <select name="status" value={formData.status} onChange={handleChange}>
            <option value="naujas">Naujas</option>
            <option value="tikrinamas">Tikrinamas</option>
            <option value="patvirtintas">Patvirtintas</option>
            <option value="atmestas">Atmestas</option>
          </select>
        </div>

        <div>
          <label>Teritorijos jautrumas:</label>
          <select value={sensitivityFactor} onChange={(e) => setSensitivityFactor(e.target.value)}>
            <option value="1.0">Įprasta</option>
            <option value="1.5">Jautri</option>
            <option value="2.0">Ypač jautri</option>
          </select>
        </div>

        <div>
          <label>Pažymėkite paveiktą teritoriją:</label>
          <PolygonPicker onPolygonChange={handlePolygonChange} />
          {formData.polygon && <div>Teritorija pažymėta.</div>}
        </div>

        <ObjectSelector 
          selectedObjectIds={selectedObjectIds} 
          onObjectIdsChange={setSelectedObjectIds} 
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