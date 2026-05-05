import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useLanguage } from '../context/LanguageContext';

const MaterialList = () => {
  const { isSpecialist } = useAuth();
  const { t } = useLanguage();
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
    if (!window.confirm(t('mat_confirm_delete'))) return;
    try {
      await api.delete(`/materials/${id}`);
      setMaterials(materials.filter(m => m.idMaterial !== id));
    } catch (error) {
      alert(t('mat_delete_error'));
    }
  };

  const substanceLabel = (type) => ({
    standard: t('mat_standard'),
    bds7: t('mat_bds7'),
    suspended: t('mat_suspended'),
  }[type] || '—');

  const categoryLabel = (c) => ({
    polymers:       t('mat_polymers'),
    plastics:       t('mat_plastics'),
    resins:         t('mat_resins'),
    paper:          t('mat_paper'),
    textile:        t('mat_textile'),
    wood:           t('mat_wood'),
    oil:            t('mat_oil'),
    rubber:         t('mat_rubber'),
    liquid_fuel:    t('mat_liquid_fuel'),
    carbon:         t('mat_carbon'),
    halogenated:    t('mat_halogenated'),
    liquid_organic: t('mat_liquid_organic'),
  }[c] || '—');

  if (loading) return <div>{t('loading')}</div>;

  return (
    <div>
      <h2>{t('mat_list_title')}</h2>
      {isSpecialist && <Link to="/materials/new">{t('mat_new_btn')}</Link>}
      <table border="1" cellPadding="8" style={{ marginTop: '20px' }}>
        <thead>
          <tr>
            <th>ID</th>
            <th>{t('name_col')}</th>
            <th>{t('mat_type_col')}</th>
            <th>{t('mat_emission_col')}</th>
            <th>T_n (€/t)</th>
            <th>{t('unit')}</th>
            {isSpecialist && <th>{t('actions')}</th>}
          </tr>
        </thead>
        <tbody>
          {materials.map(m => (
            <tr key={m.idMaterial}>
              <td>{m.idMaterial}</td>
              <td>{m.name}</td>
              <td>{substanceLabel(m.substanceType)}</td>
              <td>{categoryLabel(m.emissionCategory)}</td>
              <td>{m.baseRate ?? '—'}</td>
              <td>{m.unit}</td>
              {isSpecialist && (
                <td>
                  <Link to={`/materials/edit/${m.idMaterial}`}>{t('edit')}</Link>
                  <button onClick={() => handleDelete(m.idMaterial)} style={{ marginLeft: '8px' }}>
                    {t('delete')}
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

export default MaterialList;
