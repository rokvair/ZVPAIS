import React, { useEffect, useState } from 'react';
import api from '../services/api';
import { Link } from 'react-router-dom';

const EventList = () => {
  const [events, setEvents] = useState([]);
  const [damages, setDamages] = useState({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    fetchEvents();
  }, []);

  const fetchEvents = async () => {
    try {
      setLoading(true);
      const response = await api.get('/events');
      setEvents(response.data);
      setError('');
      
      // Po to, kai gavome įvykius, užkrauname kiekvieno žalą
      response.data.forEach(event => {
        fetchDamage(event.idEvent);
      });
    } catch (err) {
      setError('Nepavyko gauti įvykių sąrašo.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const fetchDamage = async (eventId) => {
    try {
      const response = await api.get(`/events/${eventId}/damage`);
      setDamages(prev => ({ ...prev, [eventId]: response.data }));
    } catch (err) {
      console.error(`Klaida gaunant žalą įvykiui ${eventId}:`, err);
      setDamages(prev => ({ ...prev, [eventId]: null }));
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Ar tikrai norite ištrinti šį įvykį?')) return;
    try {
      await api.delete(`/events/${id}`);
      setEvents(events.filter(e => e.idEvent !== id));
      // Taip pat pašaliname žalos įrašą iš būsenos
      setDamages(prev => {
        const newDamages = { ...prev };
        delete newDamages[id];
        return newDamages;
      });
    } catch (err) {
      alert('Klaida trinant įvykį.');
      console.error(err);
    }
  };

  if (loading) return <div>Kraunama...</div>;
  if (error) return <div style={{ color: 'red' }}>{error}</div>;

  return (
    <div>
      <h2>Įvykių sąrašas</h2>
      <Link to="/events/new">Sukurti naują įvykį</Link>
      <table border="1" cellPadding="8" style={{ marginTop: '20px', width: '100%' }}>
        <thead>
          <tr>
            <th>ID</th>
            <th>Tipas</th>
            <th>Data</th>
            <th>Aprašymas</th>
            <th>Vieta</th>
            <th>Statusas</th>
            <th>Žala (€)</th>
            <th>Veiksmai</th>
          </tr>
        </thead>
        <tbody>
          {events.map(event => (
            <tr key={event.idEvent}>
              <td>{event.idEvent}</td>
              <td>{event.eventType}</td>
              <td>{new Date(event.eventDate).toLocaleDateString('lt-LT')}</td>
              <td>{event.description}</td>
              <td>{event.location}</td>
              <td>{event.status}</td>
              <td>
                {damages[event.idEvent] !== undefined
                  ? damages[event.idEvent] !== null
                    ? `${damages[event.idEvent].toFixed(2)} €`
                    : 'Nepavyko apskaičiuoti'
                  : 'Skaičiuojama...'}
              </td>
              <td>
                <Link to={`/events/edit/${event.idEvent}`}>Redaguoti</Link>
                <button onClick={() => handleDelete(event.idEvent)} style={{ marginLeft: '8px' }}>
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

export default EventList;