import { useEffect, useRef, useState } from 'react'
import { createPortal } from 'react-dom'
import { DayPicker } from 'react-day-picker'
import { format, parse, isValid } from 'date-fns'
import { ptBR } from 'date-fns/locale'
import 'react-day-picker/dist/style.css'

const MESES = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

function strToDate(value) {
  if (!value) return undefined
  const d = parse(value.slice(0, 10), 'yyyy-MM-dd', new Date())
  return isValid(d) ? d : undefined
}

function dateToStr(d) {
  return d ? format(d, 'yyyy-MM-dd') : ''
}

function CalendarioPortal({ anchorRef, children }) {
  const [style, setStyle] = useState({})

  useEffect(() => {
    if (!anchorRef.current) return
    const rect = anchorRef.current.getBoundingClientRect()
    const spaceBelow = window.innerHeight - rect.bottom
    const popupHeight = 320
    const popupWidth = 280
    const margin = 8

    const top = spaceBelow < popupHeight
      ? rect.top - popupHeight - 4
      : rect.bottom + 4

    const rawLeft = rect.left
    const left = Math.min(rawLeft, window.innerWidth - popupWidth - margin)

    setStyle({
      position: 'fixed',
      top,
      left: Math.max(margin, left),
      zIndex: 9999,
    })
  }, [anchorRef])

  return createPortal(
    <div className="calendario-popup" style={style}>{children}</div>,
    document.body
  )
}

/** Seletor de data completa com calendário popup. value e onChange em formato YYYY-MM-DD. */
export function DatePicker({ value, onChange, required, minYear, maxYear }) {
  const anoAtual   = new Date().getFullYear()
  const fromYear   = minYear ?? anoAtual - 100
  const toYear     = maxYear ?? anoAtual + 2

  const [aberto, setAberto]   = useState(false)
  const [mes, setMes]         = useState(strToDate(value) ?? new Date())
  const ref                   = useRef(null)

  const selecionado = strToDate(value)

  useEffect(() => {
    function handler(e) {
      if (ref.current && !ref.current.contains(e.target)) setAberto(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [])

  function handleSelect(dia) {
    if (!dia) return
    onChange(dateToStr(dia))
    setAberto(false)
  }

  const exibicao = selecionado
    ? format(selecionado, 'dd/MM/yyyy')
    : ''

  return (
    <div ref={ref} style={{ position: 'relative', display: 'inline-block', width: '100%' }}>
      <input
        readOnly
        required={required}
        className="input"
        value={exibicao}
        placeholder="dd/mm/aaaa"
        onClick={() => setAberto((a) => !a)}
        style={{ cursor: 'pointer', caretColor: 'transparent' }}
      />
      {aberto && (
        <CalendarioPortal anchorRef={ref}>
          <DayPicker
            mode="single"
            selected={selecionado}
            onSelect={handleSelect}
            month={mes}
            onMonthChange={setMes}
            locale={ptBR}
            captionLayout="dropdown"
            fromYear={fromYear}
            toYear={toYear}
          />
        </CalendarioPortal>
      )}
    </div>
  )
}

/** Seletor de intervalo de datas com calendário duplo. Retorna { from, to } em YYYY-MM-DD. */
export function DateRangePicker({ from, to, onChangefrom, onChangeTo, placeholderFrom = 'Data início', placeholderTo = 'Data fim' }) {
  const [aberto, setAberto] = useState(false)
  const [range, setRange]   = useState({
    from: strToDate(from),
    to:   strToDate(to),
  })
  const ref = useRef(null)

  useEffect(() => {
    setRange({ from: strToDate(from), to: strToDate(to) })
  }, [from, to])

  useEffect(() => {
    function handler(e) {
      if (ref.current && !ref.current.contains(e.target)) setAberto(false)
    }
    document.addEventListener('mousedown', handler)
    return () => document.removeEventListener('mousedown', handler)
  }, [])

  function handleSelect(novoRange) {
    setRange(novoRange ?? { from: undefined, to: undefined })
    if (novoRange?.from) onChangefrom(dateToStr(novoRange.from))
    if (novoRange?.to)   { onChangeTo(dateToStr(novoRange.to)); setAberto(false) }
    if (!novoRange?.from) { onChangefrom(''); onChangeTo('') }
  }

  const exibicao = range?.from
    ? range?.to
      ? `${format(range.from, 'dd/MM/yyyy')} — ${format(range.to, 'dd/MM/yyyy')}`
      : `${format(range.from, 'dd/MM/yyyy')} — ...`
    : ''

  return (
    <div ref={ref} style={{ position: 'relative', display: 'inline-block', width: '100%' }}>
      <input
        readOnly
        className="input"
        value={exibicao}
        placeholder={`${placeholderFrom} — ${placeholderTo}`}
        onClick={() => setAberto((a) => !a)}
        style={{ cursor: 'pointer', caretColor: 'transparent' }}
      />
      {aberto && (
        <CalendarioPortal anchorRef={ref}>
          <div style={{ minWidth: 560 }}>
            <DayPicker
              mode="range"
              selected={range}
              onSelect={handleSelect}
              locale={ptBR}
              numberOfMonths={2}
              captionLayout="dropdown"
              fromYear={2010}
              toYear={new Date().getFullYear() + 1}
            />
          </div>
        </CalendarioPortal>
      )}
    </div>
  )
}

/** Seletor de mês/ano (dropdowns — uso em filtros de competência). value e onChange em formato YYYY-MM. */
export function MonthPicker({ value, onChange, required }) {
  const anoAtual = new Date().getFullYear()

  const [mes, setMes]   = useState(value ? value.slice(5, 7) : '')
  const [ano, setAno]   = useState(value ? value.slice(0, 4) : '')

  useEffect(() => {
    if (value) { setMes(value.slice(5, 7)); setAno(value.slice(0, 4)) }
  }, [value])

  function atualizar(campo, val) {
    const novoMes = campo === 'mes' ? val : mes
    const novoAno = campo === 'ano' ? val : ano
    if (novoMes && novoAno) onChange(`${novoAno}-${novoMes.padStart(2, '0')}`)
    if (campo === 'mes') setMes(val)
    if (campo === 'ano') setAno(val)
  }

  return (
    <div className="datepicker">
      <select value={mes} onChange={e => atualizar('mes', e.target.value)} required={required}>
        <option value="">Mês</option>
        {MESES.map((m, i) => (
          <option key={i} value={String(i + 1).padStart(2, '0')}>{m}</option>
        ))}
      </select>
      <select value={ano} onChange={e => atualizar('ano', e.target.value)} required={required}>
        <option value="">Ano</option>
        {Array.from({ length: 11 }, (_, i) => anoAtual + 2 - i).map(a => (
          <option key={a} value={String(a)}>{a}</option>
        ))}
      </select>
    </div>
  )
}
