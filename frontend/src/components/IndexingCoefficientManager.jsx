import React, { useEffect, useState } from 'react';
import api from '../services/api';

const empty = { year: new Date().getFullYear(), quarter: 1, coefficient: '' };

const IndexingCoefficientManager = () => {
  const [coefficients, setCoefficients] = useState([]);
  const [form, setForm] = useState(empty);
  const [editingId, setEditingId] = useState(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchAll();
  }, []);

  const fetchAll = async () => {
    try {
      const res = await api.get('/calculation/indexing-coefficients');
      setCoefficients(res.data);
    } catch (err) {
      setError('Nepavyko gauti koeficientų.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const startEdit = (c) => {
    setEditingId(c.idIndexingCoefficient);
    setForm({ year: c.year, quarter: c.quarter, coefficient: String(c.coefficient) });
    setError('');
  };

  const cancelEdit = () => {
    setEditingId(null);
    setForm(empty);
    setError('');
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const payload = {
      year: Number(form.year),
      quarter: Number(form.quarter),
      coefficient: parseFloat(form.coefficient)
    };
    try {
      setSaving(true);
      if (editingId) {
        const res = await api.put(`/calculation/indexing-coefficients/${editingId}`, payload);
        setCoefficients(coefficients.map(c =>
          c.idIndexingCoefficient === editingId ? res.data : c
        ));
      } else {
        const res = await api.post('/calculation/indexing-coefficients', payload);
        setCoefficients([res.data, ...coefficients]);
      }
      cancelEdit();
    } catch (err) {
      setError(err.response?.data || 'Klaida išsaugant koeficientą.');
      console.error(err);
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Trinti šį koeficientą?')) return;
    try {
      await api.delete(`/calculation/indexing-coefficients/${id}`);
      setCoefficients(coefficients.filter(c => c.idIndexingCoefficient !== id));
    } catch (err) {
      alert('Klaida trinant koeficientą.');
      console.error(err);
    }
  };

  if (loading) return <div>Kraunama...</div>;

  return (
    <div>
      <h2>Indeksavimo koeficientai (I_n)</h2>
      <p style={{ color: '#555', maxWidth: '600px' }}>
        I_n — ketvirčio indeksavimo koeficientas pagal metodiką. Skaičiavime naudojamas naujausias įrašas.
        Jei lentelė tuščia, naudojama reikšmė 1.0.
      </p>

      <h3 style={{ marginTop: '24px' }}>{editingId ? 'Redaguoti koeficientą' : 'Pridėti naują koeficientą'}</h3>
      {error && <div style={{ color: 'red', marginBottom: '8px' }}>{error}</div>}
      <form onSubmit={handleSubmit} style={{ display: 'flex', gap: '12px', alignItems: 'flex-end', flexWrap: 'wrap' }}>
        <div>
          <label style={{ display: 'block', fontSize: '0.85em' }}>Metai</label>
          <input
            type="number"
            name="year"
            value={form.year}
            onChange={handleChange}
            min="2000"
            max="2100"
            required
            style={{ width: '80px' }}
          />
        </div>
        <div>
          <label style={{ display: 'block', fontSize: '0.85em' }}>Ketvirtis</label>
          <select name="quarter" value={form.quarter} onChange={handleChange} required>
            <option value={1}>I</option>
            <option value={2}>II</option>
            <option value={3}>III</option>
            <option value={4}>IV</option>
          </select>
        </div>
        <div>
          <label style={{ display: 'block', fontSize: '0.85em' }}>Koeficientas (I_n)</label>
          <input
            type="number"
            name="coefficient"
            value={form.coefficient}
            onChange={handleChange}
            step="0.0001"
            min="0.0001"
            placeholder="pvz. 1.5"
            required
            style={{ width: '100px' }}
          />
        </div>
        <button type="submit" disabled={saving}>
          {saving ? 'Saugoma...' : (editingId ? 'Atnaujinti' : 'Pridėti')}
        </button>
        {editingId && (
          <button type="button" onClick={cancelEdit}>Atšaukti</button>
        )}
      </form>

      {coefficients.length === 0 ? (
        <p style={{ marginTop: '20px', color: '#888' }}>Nėra įrašų. Skaičiavime naudojama I_n = 1.0.</p>
      ) : (
        <table border="1" cellPadding="8" style={{ marginTop: '20px', borderCollapse: 'collapse' }}>
          <thead>
            <tr>
              <th>Metai</th>
              <th>Ketvirtis</th>
              <th>I_n</th>
              <th>Įvesta</th>
              <th>Veiksmai</th>
            </tr>
          </thead>
          <tbody>
            {coefficients.map((c, i) => (
              <tr key={c.idIndexingCoefficient} style={{ background: i === 0 ? '#f0fff0' : 'white' }}>
                <td>{c.year}</td>
                <td>{'I'.repeat(c.quarter)}</td>
                <td><strong>{Number(c.coefficient).toFixed(4)}</strong></td>
                <td>{new Date(c.createdAt).toLocaleDateString('lt-LT')}</td>
                <td>
                  <button onClick={() => startEdit(c)} style={{ marginRight: '6px' }}>Redaguoti</button>
                  <button onClick={() => handleDelete(c.idIndexingCoefficient)}>Trinti</button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
      {coefficients.length > 0 && (
        <p style={{ fontSize: '0.85em', color: '#666', marginTop: '6px' }}>
          Žalia eilutė — šiuo metu naudojamas koeficientas skaičiavime.
        </p>
      )}
    </div>
  );
};

export default IndexingCoefficientManager;
