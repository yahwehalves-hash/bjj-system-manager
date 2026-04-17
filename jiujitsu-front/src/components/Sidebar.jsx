import { useState } from 'react';
import { NavLink } from 'react-router-dom';
import {
  LayoutDashboard, Users, Swords, CreditCard,
  Receipt, FileText, ClipboardList, Building2, UserCog,
  Settings, Bell, ChevronLeft, ChevronRight, LogOut,
} from 'lucide-react';

function FaixaIcon({ size = 18 }) {
  return (
    <svg width={size} height={size} viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
      {/* corpo da faixa */}
      <rect x="2" y="9" width="20" height="6" rx="1.5" />
      {/* ponta esquerda */}
      <path d="M2 10.5 L5 12 L2 13.5" fill="currentColor" stroke="none" />
      {/* ponta direita */}
      <path d="M22 10.5 L19 12 L22 13.5" fill="currentColor" stroke="none" />
      {/* nó central */}
      <rect x="9" y="9" width="6" height="6" rx="1" fill="currentColor" stroke="none" opacity="0.4" />
    </svg>
  );
}

function SidebarLink({ to, icon: Icon, label, collapsed }) {
  return (
    <NavLink
      to={to}
      title={label}
      className={({ isActive }) => `sidebar-link${isActive ? ' active' : ''}`}
    >
      <Icon size={18} strokeWidth={1.8} />
      {!collapsed && <span>{label}</span>}
    </NavLink>
  );
}

function GroupLabel({ label, collapsed }) {
  if (collapsed) return <div className="sidebar-divider" />;
  return <span className="sidebar-group-label">{label}</span>;
}

export default function Sidebar({ usuario, onLogout }) {
  const [collapsed, setCollapsed] = useState(false);
  const isAdmin = usuario.role === 'Admin';
  const isGestor = isAdmin || usuario.role === 'GestorFilial';

  return (
    <aside className={`sidebar${collapsed ? ' collapsed' : ''}`}>
      <div className="sidebar-header">
        {!collapsed && (
          <span className="sidebar-titulo">
            <span className="sidebar-trinity">TRINITY</span> SOFTWARE
          </span>
        )}
        <button className="sidebar-toggle" onClick={() => setCollapsed(!collapsed)} title={collapsed ? 'Expandir' : 'Recolher'}>
          {collapsed ? <ChevronRight size={16} /> : <ChevronLeft size={16} />}
        </button>
      </div>

      <nav className="sidebar-nav">
        <SidebarLink to="/dashboard"    icon={LayoutDashboard} label="Dashboard"    collapsed={collapsed} />
        <SidebarLink to="/atletas"      icon={Users}           label="Atletas"      collapsed={collapsed} />
        <SidebarLink to="/turmas"       icon={Swords}          label="Turmas"       collapsed={collapsed} />
        <SidebarLink to="/graduacao"    icon={FaixaIcon}       label="Graduação"    collapsed={collapsed} />
        <SidebarLink to="/mensalidades" icon={CreditCard}      label="Mensalidades" collapsed={collapsed} />

        {isGestor && (
          <>
            <GroupLabel label="Gestão" collapsed={collapsed} />
            <SidebarLink to="/despesas"  icon={Receipt}      label="Despesas"  collapsed={collapsed} />
            <SidebarLink to="/planos"    icon={FileText}     label="Planos"    collapsed={collapsed} />
            <SidebarLink to="/contratos" icon={ClipboardList} label="Contratos" collapsed={collapsed} />
          </>
        )}

        {isAdmin && (
          <>
            <GroupLabel label="Administração" collapsed={collapsed} />
            <SidebarLink to="/filiais"       icon={Building2} label="Filiais"       collapsed={collapsed} />
            <SidebarLink to="/usuarios"      icon={UserCog}   label="Usuários"      collapsed={collapsed} />
            <SidebarLink to="/configuracoes" icon={Settings}  label="Configurações" collapsed={collapsed} />
            <SidebarLink to="/notificacoes"  icon={Bell}      label="Notificações"  collapsed={collapsed} />
          </>
        )}
      </nav>

      <div className="sidebar-footer">
        {!collapsed && <span className="sidebar-usuario-nome">{usuario.nome}</span>}
        <button className="sidebar-logout" onClick={onLogout} title="Sair">
          <LogOut size={16} strokeWidth={1.8} />
          {!collapsed && <span>Sair</span>}
        </button>
      </div>
    </aside>
  );
}
