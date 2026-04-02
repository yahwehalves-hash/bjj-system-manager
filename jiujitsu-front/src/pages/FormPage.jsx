import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { atletasApi } from '../api/atletasApi';

const FAIXAS = [
  { valor: 1, label: 'Branca' },
  { valor: 2, label: 'Cinza' },
  { valor: 3, label: 'Azul' },
  { valor: 4, label: 'Roxa' },
  { valor: 5, label: 'Marrom' },
  { valor: 6, label: 'Preta' },
];

const GRAUS = [
  { valor: 0, label: 'Sem grau' },
  { valor: 1, label: '1° grau' },
  { valor: 2, label: '2° grau' },
  { valor: 3, label: '3° grau' },
  { valor: 4, label: '4° grau' },
];

const FORM_INICIAL = {
  nomeCompleto: '',
  cpf: '',
  dataNascimento: '',
  faixa: 1,
  grau: 0,
  dataUltimaGraduacao: '',
  email: '',
};

export function FormPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const edicao = Boolean(id);

  const [form, setForm] = useState(FORM_INICIAL);
  const [carregando, setCarregando] = useState(false);
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState('');

  // Em edição, carrega os dados atuais do atleta
  useEffect(() => {
    if (!edicao) return;

    setCarregando(true);
    atletasApi
      .obterPorId(id)
      .then((atleta) => {
        setForm({
          nomeCompleto:        atleta.nomeCompleto,
          cpf:                 atleta.cpf,
          dataNascimento:      atleta.dataNascimento,
          faixa:               FAIXAS.find((f) => f.label === atleta.faixa)?.valor ?? 1,
          grau:                atleta.grau,
          dataUltimaGraduacao: atleta.dataUltimaGraduacao,
          email:               atleta.email,
        });
      })
      .catch(() => setErro('Erro ao carregar dados do atleta.'))
      .finally(() => setCarregando(false));
  }, [id]);

  function handleChange(e) {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setSalvando(true);
    setErro('');

    // Monta o payload no formato esperado pela API
    const payload = {
      nomeCompleto:        form.nomeCompleto,
      cpf:                 form.cpf.replace(/\D/g, ''), // remove formatação
      dataNascimento:      form.dataNascimento,
      faixa:               Number(form.faixa),
      grau:                Number(form.grau),
      dataUltimaGraduacao: form.dataUltimaGraduacao,
      email:               form.email,
    };

    try {
      if (edicao) {
        await atletasApi.atualizar(id, payload);
      } else {
        await atletasApi.criar(payload);
      }
      // Retorna para a lista — o Worker processará em background
      navigate('/', {
        state: {
          aviso: edicao
            ? 'Atualização enviada para processamento.'
            : 'Atleta cadastrado! Será exibido em instantes.',
        },
      });
    } catch (err) {
      const msg = err.response?.data?.erro || err.response?.data?.title || 'Erro ao salvar. Verifique os dados e tente novamente.';
      setErro(msg);
    } finally {
      setSalvando(false);
    }
  }

  if (carregando) return <p className="carregando">Carregando...</p>;

  return (
    <div className="pagina">
      <div className="cabecalho">
        <h1>{edicao ? 'Editar Atleta' : 'Novo Atleta'}</h1>
      </div>

      {erro && <div className="alerta-erro">{erro}</div>}

      <form className="formulario" onSubmit={handleSubmit}>
        <div className="campo">
          <label>Nome Completo *</label>
          <input
            name="nomeCompleto"
            value={form.nomeCompleto}
            onChange={handleChange}
            required
            maxLength={200}
            placeholder="Nome completo do atleta"
          />
        </div>

        <div className="campo">
          <label>CPF *</label>
          <input
            name="cpf"
            value={form.cpf}
            onChange={handleChange}
            required
            maxLength={14}
            placeholder="000.000.000-00"
            disabled={edicao} // CPF não pode ser alterado após cadastro
          />
        </div>

        <div className="campo">
          <label>Data de Nascimento *</label>
          <input
            type="date"
            name="dataNascimento"
            value={form.dataNascimento}
            onChange={handleChange}
            required
          />
        </div>

        <div className="linha-dupla">
          <div className="campo">
            <label>Faixa *</label>
            <select name="faixa" value={form.faixa} onChange={handleChange} required>
              {FAIXAS.map((f) => (
                <option key={f.valor} value={f.valor}>{f.label}</option>
              ))}
            </select>
          </div>

          <div className="campo">
            <label>Grau</label>
            <select name="grau" value={form.grau} onChange={handleChange}>
              {GRAUS.map((g) => (
                <option key={g.valor} value={g.valor}>{g.label}</option>
              ))}
            </select>
          </div>
        </div>

        <div className="campo">
          <label>Data da Última Graduação *</label>
          <input
            type="date"
            name="dataUltimaGraduacao"
            value={form.dataUltimaGraduacao}
            onChange={handleChange}
            required
          />
        </div>

        <div className="campo">
          <label>Email *</label>
          <input
            type="email"
            name="email"
            value={form.email}
            onChange={handleChange}
            required
            placeholder="atleta@email.com"
          />
        </div>

        <div className="acoes-form">
          <button
            type="button"
            className="btn-secundario"
            onClick={() => navigate('/')}
            disabled={salvando}
          >
            Cancelar
          </button>
          <button type="submit" className="btn-primario" disabled={salvando}>
            {salvando ? 'Salvando...' : edicao ? 'Salvar Alterações' : 'Cadastrar'}
          </button>
        </div>
      </form>
    </div>
  );
}
