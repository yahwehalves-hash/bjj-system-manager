import { useState } from 'react';
import { authApi } from '../api/authApi';

export function AlterarSenhaPage({ usuario, onSenhaAlterada }) {
  const [form, setForm] = useState({ senhaAtual: '', novaSenha: '', confirmar: '' });
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(false);

  function handleChange(e) {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setErro('');

    if (form.novaSenha !== form.confirmar) {
      setErro('As senhas não coincidem.');
      return;
    }
    if (form.novaSenha.length < 6) {
      setErro('A nova senha deve ter no mínimo 6 caracteres.');
      return;
    }

    setCarregando(true);
    try {
      await authApi.alterarSenha(form.senhaAtual, form.novaSenha);
      onSenhaAlterada();
    } catch (err) {
      setErro(err.response?.data?.erro || 'Erro ao alterar senha. Tente novamente.');
    } finally {
      setCarregando(false);
    }
  }

  return (
    <div className="login-container">
      <div className="login-card">
        <div className="login-logo">
          <svg viewBox="0 0 100 100" className="login-triangulo" xmlns="http://www.w3.org/2000/svg">
            <circle cx="50" cy="50" r="48" fill="none" stroke="#fff" strokeWidth="4" />
            <polygon points="50,12 88,78 12,78" fill="none" stroke="#fff" strokeWidth="6" />
            <polygon points="50,28 76,72 24,72" fill="#1a1a1a" stroke="#1a1a1a" strokeWidth="1" />
            <rect x="32" y="68" width="36" height="8" fill="#cc0000" />
          </svg>
        </div>

        <h1 className="login-titulo">TRINITY</h1>
        <p className="login-subtitulo">JIU-JITSU</p>
        <p className="login-descricao" style={{ textAlign: 'center', color: '#cc0000' }}>
          Olá, {usuario.nome}! Por segurança, defina uma nova senha antes de continuar.
        </p>

        {erro && <div className="login-erro">{erro}</div>}

        <form className="login-form" onSubmit={handleSubmit}>
          <input
            className="login-input"
            name="senhaAtual"
            type="password"
            placeholder="Senha atual"
            value={form.senhaAtual}
            onChange={handleChange}
            required
          />
          <input
            className="login-input"
            name="novaSenha"
            type="password"
            placeholder="Nova senha (mínimo 6 caracteres)"
            value={form.novaSenha}
            onChange={handleChange}
            required
          />
          <input
            className="login-input"
            name="confirmar"
            type="password"
            placeholder="Confirmar nova senha"
            value={form.confirmar}
            onChange={handleChange}
            required
          />
          <button className="btn-login" type="submit" disabled={carregando}>
            {carregando ? 'Salvando...' : 'Definir nova senha'}
          </button>
        </form>

        <p className="login-rodape">Trinity Jiu-Jitsu © {new Date().getFullYear()}</p>
      </div>
    </div>
  );
}
