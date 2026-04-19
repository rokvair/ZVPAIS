import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../services/api';
import ObjectMaterialManager from './ObjectMaterialManager';

const ObjectForm = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEditing = !!id;

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [totalMass, setTotalMass] = useState('');
  const [totalVolume, setTotalVolume] = useState('');
  const [loading, setLoading] = useState(false);
  const [savedId, setSavedId] = useState(isEditing ? parseInt(id) : null);

  useEffect(() => {
    if (isEditing) {
      api.get(`/environmentobjects/${id}`)
        .then(res => {
          setName(res.data.name || '');
          setDescription(res.data.description || '');
          setTotalMass(res.data.totalMass != null ? String(res.data.totalMass) : '');
          setTotalVolume(res.data.totalVolume != null ? String(res.data.totalVolume) : '');
        })
        .catch(console.error);
    }
  }, [id, isEditing]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      const payload = {
        name,
        description,
        totalMass: totalMass !== '' ? parseFloat(totalMass) : null,
        totalVolume: totalVolume !== '' ? parseFloat(totalVolume) : null
      };
      if (isEditing) {
        await api.put(`/environmentobjects/${id}`, payload);
        setSavedId(parseInt(id));
      } else {
        const res = await api.post('/environmentobjects', payload);
        const newId = res.data.idObject;
        setSavedId(newId);
      }
    } catch (error) {
      alert('Klaida išsaugant objektą.');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h2>{isEditing ? 'Redaguoti objektą' : 'Naujas objektas'}</h2>
      <form onSubmit={handleSubmit}>
        <div>
          <label>Pavadinimas:</label>
          <input
            value={name}
            onChange={(e) => setName(e.target.value)}
            required
            style={{ marginLeft: '8px', width: '300px' }}
          />
        </div>
        <div style={{ marginTop: '8px' }}>
          <label>Aprašymas:</label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            rows="3"
            style={{ marginLeft: '8px', width: '300px', display: 'inline-block', verticalAlign: 'top' }}
          />
        </div>
        <div style={{ marginTop: '8px', color: '#666', fontSize: '0.85em' }}>
          Užpildykite, jei medžiagos nurodomos procentais:
        </div>
        <div style={{ marginTop: '4px' }}>
          <label>Bendras svoris (t):</label>
          <input
            type="number"
            step="any"
            min="0"
            value={totalMass}
            onChange={e => setTotalMass(e.target.value)}
            placeholder="pvz. 5.0"
            style={{ marginLeft: '8px', width: '120px' }}
          />
          <label style={{ marginLeft: '16px' }}>Bendras tūris (m³):</label>
          <input
            type="number"
            step="any"
            min="0"
            value={totalVolume}
            onChange={e => setTotalVolume(e.target.value)}
            placeholder="pvz. 5.0"
            style={{ marginLeft: '8px', width: '120px' }}
          />
        </div>

        <div style={{ marginTop: '12px' }}>
          <button type="submit" disabled={loading}>
            {loading ? 'Saugoma...' : (isEditing ? 'Atnaujinti' : 'Sukurti ir pridėti medžiagas')}
          </button>
          <button type="button" onClick={() => navigate('/objects')} style={{ marginLeft: '8px' }}>
            {savedId ? 'Grįžti į sąrašą' : 'Atšaukti'}
          </button>
        </div>
      </form>

      {savedId && (
        <div style={{ marginTop: '30px', borderTop: '1px solid #ccc', paddingTop: '20px' }}>
          <h3>Objekto medžiagos</h3>
          <ObjectMaterialManager objectId={savedId} />
        </div>
      )}
    </div>
  );
};

export default ObjectForm;
