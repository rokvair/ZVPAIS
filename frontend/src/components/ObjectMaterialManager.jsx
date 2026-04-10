import React, { useState, useEffect } from 'react';
import api from '../services/api';

const ObjectMaterialManager = ({ objectId }) => {
  const [materials, setMaterials] = useState([]);
  const [allMaterials, setAllMaterials] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showAddForm, setShowAddForm] = useState(false);
  const [selectedMaterialId, setSelectedMaterialId] = useState('');
  const [percentage, setPercentage] = useState('');
  const [mass, setMass] = useState('');
  const [volume, setVolume] = useState('');

  useEffect(() => {
    fetchObjectMaterials();
    fetchAllMaterials();
  }, [objectId]);

  const fetchObjectMaterials = async () => {
    try {
      const response = await api.get(`/environmentobjects/${objectId}/materials`);
      setMaterials(response.data);
    } catch (error) {
      console.error(error);
    }
  };

  const fetchAllMaterials = async () => {
    try {
      const response = await api.get('/materials');
      setAllMaterials(response.data);
      setLoading(false);
    } catch (error) {
      console.error(error);
    }
  };

  const handleAddMaterial = async (e) => {
    e.preventDefault();
    if (!selectedMaterialId) return;
    try {
      await api.post(`/environmentobjects/${objectId}/materials`, {
        materialId: parseInt(selectedMaterialId),
        percentage: percentage ? parseFloat(percentage) : null,
        mass: mass ? parseFloat(mass) : null,
        volume: volume ? parseFloat(volume) : null
      });
      // Išvalyti formą ir atnaujinti sąrašą
      setSelectedMaterialId('');
      setPercentage('');
      setMass('');
      setVolume('');
      setShowAddForm(false);
      fetchObjectMaterials();
    } catch (error) {
      alert('Klaida pridedant medžiagą');
      console.error(error);
    }
  };

  const handleRemoveMaterial = async (materialId) => {
    if (!window.confirm('Pašalinti šią medžiagą?')) return;
    try {
      await api.delete(`/environmentobjects/${objectId}/materials/${materialId}`);
      fetchObjectMaterials();
    } catch (error) {
      alert('Klaida šalinant medžiagą');
      console.error(error);
    }
  };

  if (loading) return <div>Kraunama...</div>;

  return (
    <div>
      {materials.length === 0 ? (
        <p>Šis objektas neturi medžiagų.</p>
      ) : (
        <ul>
          {materials.map(m => (
            <li key={m.idObjectMaterial}>
              {m.materialName} – 
              {m.percentage !== null && `${m.percentage}%`}
              {m.mass !== null && ` ${m.mass} kg`}
              {m.volume !== null && ` ${m.volume} l`}
              <button onClick={() => handleRemoveMaterial(m.materialId)} style={{ marginLeft: '10px' }}>
                Pašalinti
              </button>
            </li>
          ))}
        </ul>
      )}

      {!showAddForm ? (
        <button onClick={() => setShowAddForm(true)}>Pridėti medžiagą</button>
      ) : (
        <form onSubmit={handleAddMaterial} style={{ marginTop: '10px' }}>
          <select
            value={selectedMaterialId}
            onChange={(e) => setSelectedMaterialId(e.target.value)}
            required
          >
            <option value="">Pasirinkite medžiagą</option>
            {allMaterials.map(m => (
              <option key={m.idMaterial} value={m.idMaterial}>{m.name}</option>
            ))}
          </select>
          <input
            type="number"
            placeholder="Procentas (%)"
            value={percentage}
            onChange={e => setPercentage(e.target.value)}
            step="any"
          />
          <input
            type="number"
            placeholder="Masė (kg)"
            value={mass}
            onChange={e => setMass(e.target.value)}
            step="any"
          />
          <input
            type="number"
            placeholder="Tūris (l)"
            value={volume}
            onChange={e => setVolume(e.target.value)}
            step="any"
          />
          <button type="submit">Išsaugoti</button>
          <button type="button" onClick={() => setShowAddForm(false)}>Atšaukti</button>
        </form>
      )}
    </div>
  );
};

export default ObjectMaterialManager;