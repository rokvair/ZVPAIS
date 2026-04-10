import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { Link } from 'react-router-dom';

const ObjectList = () => {
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

  if (loading) return <div>Kraunama...</div>;

  return (
    <div>
      <h2>Objektų sąrašas</h2>
      <Link to="/objects/new">Sukurti naują objektą</Link>
      <table border="1" cellPadding="8" style={{ marginTop: '20px', width: '100%' }}>
        <thead>
          <tr>
            <th>ID</th>
            <th>Pavadinimas</th>
            <th>Aprašymas</th>
            <th>Veiksmai</th>
          </tr>
        </thead>
        <tbody>
          {objects.map(obj => (
            <tr key={obj.idObject}>
              <td>{obj.idObject}</td>
              <td>{obj.name}</td>
              <td>{obj.description}</td>
              <td>
                <Link to={`/objects/edit/${obj.idObject}`}>Redaguoti</Link>
                <button onClick={() => handleDelete(obj.idObject)} style={{ marginLeft: '8px' }}>
                  Trinti
                </button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default ObjectList;