import { useEffect, useState } from 'react';
import { atletasApi } from '../api/atletasApi';
import { contratosApi } from '../api/contratosApi';

const TEMPLATE_PADRAO = `CONTRATO DE PRESTAÇÃO DE SERVIÇOS

Pelo presente instrumento, a Academia {NomeAcademia} e o(a) aluno(a) {NomeAtleta},
portador(a) do CPF {Cpf}, celebram o presente contrato de prestação de serviços de
treinamento em Jiu-Jitsu.

CLÁUSULA 1 — OBJETO
A academia compromete-se a oferecer aulas de Jiu-Jitsu conforme grade de horários vigente.

CLÁUSULA 2 — VIGÊNCIA
O presente contrato é celebrado por prazo indeterminado, podendo ser rescindido por
qualquer das partes com aviso prévio de 30 (trinta) dias.

CLÁUSULA 3 — PAGAMENTO
O valor mensal será definido conforme plano contratado, com vencimento no dia 5 de cada mês.

CLÁUSULA 4 — RESPONSABILIDADE
O aluno declara estar ciente dos riscos inerentes à prática de artes marciais e assume
responsabilidade por seu estado de saúde, apresentando atestado médico quando solicitado.

Ao aceitar digitalmente este contrato, o(a) aluno(a) confirma ter lido e concordado com
todos os termos acima.`;

export function ContratosPage() {
  const [tab, setTab] = useState('atletas');
  const [atletas, setAtletas] = useState([]);
  const [statusMap, setStatusMap] = useState({});
  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState('');
  const [aviso, setAviso] = useState('');

  // Template editor
  const [template, setTemplate] = useState(TEMPLATE_PADRAO);
  const [salvandoTemplate, setSalvandoTemplate] = useState(false);

  useEffect(() => {
    if (tab === 'atletas') carregarAtletas();
  }, [tab]);

  async function carregarAtletas() {
    setCarregando(true);
    setErro('');
    try {
      const resultado = await atletasApi.listar({ pagina: 1, tamanhoPagina: 100 });
      setAtletas(resultado.itens);
      await carregarStatus(resultado.itens);
    } catch {
      setErro('Erro ao carregar atletas.');
    } finally {
      setCarregando(false);
    }
  }

  async function carregarStatus(lista) {
    const entradas = await Promise.allSettled(
      lista.map((a) => contratosApi.status(a.id).then((r) => ({ id: a.id, assinado: r.assinado })))
    );
    const mapa = {};
    entradas.forEach((r) => {
      if (r.status === 'fulfilled') {
        mapa[r.value.id] = r.value.assinado;
      }
    });
    setStatusMap(mapa);
  }

  async function gerarPdf(atletaId) {
    try {
      await contratosApi.downloadPdf(atletaId);
    } catch {
      setErro('Erro ao gerar PDF.');
    }
  }

  async function registrarAceite(atletaId) {
    try {
      await contratosApi.aceitar(atletaId);
      setAviso('Aceite registrado com sucesso.');
      setStatusMap((prev) => ({ ...prev, [atletaId]: true }));
      setTimeout(() => setAviso(''), 3000);
    } catch {
      setErro('Erro ao registrar aceite.');
    }
  }

  async function salvarTemplate() {
    setSalvandoTemplate(true);
    setErro('');
    try {
      await contratosApi.salvarTemplate(null, template);
      setAviso('Template salvo com sucesso.');
      setTimeout(() => setAviso(''), 3000);
    } catch {
      setErro('Erro ao salvar template.');
    } finally {
      setSalvandoTemplate(false);
    }
  }

  return (
    <div className="pagina">
      <div className="cabecalho">
        <h1>Contratos Digitais</h1>
      </div>

      {aviso && <div className="alerta-info">{aviso}</div>}
      {erro && <div className="alerta-erro">{erro}</div>}

      <div className="tabs" style={{ display: 'flex', gap: 8, marginBottom: 20 }}>
        {[
          { id: 'atletas', label: 'Atletas' },
          { id: 'template', label: 'Template' },
        ].map((t) => (
          <button
            key={t.id}
            className={tab === t.id ? 'btn-primario btn-sm' : 'btn-secundario btn-sm'}
            onClick={() => setTab(t.id)}
          >
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'atletas' && (
        <>
          {carregando ? (
            <p className="carregando">Carregando...</p>
          ) : atletas.length === 0 ? (
            <p className="vazio">Nenhum atleta encontrado.</p>
          ) : (
            <table className="tabela">
              <thead>
                <tr>
                  <th>Nome</th>
                  <th>CPF</th>
                  <th>Status Contrato</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                {atletas.map((atleta) => {
                  const assinado = statusMap[atleta.id];
                  return (
                    <tr key={atleta.id}>
                      <td>{atleta.nomeCompleto}</td>
                      <td>{atleta.cpf}</td>
                      <td>
                        <span
                          style={{
                            display: 'inline-block',
                            padding: '2px 10px',
                            borderRadius: 12,
                            fontSize: '0.78rem',
                            fontWeight: 600,
                            background: assinado ? '#c6f6d5' : '#fed7d7',
                            color: assinado ? '#276749' : '#9b2c2c',
                          }}
                        >
                          {assinado === undefined ? '...' : assinado ? 'Assinado' : 'Pendente'}
                        </span>
                      </td>
                      <td className="acoes">
                        <button
                          className="btn-secundario btn-sm"
                          onClick={() => gerarPdf(atleta.id)}
                        >
                          PDF
                        </button>
                        {!assinado && (
                          <button
                            className="btn-primario btn-sm"
                            onClick={() => registrarAceite(atleta.id)}
                          >
                            Registrar Aceite
                          </button>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          )}
        </>
      )}

      {tab === 'template' && (
        <div style={{ maxWidth: 760 }}>
          <p style={{ fontSize: '0.85rem', color: '#718096', marginBottom: 12 }}>
            Variáveis disponíveis:{' '}
            <code>{'{NomeAtleta}'}</code>, <code>{'{Cpf}'}</code>, <code>{'{NomeAcademia}'}</code>
          </p>
          <textarea
            value={template}
            onChange={(e) => setTemplate(e.target.value)}
            rows={22}
            style={{
              width: '100%',
              fontFamily: 'monospace',
              fontSize: '0.85rem',
              padding: 12,
              border: '1px solid #e2e8f0',
              borderRadius: 6,
              resize: 'vertical',
              lineHeight: 1.6,
            }}
          />
          <div style={{ marginTop: 12, display: 'flex', justifyContent: 'flex-end' }}>
            <button
              className="btn-primario"
              onClick={salvarTemplate}
              disabled={salvandoTemplate}
            >
              {salvandoTemplate ? 'Salvando...' : 'Salvar Template'}
            </button>
          </div>
        </div>
      )}
    </div>
  );
}
