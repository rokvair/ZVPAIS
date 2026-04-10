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

  useEffect(() => {
    if (isEditing) {
      const fetchObject = async () => {
        try {
          const response = await api.get(`/environmentobjects/${id}`);
          const obj = response.data;
          setName(obj.name || '');
          setDescription(obj.description || '');
          setTotalMass(obj.totalMass ?? '');
          setTotalVolume(obj.totalVolume ?? '');
        } catch (error) {
          console.error(error);
        }
      };
      fetchObject();
    }
  }, [id, isEditing]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      const payload = {
        name,
        description,
        totalMass: totalMass ? parseFloat(totalMass) : null,
        totalVolume: totalVolume ? parseFloat(totalVolume) : null
      };
      if (isEditing) {
        await api.put(`/environmentobjects/${id}`, payload);
      } else {
        await api.post('/environmentobjects', payload);
      }
      navigate('/objects');
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
          <input value={name} onChange={(e) => setName(e.target.value)} required />
        </div>
        <div>
          <label>Aprašymas:</label>
          <textarea value={description} onChange={(e) => setDescription(e.target.value)} rows="3" />
        </div>
        <div>
          <label>Bendra masė (kg):</label>
          <input
            type="number"
            step="any"
            value={totalMass}
            onChange={(e) => setTotalMass(e.target.value)}
            placeholder="Palikite tuščią, jei netaikoma"
          />
        </div>
        <div>
          <label>Bendras tūris (l):</label>
          <input
            type="number"
            step="any"
            value={totalVolume}
            onChange={(e) => setTotalVolume(e.target.value)}
            placeholder="Palikite tuščią, jei netaikoma"
          />
        </div>
        <button type="submit" disabled={loading}>
          {loading ? 'Saugoma...' : (isEditing ? 'Atnaujinti' : 'Sukurti')}
        </button>
        <button type="button" onClick={() => navigate('/objects')}>Atšaukti</button>
      </form>

      {isEditing && (
        <div style={{ marginTop: '30px', borderTop: '1px solid #ccc', paddingTop: '20px' }}>
          <h3>Objekto medžiagos</h3>
          <ObjectMaterialManager objectId={parseInt(id)} />
        </div>
      )}
    </div>
  );
};

export default ObjectForm;