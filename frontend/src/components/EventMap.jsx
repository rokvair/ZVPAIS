import React, { useEffect, useState } from 'react';
import { MapContainer, TileLayer, GeoJSON, Popup } from 'react-leaflet';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import api from '../services/api';

// Pataisome Leaflet ikonėlių kelią
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

// Funkcija, skirta gauti spalvą pagal įvykio tipą
const getColorByType = (type) => {
  switch (type) {
    case 'gaisras':
      return '#ff4444'; // raudona
    case 'medžiagų išsiliejimas':
      return '#ffaa00'; // oranžinė
    case 'stichija':
      return '#44aa44'; // žalia
    default:
      return '#3388ff'; // mėlyna
  }
};

// Funkcija, skirta nustatyti stilių kiekvienam poligonui
const polygonStyle = (feature) => {
  const type = feature.properties?.eventType || 'unknown';
  return {
    fillColor: getColorByType(type),
    weight: 2,
    opacity: 1,
    color: 'white',
    fillOpacity: 0.5
  };
};

const EventMap = () => {
  const [events, setEvents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchEvents = async () => {
      try {
        const response = await api.get('/events');
        setEvents(response.data);
      } catch (err) {
        setError('Nepavyko gauti įvykių sąrašo.');
        console.error(err);
      } finally {
        setLoading(false);
      }
    };
    fetchEvents();
  }, []);

  if (loading) return <div>Kraunamas žemėlapis...</div>;
  if (error) return <div style={{ color: 'red' }}>{error}</div>;

  // Konvertuojame įvykius į GeoJSON FeatureCollection
  const approved = events.filter(e => e.status === 'patvirtintas' && e.polygon);

  const geoJsonData = {
    type: 'FeatureCollection',
    features: approved.map(event => {
      try {
        return {
          type: 'Feature',
          geometry: JSON.parse(event.polygon),
          properties: {
            id: event.idEvent,
            eventType: event.eventType,
            eventDate: new Date(event.eventDate).toLocaleDateString('lt-LT'),
            location: event.location,
          }
        };
      } catch { return null; }
    }).filter(Boolean)
  };

  // Funkcija, kuri sukuria popup'ą kiekvienam elementui
  const onEachFeature = (feature, layer) => {
    if (feature.properties) {
      const { eventType, eventDate, location, id } = feature.properties;
      layer.bindPopup(`
        <div style="min-width:160px">
          <strong>${eventType}</strong><br/>
          ${eventDate}<br/>
          ${location ? `${location}<br/>` : ''}
          <a href="/events/${id}/calculation" style="font-size:0.9em">Žiūrėti skaičiavimą →</a>
        </div>
      `);
    }
  };

  return (
    <div>
      <h2>Patvirtintų įvykių žemėlapis</h2>
      <p style={{ color: '#666', marginTop: 0 }}>Rodomi tik patvirtinti ({approved.length}) įvykiai.</p>
      <MapContainer
        center={[55.0, 24.0]}
        zoom={7}
        style={{ height: '600px', width: '100%' }}
      >
        <TileLayer
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        />
        <GeoJSON
          data={geoJsonData}
          style={polygonStyle}
          onEachFeature={onEachFeature}
        />
      </MapContainer>
    </div>
  );
};

export default EventMap;