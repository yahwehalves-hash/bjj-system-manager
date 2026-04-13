import { useEffect, useState } from 'react';
import { usuariosApi } from '../api/authApi';

const ROLES = ['Aluno', 'Professor', 'GestorFilial', 'Admin'];

const ROLE_LABEL = {
  Aluno:       'Aluno',
  Professor:   'Professor',
  GestorFilial:'Gestor de Filial',
  Admin:       'Administrador',
};

export default function UsuariosPage() {
  const [usuarios, setUsuarios]   = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [alerta, setAlerta]       = useState({ tipo: '', msg: '' });
  const [salvando, setSalvando]   = useState(null); // id do usuario sendo salvo

  useEffect(() => { carregar(); }, []);

  async function carregar() {
    try {
      const dados = await usuariosApi.listar();
      setUsuarios(dados);
    } catch {
      mostrarAlerta('erro', 'Erro ao carregar usuários.');
    } finally {
      setCarregando(false);
    }
  }

  function mostrarAlerta(tipo, msg) {
    setAlerta({ tipo, msg });
    setTimeout(() => setAlerta({ tipo: '', msg: '' }), 3000);
  }

  async function handleRoleChange(id, novaRole) {
    setSalvando(id);
    try {
      await usuariosApi.alterarRole(id, novaRole);
      setUsuarios((prev) =>
        prev.map((u) => (u.id === id ? { ...u, role: novaRole } : u))
      );
      mostrarAlerta('info', 'Perfil atualizado com sucesso.');
    } catch (err) {
      mostrarAlerta('erro', err.response?.data?.erro || 'Erro ao alterar perfil.');
    } finally {
      setSalvando(null);
    }
  }

  return (
    <div className="pagina">
      <div className="cabecalho">
        <h1>Usuários</h1>
      </div>

      {alerta.msg && (
        <div className={alerta.tipo === 'erro' ? 'alerta-erro' : 'alerta-info'}>
          {alerta.msg}
        </div>
      )}

      {carregando ? (
        <p className="carregando">Carregando...</p>
      ) : (
        <table className="tabela">
          <thead>
            <tr>
              <th>Nome</th>
              <th>Email</th>
              <th>Perfil</th>
              <th>Cadastro</th>
              <th>Status</th>
            </tr>
          </thead>
          <tbody>
            {usuarios.map((u) => (
              <tr key={u.id}>
                <td>{u.nome}</td>
                <td>{u.email}</td>
                <td>
                  <select
                    className="filtros"
                    style={{ padding: '5px 8px', fontSize: '0.88rem' }}
                    value={u.role}
                    disabled={salvando === u.id}
                    onChange={(e) => handleRoleChange(u.id, e.target.value)}
                  >
                    {ROLES.map((r) => (
                      <option key={r} value={r}>{ROLE_LABEL[r]}</option>
                    ))}
                  </select>
                </td>
                <td>{new Date(u.criadoEm).toLocaleDateString('pt-BR')}</td>
                <td>
                  {u.deveAlterarSenha && (
                    <span style={{ color: '#c05621', fontSize: '0.8rem', fontWeight: 600 }}>
                      Senha provisória
                    </span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
