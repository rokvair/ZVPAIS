import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [form, setForm] = useState({
    email: '',
    password: '',
    isSpecialist: false,
    name: '',
    fieldOfExpertise: ''
  });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleChange = (e) => {
    const value = e.target.type === 'checkbox' ? e.target.checked : e.target.value;
    setForm({ ...form, [e.target.name]: value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await register(form);
      navigate('/');
    } catch (err) {
      setError(err.response?.data || 'Registracijos klaida.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-container">
      <div className="auth-card">
        <h2>Registracija</h2>
        {error && <div className="auth-error">{error}</div>}
        <form onSubmit={handleSubmit}>
          <div>
            <label>El. paštas</label>
            <input
              type="email"
              name="email"
              value={form.email}
              onChange={handleChange}
              required
              autoFocus
            />
          </div>
          <div>
            <label>Slaptažodis</label>
            <input
              type="password"
              name="password"
              value={form.password}
              onChange={handleChange}
              required
              minLength={6}
            />
          </div>
          <div className="auth-checkbox-row">
            <input
              type="checkbox"
              name="isSpecialist"
              id="isSpecialist"
              checked={form.isSpecialist}
              onChange={handleChange}
            />
            <label htmlFor="isSpecialist">Registruotis kaip specialistas</label>
          </div>
          {form.isSpecialist && (
            <>
              <div>
                <label>Vardas *</label>
                <input
                  type="text"
                  name="name"
                  value={form.name}
                  onChange={handleChange}
                  required
                />
              </div>
              <div>
                <label>Specializacija</label>
                <input
                  type="text"
                  name="fieldOfExpertise"
                  value={form.fieldOfExpertise}
                  onChange={handleChange}
                />
              </div>
            </>
          )}
          <button type="submit" disabled={loading} className="auth-btn">
            {loading ? 'Registruojama...' : 'Registruotis'}
          </button>
        </form>
        <p className="auth-link">
          Jau turite paskyrą? <Link to="/login">Prisijungti</Link>
        </p>
      </div>
    </div>
  );
}
