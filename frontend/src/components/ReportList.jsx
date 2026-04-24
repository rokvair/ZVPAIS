import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { MapContainer, TileLayer, GeoJSON, useMap } from 'react-leaflet';
import L from 'leaflet';
import leafletImage from 'leaflet-image';
import 'leaflet/dist/leaflet.css';
import api from '../services/api';
import { useAuth } from '../context/AuthContext';

delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
});

const eventTypeLabel = {
  gaisras: 'Gaisras',
  'medžiagų išsiliejimas': 'Medžiagų išsiliejimas',
  stichija: 'Stichija'
};

const DONE_STATUSES = ['laukia peržiūros', 'tikrinamas', 'patvirtintas', 'atmestas'];

function FitAndCapture({ polygon, onCapture }) {
  const map = useMap();
  useEffect(() => {
    if (!polygon) { onCapture(map, null); return; }
    try {
      const layer = L.geoJSON(JSON.parse(polygon));
      map.fitBounds(layer.getBounds(), { padding: [20, 20] });
    } catch { }
    // give tiles a moment to render before capturing
    const timer = setTimeout(() => onCapture(map, polygon), 2500);
    return () => clearTimeout(timer);
  }, [polygon, onCapture]);
  return null;
}

const ReportList = () => {
  const { isSpecialist } = useAuth();
  const [reports, setReports] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [downloadingId, setDownloadingId] = useState(null);
  const [pdfJob, setPdfJob] = useState(null); // { eventId, polygon }

  useEffect(() => {
    fetchReports();
  }, []);

  const fetchReports = async () => {
    try {
      const res = await api.get('/reports');
      setReports(res.data);
    } catch (err) {
      setError('Nepavyko gauti ataskaitų.');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Ar tikrai norite ištrinti šią ataskaitą?')) return;
    try {
      await api.delete(`/reports/${id}`);
      setReports(reports.filter(r => r.idDamageEvaluation !== id));
    } catch (err) {
      alert('Klaida trinant ataskaitą.');
      console.error(err);
    }
  };

  const handleDownloadPdf = async (report) => {
    setDownloadingId(report.idDamageEvaluation);
    try {
      const eventRes = await api.get(`/events/${report.eventId}`);
      const polygon = eventRes.data.polygon || null;
      setPdfJob({ eventId: report.eventId, polygon });
    } catch {
      alert('Klaida generuojant PDF.');
      setDownloadingId(null);
    }
  };

  const handleMapReady = async (map, polygon) => {
    const captureAndDownload = async (base64) => {
      try {
        const res = await api.post(
          `/reports/event/${pdfJob.eventId}/pdf`,
          { mapImageBase64: base64 },
          { responseType: 'blob' }
        );
        const url = window.URL.createObjectURL(new Blob([res.data], { type: 'application/pdf' }));
        const a = document.createElement('a');
        a.href = url;
        a.download = `ataskaita-ivykis-${pdfJob.eventId}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      } catch {
        alert('Klaida generuojant PDF.');
      } finally {
        setPdfJob(null);
        setDownloadingId(null);
      }
    };

    if (polygon && map) {
      leafletImage(map, (err, canvas) => {
        if (err) console.error('leaflet-image capture error:', err);
        const base64 = (!err && canvas) ? canvas.toDataURL('image/png') : null;
        captureAndDownload(base64);
      });
    } else {
      await captureAndDownload(null);
    }
  };

  if (loading) return <div>Kraunama...</div>;
  if (error) return <div style={{ color: 'red' }}>{error}</div>;

  return (
    <div>
      <h2>Žalos vertinimo ataskaitos</h2>
      {isSpecialist && <Link to="/reports/new">Sukurti naują ataskaitą</Link>}
      {reports.length === 0 ? (
        <p style={{ marginTop: '16px' }}>Nėra ataskaitų.</p>
      ) : (
        <table border="1" cellPadding="8" style={{ marginTop: '20px', width: '100%', borderCollapse: 'collapse' }}>
          <thead>
            <tr>
              <th>ID</th>
              <th>Įvykio tipas</th>
              <th>Įvykio data</th>
              <th>Vieta</th>
              <th>Statusas</th>
              <th>Vertinimo data</th>
              <th>Žalos dydis (EUR)</th>
              <th>Piniginis dydis (EUR)</th>
              <th>Pastabos</th>
              <th>Veiksmai</th>
            </tr>
          </thead>
          <tbody>
            {reports.map(r => {
              const calcsDone = DONE_STATUSES.includes(r.eventStatus);
              const isDownloading = downloadingId === r.idDamageEvaluation;
              return (
                <tr key={r.idDamageEvaluation}>
                  <td>{r.idDamageEvaluation}</td>
                  <td>{eventTypeLabel[r.eventType] ?? r.eventType}</td>
                  <td>{r.eventDate ? new Date(r.eventDate).toLocaleDateString('lt-LT') : '—'}</td>
                  <td>{r.eventLocation || '—'}</td>
                  <td>{r.eventStatus || '—'}</td>
                  <td>{new Date(r.data).toLocaleDateString('lt-LT')}</td>
                  <td>{r.zalosDydis != null ? r.zalosDydis.toFixed(2) : '—'}</td>
                  <td>{r.piniginisDydis != null ? r.piniginisDydis.toFixed(2) : '—'}</td>
                  <td style={{ maxWidth: '200px', whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>{r.notes || '—'}</td>
                  <td>
                    <Link to={`/events/${r.eventId}/calculation`}>Skaičiavimas</Link>
                    {calcsDone && (
                      <>
                        {' | '}
                        <button
                          onClick={() => handleDownloadPdf(r)}
                          disabled={!!downloadingId}
                          style={{ marginLeft: '2px' }}
                        >
                          {isDownloading ? 'Generuojama...' : 'PDF'}
                        </button>
                      </>
                    )}
                    {isSpecialist && (
                      <>
                        {' | '}
                        <Link to={`/reports/edit/${r.idDamageEvaluation}`}>Redaguoti</Link>
                        {' '}
                        <button onClick={() => handleDelete(r.idDamageEvaluation)} style={{ marginLeft: '4px' }}>
                          Trinti
                        </button>
                      </>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
      )}

      {/* Hidden map used only during PDF capture */}
      {pdfJob && (
        <div style={{ position: 'fixed', left: 0, top: 0, width: '700px', height: '450px', opacity: 0, pointerEvents: 'none', zIndex: -1 }}>
          <MapContainer
            center={[55.0, 24.0]}
            zoom={7}
            style={{ width: '700px', height: '450px' }}
            zoomControl={false}
            attributionControl={false}
            preferCanvas={true}
          >
            <TileLayer
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
              crossOrigin="anonymous"
            />
            {pdfJob.polygon && (() => {
              try {
                return (
                  <GeoJSON
                    data={JSON.parse(pdfJob.polygon)}
                    style={{ fillColor: '#3388ff', weight: 2, color: '#1a5fa8', fillOpacity: 0.4 }}
                  />
                );
              } catch { return null; }
            })()}
            <FitAndCapture polygon={pdfJob.polygon} onCapture={handleMapReady} />
          </MapContainer>
        </div>
      )}
    </div>
  );
};

export default ReportList;
