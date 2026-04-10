import React from 'react';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import EventList from './components/EventList';
import EventForm from './components/EventForm';
import MaterialList from './components/MaterialList';
import MaterialForm from './components/MaterialForm';
import EventMap from './components/EventMap';
import ObjectList from './components/ObjectList';
import ObjectForm from './components/ObjectForm';
import './App.css';

function App() {
  return (
    <BrowserRouter>
      <div>
        <nav>
          <ul>
            <li><Link to="/">Pradžia</Link></li>
            <li><Link to="/events">Įvykiai</Link></li>
            <li><Link to="/materials">Medžiagos</Link></li>
            <li><Link to="/map">Žemėlapis</Link></li> {/* nauja nuoroda */}
            <li><Link to="/objects">Objektai</Link></li>
          </ul>
        </nav>

        <Routes>
          <Route path="/" element={<h1>Sveiki atvykę į ŽPVAIS</h1>} />
          <Route path="/events" element={<EventList />} />
          <Route path="/events/new" element={<EventForm />} />
          <Route path="/events/edit/:id" element={<EventForm />} />
          <Route path="/materials" element={<MaterialList />} />
          <Route path="/materials/new" element={<MaterialForm />} />
          <Route path="/materials/edit/:id" element={<MaterialForm />} />
          <Route path="/map" element={<EventMap />} /> {/* naujas maršrutas */}
          <Route path="/objects" element={<ObjectList />} />
          <Route path="/objects/new" element={<ObjectForm />} />
          <Route path="/objects/edit/:id" element={<ObjectForm />} />
        </Routes>
      </div>
    </BrowserRouter>
  );
}

export default App;