import { useEffect, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import ReactApexChart from 'react-apexcharts';
import { atletasApi } from '../api/atletasApi';
import { DatePicker } from '../components/DatePicker';

const COR_FAIXA = {
  Branca: '#e0e0e0',
  Cinza:  '#9e9e9e',
  Azul:   '#1565c0',
  Roxa:   '#6a1b9a',
  Marrom: '#5d4037',
  Preta:  '#212121',
};

const FAIXAS_ORDEM = ['Branca', 'Cinza', 'Azul', 'Roxa', 'Marrom', 'Preta'];
const GRAUS = ['', '1°', '2°', '3°', '4°'];

function formatarData(iso) {
  if (!iso) return '—';
  const [ano, mes, dia] = iso.split('-');
  return `${dia}/${mes}/${ano}`;
}

function formatarDias(dias) {
  if (dias < 30) return `${dias} dia${dias !== 1 ? 's' : ''}`;
  const totalMeses = Math.floor(dias / 30);
  const anos       = Math.floor(totalMeses / 12);
  const meses      = totalMeses % 12;
  if (anos === 0) return `${meses} mês${meses !== 1 ? 'es' : ''}`;
  if (meses === 0) return `${anos} ano${anos !== 1 ? 's' : ''}`;
  return `${anos} ano${anos !== 1 ? 's' : ''} e ${meses} mês${meses !== 1 ? 'es' : ''}`;
}

function dateStringToMs(ds) {
  const [ano, mes, dia] = ds.split('-').map(Number);
  return new Date(ano, mes - 1, dia).getTime();
}

export default function HistoricoPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [historico, setHistorico]         = useState(null);
  const [carregando, setCarregando]       = useState(true);
  const [erro, setErro]                   = useState('');
  const [aviso, setAviso]                 = useState('');

  // Histórico manual
  const [modalManual, setModalManual]     = useState(false);
  const [formManual, setFormManual]       = useState({ faixa: 'Branca', grau: 0, dataInicio: '', dataFim: '' });
  const [salvando, setSalvando]           = useState(false);

  // Foto
  const inputFotoRef = useRef(null);

  useEffect(() => { carregar(); }, [id]);

  async function carregar() {
    setCarregando(true);
    setErro('');
    try {
      const dados = await atletasApi.obterHistorico(id);
      setHistorico(dados);
    } catch {
      setErro('Erro ao carregar histórico. Verifique se a API está em execução.');
    } finally {
      setCarregando(false);
    }
  }

  async function handleFotoChange(e) {
    const file = e.target.files?.[0];
    if (!file) return;

    if (file.size > 512 * 1024) {
      setErro('A foto deve ter no máximo 500 KB.');
      return;
    }

    const reader = new FileReader();
    reader.onload = async () => {
      try {
        await atletasApi.atualizarFoto(id, reader.result);
        setAviso('Foto atualizada com sucesso!');
        await carregar();
        setTimeout(() => setAviso(''), 3000);
      } catch {
        setErro('Erro ao atualizar foto.');
      }
    };
    reader.readAsDataURL(file);
  }

  async function handleSalvarManual() {
    if (!formManual.dataInicio) { setErro('Data de início é obrigatória.'); return; }
    setSalvando(true);
    try {
      await atletasApi.adicionarHistoricoManual(id, {
        faixa:      formManual.faixa,
        grau:       Number(formManual.grau),
        dataInicio: formManual.dataInicio,
        dataFim:    formManual.dataFim || null,
      });
      setModalManual(false);
      setFormManual({ faixa: 'Branca', grau: 0, dataInicio: '', dataFim: '' });
      setAviso('Histórico adicionado com sucesso!');
      await carregar();
      setTimeout(() => setAviso(''), 3000);
    } catch (e) {
      setErro(e?.response?.data?.erro ?? 'Erro ao salvar histórico.');
    } finally {
      setSalvando(false);
    }
  }

  // ---- Gráfico ----
  function buildChartOptions(historico) {
    const hoje = new Date().toISOString().split('T')[0];

    const series = historico.historico.map((h) => ({
      x: `${h.faixa}${h.grau > 0 ? ` ${GRAUS[h.grau]}` : ''}`,
      y: [
        dateStringToMs(h.dataInicio),
        dateStringToMs(h.dataFim ?? hoje),
      ],
    }));

    // colors deve ter uma entrada por barra para o modo distributed
    const colors = historico.historico.map((h) => COR_FAIXA[h.faixa] ?? '#ccc');

    const options = {
      chart: { type: 'rangeBar', toolbar: { show: false }, background: 'transparent' },
      colors,
      plotOptions: {
        bar: {
          horizontal: true,
          distributed: true,
          dataLabels: { hideOverflowingLabels: false },
          barHeight: '60%',
          borderRadius: 4,
        },
      },
      xaxis: {
        type: 'datetime',
        labels: {
          datetimeFormatter: { year: 'yyyy', month: "MMM'yy" },
          style: { colors: '#718096', fontSize: '11px' },
        },
      },
      yaxis: {
        labels: { style: { colors: '#2d3748', fontWeight: 600, fontSize: '12px' } },
      },
      dataLabels: {
        enabled: true,
        formatter: (_, opts) => {
          const h = historico.historico[opts.dataPointIndex];
          return formatarDias(h.diasNaGraduacao);
        },
        style: { fontSize: '11px', colors: ['#fff'] },
      },
      tooltip: {
        custom: ({ dataPointIndex }) => {
          const h = historico.historico[dataPointIndex];
          const hoje2 = new Date().toISOString().split('T')[0];
          return `<div style="padding:8px 12px;font-size:13px">
            <b>${h.faixa}${h.grau > 0 ? ` — ${GRAUS[h.grau]} grau` : ''}</b><br/>
            Início: ${formatarData(h.dataInicio)}<br/>
            Fim: ${h.dataFim ? formatarData(h.dataFim) : 'Atual'}<br/>
            Tempo: ${formatarDias(h.diasNaGraduacao)}
          </div>`;
        },
      },
      legend: { show: false },
      grid: { borderColor: '#e2e8f0' },
    };

    return { options, series: [{ data: series }] };
  }

  if (carregando) return <div className="pagina"><p className="carregando">Carregando histórico...</p></div>;
  if (erro && !historico) return <div className="pagina"><div className="alerta-erro">{erro}</div></div>;
  if (!historico) return <div className="pagina"><p className="vazio">Histórico não encontrado.</p></div>;

  const chart = buildChartOptions(historico);
  const chartHeight = Math.max(200, historico.historico.length * 60 + 60);
  const faixaAtualCor = COR_FAIXA[historico.faixaAtual] ?? '#ccc';

  return (
    <div className="pagina">
      <div className="cabecalho">
        <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
          <button className="btn-secundario btn-sm" onClick={() => navigate('/atletas')}>← Voltar</button>
          <h1>Histórico de Graduação</h1>
        </div>
        <button className="btn-secundario" onClick={() => setModalManual(true)}>
          + Adicionar histórico manual
        </button>
      </div>

      {aviso && <div className="alerta-info">{aviso}</div>}
      {erro   && <div className="alerta-erro">{erro}</div>}

      {/* Card do atleta */}
      <div style={{ display: 'flex', gap: 24, marginBottom: 28, flexWrap: 'wrap' }}>
        {/* Foto */}
        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 8 }}>
          <div
            style={{
              width: 100, height: 100, borderRadius: '50%', overflow: 'hidden',
              border: `3px solid ${faixaAtualCor}`,
              background: '#e2e8f0', display: 'flex', alignItems: 'center', justifyContent: 'center',
              cursor: 'pointer', flexShrink: 0,
            }}
            onClick={() => inputFotoRef.current?.click()}
            title="Clique para alterar a foto"
          >
            {historico.fotoBase64 ? (
              <img src={historico.fotoBase64} alt="Foto do atleta" style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
            ) : (
              <span style={{ fontSize: 36, color: '#a0aec0' }}>👤</span>
            )}
          </div>
          <span style={{ fontSize: '0.75rem', color: '#718096' }}>Clique para alterar</span>
          <input ref={inputFotoRef} type="file" accept="image/*" style={{ display: 'none' }} onChange={handleFotoChange} />
        </div>

        {/* Info */}
        <div style={{ flex: 1 }}>
          <h2 style={{ fontSize: '1.3rem', fontWeight: 700, marginBottom: 6 }}>{historico.nomeCompleto}</h2>
          <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 12 }}>
            <FaixaVisual faixa={historico.faixaAtual} grau={historico.grauAtual} />
          </div>
          <div style={{ display: 'flex', gap: 24, flexWrap: 'wrap' }}>
            <CardResumo label="Faixa atual" valor={`${historico.faixaAtual}${historico.grauAtual > 0 ? ` — ${GRAUS[historico.grauAtual]} grau` : ''}`} />
            <CardResumo label="Tempo total na arte" valor={formatarDias(historico.totalDiasNaArte)} destaque />
            <CardResumo label="Graduações" valor={`${historico.historico.length}`} />
          </div>
        </div>
      </div>

      {historico.historico.length === 0 ? (
        <div style={{ textAlign: 'center', padding: '40px 0', color: '#718096' }}>
          <p>Nenhum histórico registrado ainda.</p>
          <p style={{ fontSize: '0.875rem' }}>Ao graduar o atleta, o histórico será gerado automaticamente.</p>
        </div>
      ) : (
        <>
          {/* Gráfico timeline */}
          <div className="card" style={{ marginBottom: 24 }}>
            <h3 style={{ fontSize: '0.95rem', fontWeight: 700, marginBottom: 16, color: '#2d3748' }}>
              Linha do Tempo
            </h3>
            <ReactApexChart
              type="rangeBar"
              options={chart.options}
              series={chart.series}
              height={chartHeight}
            />
          </div>

          {/* Tabela */}
          <div className="card">
            <h3 style={{ fontSize: '0.95rem', fontWeight: 700, marginBottom: 16, color: '#2d3748' }}>
              Detalhamento por Graduação
            </h3>
            <table className="tabela">
              <thead>
                <tr>
                  <th>Faixa</th>
                  <th>Grau</th>
                  <th>Início</th>
                  <th>Fim</th>
                  <th>Tempo</th>
                </tr>
              </thead>
              <tbody>
                {historico.historico.map((h) => (
                  <tr key={h.id}>
                    <td>
                      <div className="faixa-cell">
                        <span
                          className="faixa-barra"
                          style={{
                            background: `linear-gradient(180deg, ${COR_FAIXA[h.faixa] ?? '#ccc'} 75%, ${ponteira(h.faixa)} 75%)`,
                            border: h.faixa === 'Branca' ? '1px solid #ccc' : 'none',
                          }}
                        />
                        {h.faixa}
                      </div>
                    </td>
                    <td>{h.grau > 0 ? GRAUS[h.grau] : '—'}</td>
                    <td>{formatarData(h.dataInicio)}</td>
                    <td>{h.dataFim ? formatarData(h.dataFim) : <span style={{ color: '#38a169', fontWeight: 600 }}>Atual</span>}</td>
                    <td>{formatarDias(h.diasNaGraduacao)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}

      {/* Modal histórico manual */}
      {modalManual && (
        <div className="overlay">
          <div className="dialog" style={{ maxWidth: 460 }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
              <h2 style={{ fontSize: '1.1rem', fontWeight: 700 }}>Adicionar Histórico Manual</h2>
              <button className="btn-secundario btn-sm" onClick={() => setModalManual(false)}>Fechar</button>
            </div>

            <div className="form-grupo">
              <label>Faixa</label>
              <select
                className="form-input"
                value={formManual.faixa}
                onChange={(e) => setFormManual((f) => ({ ...f, faixa: e.target.value }))}
              >
                {FAIXAS_ORDEM.map((f) => <option key={f} value={f}>{f}</option>)}
              </select>
            </div>

            <div className="form-grupo">
              <label>Grau</label>
              <select
                className="form-input"
                value={formManual.grau}
                onChange={(e) => setFormManual((f) => ({ ...f, grau: e.target.value }))}
              >
                <option value={0}>Sem grau</option>
                <option value={1}>1° grau</option>
                <option value={2}>2° grau</option>
                <option value={3}>3° grau</option>
                <option value={4}>4° grau</option>
              </select>
            </div>

            <div className="form-grupo">
              <label>Data de início *</label>
              <DatePicker
                value={formManual.dataInicio}
                onChange={(v) => setFormManual((f) => ({ ...f, dataInicio: v }))}
                maxYear={new Date().getFullYear()}
              />
            </div>

            <div className="form-grupo">
              <label>Data de fim <span style={{ color: '#718096', fontSize: '0.8rem' }}>(deixe vazio se for a faixa atual)</span></label>
              <DatePicker
                value={formManual.dataFim}
                onChange={(v) => setFormManual((f) => ({ ...f, dataFim: v }))}
                maxYear={new Date().getFullYear()}
              />
            </div>

            {erro && <div className="alerta-erro" style={{ marginBottom: 12 }}>{erro}</div>}

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 10, marginTop: 20 }}>
              <button className="btn-secundario" onClick={() => setModalManual(false)}>Cancelar</button>
              <button className="btn-primario" onClick={handleSalvarManual} disabled={salvando}>
                {salvando ? 'Salvando...' : 'Salvar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function ponteira(faixa) {
  return faixa === 'Preta' ? '#c62828' : '#1a1a1a';
}

function FaixaVisual({ faixa, grau }) {
  const cor = COR_FAIXA[faixa] ?? '#ccc';
  const tip = ponteira(faixa);
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
      <div style={{
        width: 56, height: 14, borderRadius: 3,
        background: `linear-gradient(90deg, ${cor} 78%, ${tip} 78%)`,
        border: faixa === 'Branca' ? '1px solid #ccc' : 'none',
        boxShadow: '0 1px 3px rgba(0,0,0,0.2)',
      }} />
      <span style={{ fontWeight: 600, color: '#2d3748' }}>
        {faixa}{grau > 0 ? ` — ${GRAUS[grau]} grau` : ''}
      </span>
    </div>
  );
}

function CardResumo({ label, valor, destaque }) {
  return (
    <div style={{
      background: destaque ? '#ebf8ff' : '#f7fafc',
      border: `1px solid ${destaque ? '#bee3f8' : '#e2e8f0'}`,
      borderRadius: 8, padding: '10px 16px', minWidth: 140,
    }}>
      <div style={{ fontSize: '0.75rem', color: '#718096', marginBottom: 2 }}>{label}</div>
      <div style={{ fontSize: '1rem', fontWeight: 700, color: destaque ? '#2b6cb0' : '#2d3748' }}>{valor}</div>
    </div>
  );
}
