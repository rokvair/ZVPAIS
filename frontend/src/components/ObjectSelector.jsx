import React, { useState, useEffect } from 'react';
import api from '../services/api';

const ObjectSelector = ({ selectedObjectIds, onObjectIdsChange }) => {
  const [objects, setObjects] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchObjects = async () => {
      try {
        const response = await api.get('/environmentobjects'); // atkreipkite dėmesį į URL
        setObjects(response.data);
      } catch (error) {
        console.error('Klaida kraunant objektus:', error);
      } finally {
        setLoading(false);
      }
    };
    fetchObjects();
  }, []);

  const handleCheckboxChange = (objectId) => {
    if (selectedObjectIds.includes(objectId)) {
      onObjectIdsChange(selectedObjectIds.filter(id => id !== objectId));
    } else {
      onObjectIdsChange([...selectedObjectIds, objectId]);
    }
  };

  if (loading) return <div>Kraunami objektai...</div>;

  return (
    <div>
      <h3>Objektai</h3>
      {objects.map(obj => (
        <label key={obj.idObject} style={{ display: 'block', marginBottom: '5px' }}>
          <input
            type="checkbox"
            checked={selectedObjectIds.includes(obj.idObject)}
            onChange={() => handleCheckboxChange(obj.idObject)}
          />
          {obj.name} {obj.description && `(${obj.description})`}
        </label>
      ))}
    </div>
  );
};

export default ObjectSelector;