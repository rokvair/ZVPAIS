import React, { useState, useEffect } from 'react';
import api from '../services/api';

// selectedEventObjects: [{objectId, componentType, kKat}]
// onEventObjectsChange: (updatedList) => void
const ObjectSelector = ({ selectedEventObjects, onEventObjectsChange, specialistMode = false }) => {
  const [objects, setObjects] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
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
    fetchObjects();
  }, []);

  const isSelected = (objectId) =>
    selectedEventObjects.some(eo => eo.objectId === objectId);

  const getEntry = (objectId) =>
    selectedEventObjects.find(eo => eo.objectId === objectId) || { objectId, componentType: '', kKat: '' };

  const handleCheckboxChange = (objectId) => {
    if (isSelected(objectId)) {
      onEventObjectsChange(selectedEventObjects.filter(eo => eo.objectId !== objectId));
    } else {
      onEventObjectsChange([...selectedEventObjects, { objectId, componentType: '', kKat: '' }]);
    }
  };

  const handleFieldChange = (objectId, field, value) => {
    onEventObjectsChange(selectedEventObjects.map(eo =>
      eo.objectId === objectId ? { ...eo, [field]: value } : eo
    ));
  };

  if (loading) return <div>Kraunami objektai...</div>;

  return (
    <div>
      <h3>Objektai</h3>
      {objects.map(obj => (
        <div key={obj.idObject} style={{ marginBottom: '8px' }}>
          <label>
            <input
              type="checkbox"
              checked={isSelected(obj.idObject)}
              onChange={() => handleCheckboxChange(obj.idObject)}
            />
            {' '}{obj.name}{obj.description && ` (${obj.description})`}
          </label>
          {isSelected(obj.idObject) && specialistMode && (
            <span style={{ marginLeft: '16px' }}>
              <label>
                Komponentas:{' '}
                <select
                  value={getEntry(obj.idObject).componentType}
                  onChange={(e) => handleFieldChange(obj.idObject, 'componentType', e.target.value)}
                  style={{ marginRight: '12px' }}
                >
                  <option value="">— Nepasirinkta —</option>
                  <option value="water">Vanduo</option>
                  <option value="soil">Žemė</option>
                  <option value="air">Oras</option>
                </select>
              </label>
              <label>
                K_kat:{' '}
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  value={getEntry(obj.idObject).kKat}
                  onChange={(e) => handleFieldChange(obj.idObject, 'kKat', e.target.value)}
                  placeholder="pvz. 1.0"
                  style={{ width: '70px' }}
                />
              </label>
            </span>
          )}
        </div>
      ))}
    </div>
  );
};

export default ObjectSelector;
