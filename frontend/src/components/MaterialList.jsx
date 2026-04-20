import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const MaterialList = () => {
  const { isSpecialist } = useAuth();
  const [materials, setMaterials] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchMaterials();
  }, []);

  const fetchMaterials = async () => {
    try {
      const response = await api.get('/materials');
      setMaterials(response.data);
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Ar tikrai norite ištrinti?')) return;
    try {
      await api.delete(`/materials/${id}`);
      setMaterials(materials.filter(m => m.idMaterial !== id));
    } catch (error) {
      alert('Klaida trinant medžiagą.');
    }
  };

  if (loading) return <div>Kraunama...</div>;

  return (
    <div>
      <h2>Medžiagų sąrašas</h2>
      {isSpecialist && <Link to="/materials/new">Sukurti naują medžiagą</Link>}
      <table border="1" cellPadding="8" style={{ marginTop: '20px' }}>
        <thead>
          <tr>
            <th>ID</th>
            <th>Pavadinimas</th>
            <th>Tipas</th>
            <th>T_n (€/t)</th>
            <th>Vienetas</th>
            {isSpecialist && <th>Veiksmai</th>}
          </tr>
        </thead>
        <tbody>
          {materials.map(m => (
            <tr key={m.idMaterial}>
              <td>{m.idMaterial}</td>
              <td>{m.name}</td>
              <td>{substanceLabel(m.substanceType)}</td>
              <td>{m.baseRate ?? '—'}</td>
              <td>{m.unit}</td>
              {isSpecialist && (
                <td>
                  <Link to={`/materials/edit/${m.idMaterial}`}>Redaguoti</Link>
                  <button onClick={() => handleDelete(m.idMaterial)} style={{ marginLeft: '8px' }}>
                    Trinti
                  </button>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

const substanceLabel = (t) => ({ standard: 'Standartinis', bds7: 'BDS₇', suspended: 'Suspenduotos' }[t] || '—');

export default MaterialList;