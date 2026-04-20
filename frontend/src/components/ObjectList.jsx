import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const ObjectList = () => {
  const { isSpecialist } = useAuth();
  const [objects, setObjects] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchObjects();
  }, []);

  const fetchObjects = async () => {
    try {
      const response = await api.get('/environmentobjects');
      setObjects(response.data);
    } catch (error) {
      console.error('Klaida kraunant objektus:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Ar tikrai norite ištrinti šį objektą?')) return;
    try {
      await api.delete(`/environmentobjects/${id}`);
      setObjects(objects.filter(obj => obj.idObject !== id));
    } catch (error) {
      alert('Klaida trinant objektą.');
      console.error(error);
    }
  };

  const formatMaterial = (m) => {
    const parts = [];
    if (m.mass != null) parts.push(`${m.mass} t`);
    if (m.volume != null) parts.push(`${m.volume} m³`);
    if (m.percentage != null) parts.push(`${m.percentage}%`);
    if (m.recoveredQuantity != null) parts.push(`susigrąžinta ${m.recoveredQuantity} t`);
    return `${m.materialName} (${parts.join(', ')})`;
  };

  if (loading) return <div>Kraunama...</div>;

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '12px' }}>
        <h2 style={{ margin: 0 }}>Aplinkos objektai</h2>
        {isSpecialist && <Link to="/objects/new">+ Naujas objektas</Link>}
      </div>
      {objects.length === 0 ? (
        <p>Nėra objektų.</p>
      ) : (
        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9em' }}>
          <thead>
            <tr style={{ background: '#f5f5f5' }}>
              <th style={th}>ID</th>
              <th style={th}>Pavadinimas</th>
              <th style={th}>Aprašymas</th>
              <th style={th}>Medžiagos</th>
              {isSpecialist && <th style={th}>Veiksmai</th>}
            </tr>
          </thead>
          <tbody>
            {objects.map(obj => (
              <tr key={obj.idObject} style={{ borderBottom: '1px solid #eee' }}>
                <td style={td}>{obj.idObject}</td>
                <td style={td}><strong>{obj.name}</strong></td>
                <td style={{ ...td, color: '#555', maxWidth: '200px' }}>
                  {obj.description || '—'}
                  {obj.totalMass != null && <div style={{ fontSize: '0.8em', color: '#888' }}>Bendras svoris: {obj.totalMass} t</div>}
                  {obj.totalVolume != null && <div style={{ fontSize: '0.8em', color: '#888' }}>Bendras tūris: {obj.totalVolume} m³</div>}
                </td>
                <td style={td}>
                  {obj.materials && obj.materials.length > 0 ? (
                    <ul style={{ margin: 0, paddingLeft: '16px' }}>
                      {obj.materials.map(m => (
                        <li key={m.idObjectMaterial} style={{ fontSize: '0.85em' }}>
                          {formatMaterial(m)}
                        </li>
                      ))}
                    </ul>
                  ) : (
                    <span style={{ color: '#aaa', fontSize: '0.85em' }}>Nėra medžiagų</span>
                  )}
                </td>
                {isSpecialist && (
                  <td style={td}>
                    <Link to={`/objects/edit/${obj.idObject}`}>Redaguoti</Link>
                    <button onClick={() => handleDelete(obj.idObject)} style={{ marginLeft: '8px' }}>
                      Trinti
                    </button>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
};

const th = { padding: '8px 10px', textAlign: 'left', borderBottom: '2px solid #ddd' };
const td = { padding: '8px 10px', verticalAlign: 'top' };

export default ObjectList;
