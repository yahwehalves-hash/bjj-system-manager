import { useState } from 'react';
import { authApi } from '../api/authApi';

export function LoginPage({ onLogin }) {
  const [modo, setModo] = useState('login'); // 'login' | 'registrar'
  const [form, setForm] = useState({ nome: '', email: '', senha: '' });
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(false);

  function handleChange(e) {
    setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setErro('');
    setCarregando(true);
    try {
      if (modo === 'login') {
        const dados = await authApi.login(form.email, form.senha);
        localStorage.setItem('token', dados.token);
        let filialId = null;
        try {
          const payload = JSON.parse(atob(dados.token.split('.')[1]));
          filialId = payload.filialId ?? null;
        } catch { /* ignora */ }
        onLogin({ nome: dados.nome, email: dados.email, role: dados.role, filialId, deveAlterarSenha: dados.deveAlterarSenha });
      } else {
        await authApi.registrar(form.nome, form.email, form.senha);
        setModo('login');
        setErro('');
        setForm((prev) => ({ ...prev, nome: '', senha: '' }));
      }
    } catch (err) {
      setErro(err.response?.data?.erro || 'Erro ao processar. Tente novamente.');
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
        <p className="login-descricao">Sistema de Gestão</p>

        {erro && <div className="login-erro">{erro}</div>}

        <form className="login-form" onSubmit={handleSubmit}>
          {modo === 'registrar' && (
            <>
              <input
                className="login-input"
                name="nome"
                placeholder="Nome completo"
                value={form.nome}
                onChange={handleChange}
                required
              />
            </>
          )}
          <input
            className="login-input"
            name="email"
            type="email"
            placeholder="Email"
            value={form.email}
            onChange={handleChange}
            required
          />
          <input
            className="login-input"
            name="senha"
            type="password"
            placeholder="Senha"
            value={form.senha}
            onChange={handleChange}
            required
          />
          <button className="btn-login" type="submit" disabled={carregando}>
            {carregando ? 'Aguarde...' : modo === 'login' ? 'Entrar' : 'Criar Conta'}
          </button>
        </form>

        <button className="login-troca-modo" onClick={() => { setModo(modo === 'login' ? 'registrar' : 'login'); setErro(''); }}>
          {modo === 'login' ? 'Criar uma conta' : 'Já tenho uma conta'}
        </button>

        <p className="login-rodape">Trinity Jiu-Jitsu © {new Date().getFullYear()}</p>
      </div>
    </div>
  );
}
