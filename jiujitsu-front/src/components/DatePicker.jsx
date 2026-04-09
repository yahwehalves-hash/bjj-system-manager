import { useEffect, useState } from 'react'

const MESES = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

function parseDate(value) {
  if (!value) return { ano: '', mes: '', dia: '' }
  const s = value.slice(0, 10) // garante YYYY-MM-DD mesmo se vier YYYY-MM-DDTHH:mm:ss
  const [ano = '', mes = '', dia = ''] = s.split('-')
  return { ano, mes, dia }
}

function parseMonth(value) {
  if (!value) return { ano: '', mes: '' }
  const s = value.slice(0, 7) // YYYY-MM
  const [ano = '', mes = ''] = s.split('-')
  return { ano, mes }
}

/** Seletor de data completa (dia/mês/ano). value e onChange em formato YYYY-MM-DD. */
export function DatePicker({ value, onChange, required, minYear, maxYear }) {
  const anoAtual = new Date().getFullYear()
  const min = minYear ?? anoAtual - 100
  const max = maxYear ?? anoAtual

  const [interno, setInterno] = useState(() => parseDate(value))

  // Sincroniza quando o valor externo muda (p.ex. ao carregar dados de edição)
  useEffect(() => {
    setInterno(parseDate(value))
  }, [value])

  const diasNoMes = interno.mes && interno.ano
    ? new Date(Number(interno.ano), Number(interno.mes), 0).getDate()
    : 31

  function atualizar(campo, val) {
    const novo = { ...interno, [campo]: val }
    setInterno(novo)
    // Só emite quando todos os três campos estão preenchidos
    if (novo.ano && novo.mes && novo.dia) {
      onChange(`${novo.ano}-${novo.mes.padStart(2, '0')}-${novo.dia.padStart(2, '0')}`)
    }
  }

  return (
    <div className="datepicker">
      <select value={interno.dia} onChange={e => atualizar('dia', e.target.value)} required={required}>
        <option value="">Dia</option>
        {Array.from({ length: diasNoMes }, (_, i) => i + 1).map(d => (
          <option key={d} value={String(d).padStart(2, '0')}>{d}</option>
        ))}
      </select>
      <select value={interno.mes} onChange={e => atualizar('mes', e.target.value)} required={required}>
        <option value="">Mês</option>
        {MESES.map((m, i) => (
          <option key={i} value={String(i + 1).padStart(2, '0')}>{m}</option>
        ))}
      </select>
      <select value={interno.ano} onChange={e => atualizar('ano', e.target.value)} required={required}>
        <option value="">Ano</option>
        {Array.from({ length: max - min + 1 }, (_, i) => max - i).map(a => (
          <option key={a} value={String(a)}>{a}</option>
        ))}
      </select>
    </div>
  )
}

/** Seletor de mês/ano. value e onChange em formato YYYY-MM. */
export function MonthPicker({ value, onChange, required }) {
  const anoAtual = new Date().getFullYear()

  const [interno, setInterno] = useState(() => parseMonth(value))

  useEffect(() => {
    setInterno(parseMonth(value))
  }, [value])

  function atualizar(campo, val) {
    const novo = { ...interno, [campo]: val }
    setInterno(novo)
    if (novo.ano && novo.mes) {
      onChange(`${novo.ano}-${novo.mes.padStart(2, '0')}`)
    }
  }

  return (
    <div className="datepicker">
      <select value={interno.mes} onChange={e => atualizar('mes', e.target.value)} required={required}>
        <option value="">Mês</option>
        {MESES.map((m, i) => (
          <option key={i} value={String(i + 1).padStart(2, '0')}>{m}</option>
        ))}
      </select>
      <select value={interno.ano} onChange={e => atualizar('ano', e.target.value)} required={required}>
        <option value="">Ano</option>
        {Array.from({ length: 11 }, (_, i) => anoAtual + 2 - i).map(a => (
          <option key={a} value={String(a)}>{a}</option>
        ))}
      </select>
    </div>
  )
}
