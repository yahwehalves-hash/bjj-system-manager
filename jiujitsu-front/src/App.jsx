import { useState } from 'react';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { ListaPage } from './pages/ListaPage';
import { FormPage } from './pages/FormPage';
import { LoginPage } from './pages/LoginPage';

export default function App() {
  const [usuario, setUsuario] = useState(() => {
    const token = localStorage.getItem('token');
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      if (payload.exp * 1000 < Date.now()) { localStorage.removeItem('token'); return null; }
      return { nome: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'], email: payload.email, role: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] };
    } catch { localStorage.removeItem('token'); return null; }
  });

  function handleLogout() {
    localStorage.removeItem('token');
    setUsuario(null);
  }

  if (!usuario) {
    return <LoginPage onLogin={(dados) => setUsuario(dados)} />;
  }

  return (
    <BrowserRouter>
      <nav className="navbar">
        <span className="navbar-titulo">
          <span className="navbar-trinity">TRINITY</span> JIU-JITSU
        </span>
        <div className="navbar-usuario">
          <span>{usuario.nome}</span>
          <button className="btn-sair" onClick={handleLogout}>Sair</button>
        </div>
      </nav>
      <main>
        <Routes>
          <Route path="/"                   element={<ListaPage />} />
          <Route path="/atletas/novo"       element={<FormPage />} />
          <Route path="/atletas/:id/editar" element={<FormPage />} />
        </Routes>
      </main>
    </BrowserRouter>
  );
}
