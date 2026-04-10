import React, { useState } from 'react';
import { MapContainer, TileLayer, FeatureGroup } from 'react-leaflet';
import { EditControl } from 'react-leaflet-draw';
import 'leaflet/dist/leaflet.css';
import 'leaflet-draw/dist/leaflet.draw.css';
import L from 'leaflet';

// Pataisome Leaflet ikonėlių kelią
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

const PolygonPicker = ({ onPolygonChange }) => {
  const [polygon, setPolygon] = useState(null);

  const handleCreated = (e) => {
    const layer = e.layer;
    const geoJson = layer.toGeoJSON();
    setPolygon(geoJson.geometry);
    onPolygonChange(geoJson.geometry);
  };

  const handleEdited = (e) => {
    const layers = e.layers;
    layers.eachLayer(layer => {
      const geoJson = layer.toGeoJSON();
      setPolygon(geoJson.geometry);
      onPolygonChange(geoJson.geometry);
    });
  };

  const handleDeleted = (e) => {
    setPolygon(null);
    onPolygonChange(null);
  };

  return (
    <div style={{  display: 'flex', justifyContent: 'center' ,height: '500px', maxWidth: '500px', marginBottom: '20px' }}>
      <MapContainer center={[55.0, 24.0]} zoom={7} style={{ height: '100%', width: '100%' }}>
        <TileLayer
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
        />
        <FeatureGroup>
          <EditControl
            position="topright"
            onCreated={handleCreated}
            onEdited={handleEdited}
            onDeleted={handleDeleted}
            draw={{
              rectangle: true,
              polygon: true,
              circle: false,
              marker: false,
              polyline: false,
              circlemarker: false,
            }}
          />
        </FeatureGroup>
      </MapContainer>
    </div>
  );
};

export default PolygonPicker;