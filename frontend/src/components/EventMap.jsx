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
    case 'gaisas':
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
  const geoJsonData = {
    type: 'FeatureCollection',
    features: events.map(event => ({
      type: 'Feature',
      geometry: JSON.parse(event.polygon), // event.polygon yra GeoJSON string
      properties: {
        id: event.idEvent,
        eventType: event.eventType,
        eventDate: new Date(event.eventDate).toLocaleString('lt-LT'),
        description: event.description,
        location: event.location,
        status: event.status
      }
    }))
  };

  // Funkcija, kuri sukuria popup'ą kiekvienam elementui
  const onEachFeature = (feature, layer) => {
    if (feature.properties) {
      const props = feature.properties;
      const popupContent = `
        <div>
          <strong>Tipas:</strong> ${props.eventType}<br/>
          <strong>Data:</strong> ${props.eventDate}<br/>
          <strong>Vieta:</strong> ${props.location || 'Nenurodyta'}<br/>
          <strong>Statusas:</strong> ${props.status}<br/>
          ${props.description ? `<strong>Aprašymas:</strong> ${props.description}` : ''}
        </div>
      `;
      layer.bindPopup(popupContent);
    }
  };

  return (
    <div>
      <h2>Įvykių žemėlapis</h2>
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