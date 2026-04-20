import React from 'react';
import { BrowserRouter, Routes, Route, Link, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';
import LoginPage from './components/LoginPage';
import RegisterPage from './components/RegisterPage';
import EventManagement from './components/EventManagement';
import EventForm from './components/EventForm';
import CalculationReview from './components/CalculationReview';
import MaterialList from './components/MaterialList';
import MaterialForm from './components/MaterialForm';
import EventMap from './components/EventMap';
import ObjectList from './components/ObjectList';
import ObjectForm from './components/ObjectForm';
import ReportList from './components/ReportList';
import ReportForm from './components/ReportForm';
import IndexingCoefficientManager from './components/IndexingCoefficientManager';
import './App.css';

function NavBar() {
  const { isAuthenticated, isSpecialist, user, logout } = useAuth();

  return (
    <nav>
      <ul>
        {isAuthenticated ? (
          <>
            <li><Link to="/">Pradžia</Link></li>
            <li><Link to="/events">Įvykiai</Link></li>
            <li><Link to="/materials">Medžiagos</Link></li>
            <li><Link to="/objects">Objektai</Link></li>
            <li><Link to="/reports">Ataskaitos</Link></li>
            <li><Link to="/map">Žemėlapis</Link></li>
            {isSpecialist && <li><Link to="/indexing">I_n koeficientai</Link></li>}
            <li className="nav-user">
              <span className="nav-role">{isSpecialist ? '★ Specialistas' : 'Naudotojas'}</span>
              <span className="nav-email">{user.email}</span>
              <button className="nav-logout" onClick={logout}>Atsijungti</button>
            </li>
          </>
        ) : (
          <>
            <li><Link to="/login">Prisijungti</Link></li>
            <li><Link to="/register">Registruotis</Link></li>
          </>
        )}
      </ul>
    </nav>
  );
}

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <div>
          <NavBar />
          <div className="page-content"><div className="card">
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />

              <Route path="/" element={
                <ProtectedRoute>
                  <h1>Sveiki atvykę į ŽPVAIS</h1>
                </ProtectedRoute>
              } />

              <Route path="/events" element={<ProtectedRoute><EventManagement /></ProtectedRoute>} />
              <Route path="/events/new" element={<ProtectedRoute><EventForm /></ProtectedRoute>} />
              <Route path="/events/edit/:id" element={<ProtectedRoute requireSpecialist><EventForm /></ProtectedRoute>} />
              <Route path="/events/:id/calculation" element={<ProtectedRoute><CalculationReview /></ProtectedRoute>} />

              <Route path="/materials" element={<ProtectedRoute><MaterialList /></ProtectedRoute>} />
              <Route path="/materials/new" element={<ProtectedRoute><MaterialForm /></ProtectedRoute>} />
              <Route path="/materials/edit/:id" element={<ProtectedRoute requireSpecialist><MaterialForm /></ProtectedRoute>} />

              <Route path="/map" element={<ProtectedRoute><EventMap /></ProtectedRoute>} />

              <Route path="/objects" element={<ProtectedRoute><ObjectList /></ProtectedRoute>} />
              <Route path="/objects/new" element={<ProtectedRoute><ObjectForm /></ProtectedRoute>} />
              <Route path="/objects/edit/:id" element={<ProtectedRoute requireSpecialist><ObjectForm /></ProtectedRoute>} />

              <Route path="/reports" element={<ProtectedRoute><ReportList /></ProtectedRoute>} />
              <Route path="/reports/new" element={<ProtectedRoute requireSpecialist><ReportForm /></ProtectedRoute>} />
              <Route path="/reports/edit/:id" element={<ProtectedRoute requireSpecialist><ReportForm /></ProtectedRoute>} />

              <Route path="/indexing" element={<ProtectedRoute requireSpecialist><IndexingCoefficientManager /></ProtectedRoute>} />

              <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
          </div></div>
        </div>
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
