import { useState } from 'react';
import { BrowserRouter, NavLink, Navigate, Route, Routes } from 'react-router-dom';
import { ListaPage } from './pages/ListaPage';
import { FormPage } from './pages/FormPage';
import { LoginPage } from './pages/LoginPage';
import { AlterarSenhaPage } from './pages/AlterarSenhaPage';
import DashboardPage from './pages/DashboardPage';
import FiliaisPage from './pages/FiliaisPage';
import MensalidadesPage from './pages/MensalidadesPage';
import DespesasPage from './pages/DespesasPage';
import ConfiguracoesPage from './pages/ConfiguracoesPage';
import UsuariosPage from './pages/UsuariosPage';
import HistoricoPage from './pages/HistoricoPage';

export default function App() {
  const [usuario, setUsuario] = useState(() => {
    const token = localStorage.getItem('token');
    if (!token) return null;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      if (payload.exp * 1000 < Date.now()) { localStorage.removeItem('token'); return null; }
      return {
        nome:  payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'],
        email: payload.email,
        role:  payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'],
        deveAlterarSenha: false, // token existente = sessão já validada anteriormente
      };
    } catch { localStorage.removeItem('token'); return null; }
  });

  function handleLogin(dados) {
    setUsuario(dados);
  }

  function handleLogout() {
    localStorage.removeItem('token');
    setUsuario(null);
  }

  function handleSenhaAlterada() {
    setUsuario((prev) => ({ ...prev, deveAlterarSenha: false }));
  }

  if (!usuario) {
    return <LoginPage onLogin={handleLogin} />;
  }

  if (usuario.deveAlterarSenha) {
    return <AlterarSenhaPage usuario={usuario} onSenhaAlterada={handleSenhaAlterada} />;
  }

  const isAdmin = usuario.role === 'Admin';

  return (
    <BrowserRouter>
      <nav className="navbar">
        <span className="navbar-titulo">
          <span className="navbar-trinity">TRINITY</span> JIU-JITSU
        </span>
        <div className="navbar-links">
          <NavLink to="/dashboard"    className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>Dashboard</NavLink>
          <NavLink to="/atletas"      className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>Atletas</NavLink>
          <NavLink to="/mensalidades" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>Mensalidades</NavLink>
          {(isAdmin || usuario.role === 'GestorFilial') && (
            <NavLink to="/despesas" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>Despesas</NavLink>
          )}
          {isAdmin && (
            <>
              <NavLink to="/filiais"       className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>Filiais</NavLink>
              <NavLink to="/usuarios"      className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>Usuários</NavLink>
              <NavLink to="/configuracoes" className={({ isActive }) => isActive ? 'nav-link active' : 'nav-link'}>Configurações</NavLink>
            </>
          )}
        </div>
        <div className="navbar-usuario">
          <span>{usuario.nome}</span>
          <button className="btn-sair" onClick={handleLogout}>Sair</button>
        </div>
      </nav>
      <main>
        <Routes>
          <Route path="/"                       element={<Navigate to="/dashboard" replace />} />
          <Route path="/dashboard"              element={<DashboardPage usuario={usuario} />} />
          <Route path="/atletas"                element={<ListaPage />} />
          <Route path="/atletas/novo"           element={<FormPage usuario={usuario} />} />
          <Route path="/atletas/:id/editar"     element={<FormPage usuario={usuario} />} />
          <Route path="/atletas/:id/historico"  element={<HistoricoPage />} />
          <Route path="/mensalidades"           element={<MensalidadesPage usuario={usuario} />} />
          <Route path="/despesas"               element={<DespesasPage usuario={usuario} />} />
          <Route path="/filiais"                element={isAdmin ? <FiliaisPage /> : <Navigate to="/dashboard" replace />} />
          <Route path="/usuarios"               element={isAdmin ? <UsuariosPage /> : <Navigate to="/dashboard" replace />} />
          <Route path="/configuracoes"          element={isAdmin ? <ConfiguracoesPage usuario={usuario} /> : <Navigate to="/dashboard" replace />} />
        </Routes>
      </main>
    </BrowserRouter>
  );
}
