import React, { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import api from '../services/api';

const MaterialForm = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEditing = !!id;

  const [formData, setFormData] = useState({
    name: '',
    description: '',
    toxicityFactor: '',
    unit: '',
    baseRate: '',
    substanceType: 'standard',
    emissionCategory: ''
  });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (isEditing) {
      const fetchMaterial = async () => {
        try {
          const response = await api.get(`/materials/${id}`);
          const m = response.data;
          setFormData({
            name: m.name || '',
            description: m.description || '',
            toxicityFactor: m.toxicityFactor ?? '',
            unit: m.unit || '',
            baseRate: m.baseRate ?? '',
            substanceType: m.substanceType || 'standard',
            emissionCategory: m.emissionCategory || ''
          });
        } catch (error) {
          console.error(error);
        }
      };
      fetchMaterial();
    }
  }, [id, isEditing]);

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setLoading(true);
      const payload = {
        name: formData.name,
        description: formData.description,
        toxicityFactor: formData.toxicityFactor ? parseFloat(formData.toxicityFactor) : null,
        unit: formData.unit,
        baseRate: formData.baseRate ? parseFloat(formData.baseRate) : null,
        substanceType: formData.substanceType || 'standard',
        emissionCategory: formData.emissionCategory || null
      };
      if (isEditing) {
        await api.put(`/materials/${id}`, payload);
      } else {
        await api.post('/materials', payload);
      }
      navigate('/materials');
    } catch (error) {
      alert('Klaida išsaugant medžiagą.');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h2>{isEditing ? 'Redaguoti medžiagą' : 'Nauja medžiaga'}</h2>
      <form onSubmit={handleSubmit}>
        <div>
          <label>Pavadinimas:</label>
          <input name="name" value={formData.name} onChange={handleChange} required />
        </div>
        <div>
          <label>Aprašymas:</label>
          <textarea name="description" value={formData.description} onChange={handleChange} />
        </div>
        <div>
          <label>Toksiškumo faktorius:</label>
          <input type="number" step="0.01" name="toxicityFactor" value={formData.toxicityFactor} onChange={handleChange} />
        </div>
        <div>
          <label>Vienetas:</label>
          <input name="unit" value={formData.unit} onChange={handleChange} />
        </div>
        <div>
          <label>Bazinis tarifas T_n (EUR/t):</label>
          <input type="number" step="0.01" name="baseRate" value={formData.baseRate} onChange={handleChange} />
        </div>
        <div>
          <label>Medžiagos tipas:</label>
          <select name="substanceType" value={formData.substanceType} onChange={handleChange}>
            <option value="standard">Standartinis</option>
            <option value="bds7">BDS₇</option>
            <option value="suspended">Suspenduotos medžiagos</option>
          </select>
        </div>
        <div>
          <label>Emisijų kategorija (vėjo sklaidai):</label>
          <select name="emissionCategory" value={formData.emissionCategory} onChange={handleChange}>
            <option value="">— nenaudojama sklaidai —</option>
            <option value="polymers">Polimerai (polymers)</option>
            <option value="plastics">Plastikai (plastics)</option>
            <option value="resins">Dervos (resins)</option>
            <option value="paper">Popierius / kartonas (paper)</option>
            <option value="textile">Tekstilė (textile)</option>
          </select>
          <small style={{ color: '#888' }}>Nustato emisijų faktorių šiai medžiagai gaisro sklaidoje.</small>
        </div>
        <button type="submit" disabled={loading}>{loading ? 'Saugoma...' : 'Išsaugoti'}</button>
        <button type="button" onClick={() => navigate('/materials')}>Atšaukti</button>
      </form>
    </div>
  );
};

export default MaterialForm;
